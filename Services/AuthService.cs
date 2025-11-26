using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Threading.Tasks;
using Travel_Journal.Data;
using Travel_Journal.Email;
using Travel_Journal.Models;
using Travel_Journal.UIServices;

namespace Travel_Journal.Services
{
    // En tjänst för autentisering och kontohantering
    public class AuthService
    {
        private readonly TwoFactorService _twoFactor;

        public AuthService()
        {
            // Standard: SMTP via miljövariabler
            _twoFactor = new TwoFactorService(new SmtpEmailSender());
            SeedAdmin();
        }
        // Skapar en standard admin-användare om ingen finns
        private void SeedAdmin()
        {
            var all = AccountStore.GetAll();
            if (all.Any(all => all.IsAdmin))
                return;
            var acc = new Account
            {
                UserName = "Admin",
                Password = "Admin123!", // helst byt till något från config
                Email = "admin@example.com",
                EmailVerified = false,      // du kan köra din vanliga verifieringsprocess sen om du vill
                TwoFactorEnabled = false,
                CreatedAt = DateTime.UtcNow,
                IsAdmin = true
            };
            AccountStore.Add(acc);
            AccountStore.Save();
        }

        // === NY === Registrering med e-postverifiering (Spectre + 2FA-val)
        public async Task<bool> RegisterWithEmailVerificationAsync(string username, string password)
        {
            if (AccountStore.Exists(username))
            {
                UI.Warn("User already exists!");
                Logg.Log($"Attempted registration with existing username {username}");
                return false;
            }

            var acc = new Account();

            if (!acc.CheckUserName(username))
            {
                UI.Error("Username must be at least 1 character long.");
                Logg.Log($"User {username} trying to register account with less than 1 character");
                return false;
            }
            if (!acc.CheckPassword(password))
            {
                UI.Error("Password requirements: min 6 chars, uppercase, lowercase, number, special.");
                Logg.Log($"Password for {username} doesnt meet the requirements");
                return false;
            }

            var email = AnsiConsole.Prompt(
                new TextPrompt<string>("[cyan]Enter your email for verification:[/] ")
                    .Validate(input =>
                        string.IsNullOrWhiteSpace(input) || !input.Contains("@")
                            ? ValidationResult.Error("[red]Please enter a valid email[/]")
                            : ValidationResult.Success())
            );

            acc.UserName = username;
            acc.Password = password;
            acc.Email = email;
            acc.EmailVerified = false;
            acc.TwoFactorEnabled = false;
            acc.CreatedAt = DateTime.UtcNow;

            // Skicka verifikationskod (wrappar utan WithStatusAsync)
            bool sent = false;
            UI.WithStatus("Sending verification email...", () =>
            {
                try
                {
                    // blockera sync i status-lambdan (ingen await här)
                    sent = _twoFactor.SendEmailCodeAsync(acc, "Verify your email")
                                     .GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    UI.Error("Failed to send verification email: " + ex.Message);
                    Logg.Log("Failed to send verification email");
                    sent = false;
                }
            });

            if (!sent) return false;

            bool verified = false;
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                var code = AnsiConsole.Prompt(
                    new TextPrompt<string>($"[yellow]Enter the 6-digit code (attempt {attempt}/3):[/]")
                        .PromptStyle("white")
                );

                if (_twoFactor.VerifyCode(acc, code))
                {
                    verified = true;
                    break;
                }
                UI.Warn("Wrong code.");
                Logg.Log("Wrong verification-code");
            }

            if (!verified)
            {
                UI.Error("Email not verified.");
                Logg.Log("Email not verified.");
                _twoFactor.ClearPending(acc);
                return false;
            }

            acc.EmailVerified = true;
            _twoFactor.ClearPending(acc);

            UI.WithStatus("Saving account...", () =>
            {
                AccountStore.Add(acc);
                AccountStore.Save();
            });

            var enable2fa = AnsiConsole.Confirm("Enable email 2FA for login?");
            if (enable2fa)
            {
                acc.TwoFactorEnabled = true;
                AccountStore.Update(acc);
                AccountStore.Save();
            }

            var panel = new Panel($"""
            [green]✅ User {acc.UserName} created & email verified![/]
            [grey]2FA enabled:[/] {(acc.TwoFactorEnabled ? "[green]Yes[/]" : "[yellow]No[/]")}
            """)
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader("Registration Successful", Justify.Center)
            };
            AnsiConsole.Write(panel);

