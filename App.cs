using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using System.IO;
using System.Text.Json;
using Spectre.Console.Cli;

namespace Travel_Journal
{
    public static class App
    {
        public static void Run()
        {
            // Visar en startskärm / splash-animation
            UI.Splash();

            // Laddar användardata med en progressbar
            AccountStore.LoadWithProgress();

            // Huvudloop för programmet – kör tills användaren väljer att avsluta
            while (true)
            {
                // Visar huvudmenyn och sparar användarens val
                var choice = UI.MainMenu();

                // Hanterar menyvalet
                if (choice == "Register")
                    Register(); // Registrera nytt konto
                else if (choice == "Login")
                    Login(); // Logga in på befintligt konto
                else if (choice == "Forgot password")
                    ForgotPassword(); // Glömt lösenord
                else
                {
                    // Om användaren väljer att avsluta programmet
                    UI.Transition("Exiting...");
                    AnsiConsole.MarkupLine("[green]Thank you for using the program![/]");
                    AnsiConsole.MarkupLine("[grey]Have a great day! [/]");
                    System.Threading.Thread.Sleep(1200);
                    return; // Avslutar programmet
                }
            }
        }

        // -------- Register --------
        private static void Register()
        {
            // Visar en övergångsanimation med rubrik
            UI.Transition("Register account");

            // Frågar efter användarnamn
            var username = AnsiConsole.Ask<string>("Username:");

            // Kollar om användaren redan finns
            if (AccountStore.Exists(username))
            {
                UI.Warn("User already exists!");
                return;
            }

            // Frågar efter lösenord (dolt)
            var password = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

            // Skapar nytt kontoobjekt
            var acc = new Account();

            // Försöker registrera kontot, kontrollerar lösenordets giltighet
            if (!acc.Register(username, password))
            {
                UI.Error("Password requirements: at least 6 characters, one uppercase, one lowercase, one number, one special character.");
                return;
            }

            // Genererar återställningskod och sätter skapelsedatum
            acc.RecoveryCode = Util.GenerateRecoveryCode();
            acc.CreatedAt = DateTime.UtcNow;

            // Sparar kontot med status-animation
            UI.WithStatus("Saving account...", () =>
            {
                AccountStore.Add(acc);
                AccountStore.Save();
            });

            // Skapar en grid-layout för att snyggt visa återställningskod
            var grid = new Grid();
            grid.AddColumn();
            grid.AddRow(new Markup($"User [bold]{acc.UserName}[/] created."));
            grid.AddRow(new Markup("[yellow]IMPORTANT:[/] Save your [bold]recovery code[/] for forgotten password!"));
            grid.AddRow(new Markup($"[bold green]{acc.RecoveryCode}[/]"));

            // Skapar en panel runt texten med rubrik
            var panel = new Panel(grid)
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader("Registration Succesful", Justify.Center)
            };

            // Skriver ut panelen i terminalen
            AnsiConsole.Write(panel);
        }

        // -------- Login --------
        private static void Login()
        {
            // Visar övergång till inloggningsskärm
            UI.Transition("Log in");

            // Frågar efter användarnamn och lösenord (dolt)
            var username = AnsiConsole.Ask<string>("Username:");
            var password = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

            // Hämtar kontot från AccountStore
            var acc = AccountStore.Get(username);

            // Om inget konto hittas, visa felmeddelande
            if (acc == null)
            {
                UI.Error("Unknown username");
                return;
            }

            // Variabel för att spara resultatet av inloggningen
            var ok = false;

            // Visar status-animation under verifiering
            UI.WithStatus("Verifying...", () =>
            {
                ok = acc.Login(username, password);
                System.Threading.Thread.Sleep(350);
            });

            // Om inloggningen misslyckades
            if (!ok)
            {
                UI.Error("Wrong Username or password");
                return;
            }

            // Bekräfta lyckad inloggning
            UI.Success($"Logged in as {username}!");
            
            TripService service = new TripService();

            // Enkel profilmeny efter inloggning
            while (true)
            {
                // Val mellan att visa profil eller logga ut
                var sub = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold cyan]Menu[/]")
                        .HighlightStyle(new Style(Color.Chartreuse1))
                        .AddChoices("View profile", "Add upcoming trip", "Add previous trip", "Show all trips", "Log out"));

                if (sub == "View profile")
                {
                    // Skapar tabell med kontoinformation
                    var t = new Table();
                    t.Border = TableBorder.Rounded;
                    t.BorderStyle = new Style(Color.Grey50);
                    t.AddColumn("Field");
                    t.AddColumn("Value");
                    t.AddRow("Username", acc.UserName);
                    t.AddRow("Created (UTC)", acc.CreatedAt == default ? "—" : acc.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                    t.AddRow("Recovery-code", acc.RecoveryCode);

                    // Skriver ut tabellen
                    AnsiConsole.Write(t);
                }
                else if (sub == "Add upcoming trip")
                {
                    service.AddUpcomingTrip();
                }
                else if (sub == "Add previous trip")
                {
                    service.AddPreviousTrip();
                }
                else if (sub == "Show all trips")
                {
                    service.ShowAllTrips();
                }
                else break; // Logga ut
            }
        }

        // -------- Forgot Password --------
        private static void ForgotPassword()
        {
            // Visar övergång till lösenordsåterställning
            UI.Transition("Forgotten Password");

            // Frågar efter nödvändiga uppgifter
            var username = AnsiConsole.Ask<string>("Username:");
            var code = AnsiConsole.Ask<string>("Recovery code:");
            var newPwd = AnsiConsole.Prompt(new TextPrompt<string>("New Password:").Secret());
            var confirm = AnsiConsole.Prompt(new TextPrompt<string>("Confirm new password:").Secret());

            // Säkerställer att de två lösenorden matchar
            if (!string.Equals(newPwd, confirm, StringComparison.Ordinal))
            {
                UI.Error("Password doesn't match");
                return;
            }

            // Hämtar kontot baserat på användarnamnet
            var acc = AccountStore.Get(username);
            if (acc == null)
            {
                UI.Error("Unknown username");
                return;
            }

            // Kontrollerar om återställningskoden stämmer
            if (!string.Equals(acc.RecoveryCode, code, StringComparison.Ordinal))
            {
                UI.Error("Wrong recovery code");
                return;
            }

            // Kollar att det nya lösenordet uppfyller säkerhetskrav
            if (!acc.CheckPassword(newPwd))
            {
                UI.Error("New password does not meet the requirements");
                return;
            }

            // Uppdaterar lösenordet med statusanimation
            UI.WithStatus("Updating password...", () =>
            {
                acc.Password = newPwd;
                acc.RecoveryCode = Util.GenerateRecoveryCode(); // Skapar ny återställningskod
                AccountStore.Update(acc);
                AccountStore.Save();
            });

            // Bekräftelse till användaren
            UI.Success("Password reset! A new recovery code has been generated.");
        }
    }
}
