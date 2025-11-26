using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Threading.Tasks;
using Travel_Journal.Data;
using Travel_Journal.Interfaces;
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
            if (all.Any(a => a.IsAdmin))
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
        public async Task<bool> RegisterWithEmailVerificationAsync()
        {
            UI.Transition("Register Account");

            var acc = new Account(); // Skapa instansen tidigt för att använda valideringsmetoderna
            string? username;

            // === LOOP 1: Användarnamn ===
            // Vi loopar tills vi får ett unikt och giltigt namn eller användaren backar
            while (true)
            {
                username = UI.AskWithBack("Username");
                if (username == null) return false; // Användaren valde [Back]

                // 1. Kolla om det redan finns
                if (AccountStore.Exists(username))
                {
                    UI.Warn($"The username '{username}' is already taken. Please try another.");
                    continue; // Börja om loopen
                }

                // 2. Kolla format (t.ex. längd)
                if (!acc.CheckUserName(username))
                {
                    UI.Error("Username must be at least 1 character long.");
                    continue; // Börja om loopen
                }

                // Om vi kommer hit är namnet OK!
                break;
            }

            // === LOOP 2: Lösenord ===
            // Här är fixen du bad om: Vi loopar tills lösenordet är starkt nog
            string password;
            while (true)
            {
                password = AnsiConsole.Prompt(
                    new TextPrompt<string>("Password:")
                        .Secret()
                );

                if (acc.CheckPassword(password))
                {
                    break; // Lösenordet är godkänt, gå vidare
                }

                // Om lösenordet är för svagt:
                UI.Error("Weak password! Requirements: min 6 chars, Uppercase, Lowercase, Number, Special.");
                Logg.Log($"User {username} failed password validation.");
                // Loopen körs automatiskt igen härifrån
            }

            // === 3. E-post (Spectre.Console .Validate sköter loopen här) ===
            var email = AnsiConsole.Prompt(
                new TextPrompt<string>("[cyan]Enter your email for verification:[/] ")
                    .Validate(input =>
                        string.IsNullOrWhiteSpace(input) || !input.Contains("@")
                            ? ValidationResult.Error("[red]Please enter a valid email[/]")
                            : ValidationResult.Success())
            );

            // Sätt data på kontot
            acc.UserName = username;
            acc.Password = password;
            acc.Email = email;
            acc.EmailVerified = false;
            acc.TwoFactorEnabled = false;
            acc.CreatedAt = DateTime.UtcNow;

            // === 4. Skicka verifikationskod ===
            bool sent = false;
            UI.WithStatus("Sending verification email...", () =>
            {
                try
                {
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

            // === 5. Verifiera kod (Loop 3 försök) ===
            bool verified = false;
            // Loop för att skriva in koden (3 försök) 
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                // Fråga efter koden
                var code = AnsiConsole.Prompt(
                    new TextPrompt<string>($"[yellow]Enter the 6-digit code (attempt {attempt}/3):[/]")
                        .PromptStyle("white")
                );

                // Kontrollera koden mot den sparade hashen i TwoFactorService
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

            // Spara konto
            UI.WithStatus("Saving account...", () =>
            {
                AccountStore.Add(acc);
                AccountStore.Save();
            });

            // Fråga om 2FA vid inloggning
            var enable2fa = AnsiConsole.Confirm("Enable email 2FA for login?");
            if (enable2fa)
            {
                acc.TwoFactorEnabled = true;
                AccountStore.Update(acc);
                AccountStore.Save();
            }

            // Visa framgång
            var panel = new Panel($"""
    [green]✅ User {acc.UserName} created & email verified![/]
    [grey]2FA enabled:[/] {(acc.TwoFactorEnabled ? "[green]Yes[/]" : "[yellow]No[/]")}
    """)
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader("Registration Successful", Justify.Center)
            };
            AnsiConsole.Write(panel);
            UI.Pause();

            return true;
        }

        // Metod för inloggning 
        public Account? Login()
        {
            UI.Transition("Login"); // Visa rubrik

            Account? acc = null;
            string? username = null;

            // === LOOP 1: Hitta användaren ===
            while (true)
            {
                // Fråga efter användarnamn (med möjlighet att backa)
                username = UI.AskWithBack("Username");

                // Om användaren väljer att backa
                if (username == null) return null;

                // Försök hämta kontot
                acc = AccountStore.Get(username);

                // Om kontot hittades -> Gå vidare till lösenord
                if (acc != null)
                {
                    break;
                }

                // Annars, visa fel och loopa igen
                UI.Error("Unknown username.");
                Logg.Log($"Login failed: Unknown User '{username}'");
            }

            // === LOOP 2: Kontrollera lösenord ===
            // Här loopar vi tills användaren skriver RÄTT lösenord.
            while (true)
            {
                // Fråga efter lösenord
                var password = AnsiConsole.Prompt(
                    new TextPrompt<string>("Password:")
                        .Secret()
                );

                // Kontrollera om lösenordet stämmer
                if (acc.Password == password)
                {
                    break; // Rätt lösenord! Gå vidare till 2FA/Inloggning
                }

                // Fel lösenord -> Visa fel och låt loopen köra igen
                UI.Error("Wrong password.");
                Logg.Log($"Login failed: Wrong password for '{acc.UserName}'");
            }

            // === 2FA Logik (Oförändrad, men använder nu 'acc' vi hämtade ovan) ===

            // Kräver 2FA?
            if (acc.TwoFactorEnabled)
            {
                if (!acc.EmailVerified || string.IsNullOrWhiteSpace(acc.Email))
                {
                    UI.Warn("2FA is enabled but email is not verified. Disabling 2FA for safety.");
                    acc.TwoFactorEnabled = false;
                    AccountStore.Update(acc);
                    AccountStore.Save();

                    // Logga in direkt
                    UI.Success($"Logging in as [bold]{acc.UserName}[/]! ✈️");
                    Logg.Log($"User '{acc.UserName}' logged in (2FA disabled by system)");
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

                if (!sent) return null; // Om mail misslyckas, avbryt inloggning

                // Loop för att skriva in koden (3 försök)
                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    var code = AnsiConsole.Prompt(
                        new TextPrompt<string>($"[yellow]Enter the 6-digit code (attempt {attempt}/3):[/] ")
                    );

                    if (_twoFactor.VerifyCode(acc, code))
                    {
                        _twoFactor.ClearPending(acc);
                        UI.Success($"Logged in as [bold]{acc.UserName}[/]! ✈️");
                        return acc;
                    }
                    // Logga BARA felförsök
                    Logg.Log($"2FA failed attempt {attempt}/3 for user '{acc.UserName}'.");
                    UI.Warn("Wrong code.");
                }

                UI.Error("Too many attempts.");
                Logg.Log("Too many 2FA code attempts.");
                _twoFactor.ClearPending(acc);
                return null;
            }
            // Returnera kontot vid lyckad inloggning utan 2FA
            return acc;
        }

        // === Glömt Lösenord ===
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

            // 2. Skicka verifieringskod
            bool sent = false;
            UI.WithStatus("Sending verification code...", () =>
            {
                try
                {
                    // Vi tvingar den asynkrona metoden att köras synkront inuti status-snurran
                    sent = _twoFactor.SendEmailCodeAsync(acc, "Password Reset Code")
                                   .GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    UI.Error("Failed to send email: " + ex.Message);
                    Logg.Log($"Failed to send reset email to {username}: {ex.Message}");
                    sent = false;
                }
            });

            if (!sent)
            {
                // Felmeddelande skrevs redan ut i catch-blocket ovan
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

                AccountStore.Update(acc);
                AccountStore.Save();
            });

            UI.Success("Password successfully reset! You can now login.");
            Logg.Log($"User {acc.UserName} reset their password via email verification.");
            UI.Pause();
        }
    }
}