            return true;
        }

        // === UPPDATERAD === Inloggning med villkorlig 2FA (endast EN definition)
        public Account? Login(string username, string password)
        {
            var acc = AccountStore.Get(username);

            if (acc == null)
            {
                UI.Error("Unknown username.");
                Logg.Log($"Login failed: Unknown User ´{username}´");//Logg för Unknown User
                return null;
            }

            if (!(acc.UserName == username && acc.Password == password))
            {
                UI.Error("Wrong username or password.");
                Logg.Log($"Login failed:Wrong password for ´{username}`"); //Logg för fel password
                return null; 
            }

            // Kräver 2FA?
            if (acc.TwoFactorEnabled)
            {
                if (!acc.EmailVerified || string.IsNullOrWhiteSpace(acc.Email))
                {
                    UI.Warn("2FA is enabled but email is not verified. Disabling 2FA for safety.");
                    acc.TwoFactorEnabled = false;
                    AccountStore.Update(acc);
                    AccountStore.Save();
                    UI.Success($"Logged in as [bold]{username}[/]! ✈️");
                    Logg.Log($"User ´{username}´ logged in"); //logg för lyckad inloggning utan 2FA fördjupning
                    return acc;
                }

                bool sent = false;
                UI.WithStatus("Sending login code...", () =>
                {
                    try
                    {
                        sent = _twoFactor.SendEmailCodeAsync(acc, "Login-code")
                                         .GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        UI.Error("Email send error: " + ex.Message);
                        Logg.Log($"[Email Error] Failed to send email: {ex.Message}\n{ex.StackTrace}");
                        sent = false;
                    }
                });

                if (!sent) return null;

                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    var code = AnsiConsole.Prompt(
                        new TextPrompt<string>($"[yellow]Enter the 6-digit code (attempt {attempt}/3):[/] ")
                    );

                    if (_twoFactor.VerifyCode(acc, code))
                    {
                        _twoFactor.ClearPending(acc);
                        UI.Success($"Logged in as [bold]{username}[/]! ✈️");
                        return acc;
                    }
                    // Logga BARA felförsök
                    Logg.Log($"2FA failed attempt {attempt}/3 for user '{username}'.");
                }

                UI.Error("Too many attempts.");
                Logg.Log("Too many 2FA code attempts.");
                _twoFactor.ClearPending(acc);
                return null;
            }

            // Ingen 2FA
            UI.Success($"Logged in as [bold]{username}[/]! ✈️");
            return acc;
        }

        // === EN (1) definition av ResetPassword ===
        public async Task ForgotPasswordAsync()
        {
            AnsiConsole.MarkupLine("[yellow]--- Reset Password ---[/]");

            // 1. Hitta användaren
            var username = UI.AskWithBack("Enter your username");
            var acc = AccountStore.Get(username);

            if (acc == null)
            {
                UI.Error("Unknown username.");
                return;
            }

            if (string.IsNullOrEmpty(acc.Email))
            {
                UI.Error("No email connected to this account. Cannot reset password.");
                return;
            }

            // 2. Skicka verifieringskod (Samma logik som vid 2FA)
            bool sent = false;
            await UI.WithStatusAsync("Sending verification code...", async () =>
            {
                // Här anropas metoden som genererar koden, hashar den och skickar mailet
                sent = await _twoFactor.SendEmailCodeAsync(acc, "Password Reset Code");
            });

            if (!sent)
            {
                UI.Error("Failed to send email. Please try again later.");
                return;
            }

            // VIKTIGT: Spara kontot så att den nya hashen (koden) sparas i "databasen"
            AccountStore.Update(acc);
            AccountStore.Save();

            UI.Success($"A verification code has been sent to the email you registered.");

            // 3. Verifiera koden
            bool verified = false;
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                var code = AnsiConsole.Prompt(
                    new TextPrompt<string>($"[yellow]Enter the 6-digit code (attempt {attempt}/3):[/] ")
                );

                // VerifyCode kollar input mot den hash som precis sparades
                if (_twoFactor.VerifyCode(acc, code))
                {
                    verified = true;
                    _twoFactor.ClearPending(acc); // Rensa koden så den inte kan användas igen
                    break;
                }
                UI.Warn("Wrong code.");
            }

            if (!verified)
            {
                UI.Error("Too many failed attempts. Password reset cancelled.");
                return;
            }

            // 4. Byt lösenord
            string newPass; // Vi deklarerar variabeln utanför loopen så vi kan använda den efteråt

            while (true)
            {
                newPass = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter new password:").Secret()
                );

                var confirmPass = AnsiConsole.Prompt(
                    new TextPrompt<string>("Confirm new password:").Secret()
                );

                // Kontroll 1: Matchar lösenorden?
                if (newPass != confirmPass)
                {
                    UI.Error("Passwords do not match. Please try again.");
                    continue; // Hoppar tillbaka till början av while-loopen
                }

                // Kontroll 2: Uppfyller lösenordet kraven?
                if (!acc.CheckPassword(newPass))
                {
                    UI.Error("Password requirements not met (min 6 chars, upper, lower, number, special).");
                    continue; // Hoppar tillbaka till början av while-loopen
                }

                // Om vi kommer hit är allt korrekt!
                break; // Bryter loopen och fortsätter koden nedanför
            }

            UI.WithStatus("Saving new password...", () =>
            {
                acc.Password = newPass;
                // Generera en ny recovery code så användaren har en fräsch
                acc.RecoveryCode = Util.GenerateRecoveryCode();

                AccountStore.Update(acc);
                AccountStore.Save();
            });

            UI.Success("Password successfully reset! You can now login.");
            Logg.Log($"User {acc.UserName} reset their password via email verification.");
        }
    }
}
