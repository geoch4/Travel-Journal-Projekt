using Spectre.Console;
using System;
using System.Threading;

namespace Travel_Journal
{
    public static class App
    {
        // === Huvudmetoden som startar hela programmet ===
        public static void Run()
        {
            // Säkerställer att "data"-mappen finns innan vi läser/sparar något
            Paths.EnsureDataDir();

            // Visar ett snyggt intro med titel (Travel Journal)
            UI.Splash();

            // Laddar alla registrerade användarkonton från users.json
            AccountStore.LoadWithProgress();

            // Huvudmeny-loop: körs tills användaren väljer "Exit"
            while (true)
            {
                var choice = UI.MainMenu(); // Visar menyn (Register / Login / Forgot password / Exit)

                if (choice == "Register")
                    Register(); // Gå till registreringsflödet
                else if (choice == "Login")
                    Login(); // Gå till inloggning
                else if (choice == "Forgot password")
                    ForgotPassword(); // Återställ lösenord
                else
                {
                    // Avslutar programmet
                    UI.Transition("Exiting...");
                    AnsiConsole.MarkupLine("[green]Thank you for using Travel Journal![/]");
                    Thread.Sleep(1000);
                    return; // Avsluta applikationen
                }
            }
        }

        // === Registrerar ett nytt användarkonto ===
        private static void Register()
        {
            UI.Transition("Register Account");

            // Fråga användaren efter ett användarnamn
            var username = AnsiConsole.Ask<string>("Username:");

            // Kolla om det redan finns ett konto med det namnet
            if (AccountStore.Exists(username))
            {
                UI.Warn("User already exists!");
                return;
            }

            // Fråga efter lösenord (dolt i konsolen)
            var password = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

            // Skapa ett nytt kontoobjekt
            var acc = new Account();

            // Kolla att lösenordet följer reglerna
            if (!acc.Register(username, password))
            {
                UI.Error("Password requirements: min 6 chars, uppercase, lowercase, number, special.");
                return;
            }

            // Skapa en återställningskod och spara tidpunkten kontot skapades
            acc.RecoveryCode = Util.GenerateRecoveryCode();
            acc.CreatedAt = DateTime.UtcNow;

            // Visa spinner medan kontot sparas till users.json
            UI.WithStatus("Saving account...", () =>
            {
                AccountStore.Add(acc);
                AccountStore.Save();
            });

            // Visa bekräftelse i en snygg panel
            var panel = new Panel($"[green]✅ User {acc.UserName} created successfully![/]\n[yellow]Recovery code:[/] [bold]{acc.RecoveryCode}[/]")
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader("Registration Successful", Justify.Center)
            };
            AnsiConsole.Write(panel);
        }

        // === Loggar in en befintlig användare ===
        private static void Login()
        {
            UI.Transition("Login");

            // Be användaren skriva in användarnamn och lösenord
            var username = AnsiConsole.Ask<string>("Username:");
            var password = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

            // Försök hämta kontot ur AccountStore
            var acc = AccountStore.Get(username);

            if (acc == null)
            {
                // Om inget konto hittas med det namnet
                UI.Error("Unknown username.");
                return;
            }

            bool ok = false;

            // Spinner som visar att inloggningen kontrolleras
            UI.WithStatus("Verifying...", () =>
            {
                ok = acc.Login(username, password);
                Thread.Sleep(400);
            });

            if (!ok)
            {
                UI.Error("Wrong username or password.");
                return;
            }

            // Inloggning lyckades 🎉
            UI.Success($"Logged in as [bold]{username}[/]! ✈️");

            // Skapa en TripService som laddar/sparar användarens personliga trips.json
            var service = new TripService(username);

            // === Meny som visas när användaren är inloggad ===
            while (true)
            {
                // Visa alternativ som användaren kan göra
                var sub = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold cyan]Welcome, {username}![/] Choose an option:")
                        .HighlightStyle(new Style(Color.DeepSkyBlue1))
                        .AddChoices(
                            "👤 View Profile",
                            "➕ Add Upcoming Trip",
                            "🧳 Add Previous Trip",
                            "📋 Show All Trips",
                            "💰 Budget & Savings",
                            "🚪 Log out"
                        )
                );

                // === Hantera menyval ===
                if (sub == "👤 View Profile")
                    ShowProfile(acc); // Visa användarinfo
                else if (sub == "➕ Add Upcoming Trip")
                {
                    service.AddUpcomingTrip(); // Lägg till ny kommande resa
                    Pause(); // Vänta på ENTER
                }
                else if (sub == "🧳 Add Previous Trip")
                {
                    service.AddPreviousTrip(); // Lägg till redan genomförd resa
                    Pause();
                }
                else if (sub == "📋 Show All Trips")
                {
                    service.ShowAllTrips(); // Visa alla resor i tabell
                    Pause();
                }
                else if (sub == "💰 Budget & Savings")
                {
                    var budgetService = new BudgetService(acc, service);
                    budgetService.ShowBudgetMenu();
                }
                else if (sub == "🚪 Log out")
                {
                    // När användaren loggar ut
                    UI.Transition("Logging out...");
                    UI.Info($"Goodbye, {username}! 👋");
                    Thread.Sleep(800);
                    return; // Gå tillbaka till huvudmenyn
                }
            }
        }

        // === Visar användarens profilinfo (används i inloggad meny) ===
        private static void ShowProfile(Account acc)
        {
            // Skapar en tabell för att snyggt presentera användarinfo
            var t = new Table()
                .Border(TableBorder.Rounded)
                .BorderStyle(new Style(Color.Grey50));

            // Kolumnrubriker
            t.AddColumn("Field");
            t.AddColumn("Value");

            // Fyll tabellen med användarens data
            t.AddRow("Username", acc.UserName);
            t.AddRow("Created", acc.CreatedAt == default ? "—" : acc.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
            t.AddRow("Recovery Code", acc.RecoveryCode);

            // Skriv ut tabellen till konsolen
            AnsiConsole.Write(t);
        }

        // === Återställ glömt lösenord ===
        private static void ForgotPassword()
        {
            UI.Transition("Forgot Password");

            // Fråga användaren om nödvändig information
            var username = AnsiConsole.Ask<string>("Username:");
            var code = AnsiConsole.Ask<string>("Recovery code:");
            var newPwd = AnsiConsole.Prompt(new TextPrompt<string>("New password:").Secret());
            var confirm = AnsiConsole.Prompt(new TextPrompt<string>("Confirm password:").Secret());

            // Säkerställ att användaren skrev samma lösenord två gånger
            if (!string.Equals(newPwd, confirm, StringComparison.Ordinal))
            {
                UI.Error("Passwords do not match.");
                return;
            }

            // Hämta kontot för att kunna uppdatera det
            var acc = AccountStore.Get(username);
            if (acc == null)
            {
                UI.Error("Unknown username.");
                return;
            }

            // Kontrollera att återställningskoden är korrekt
            if (!string.Equals(acc.RecoveryCode, code, StringComparison.Ordinal))
            {
                UI.Error("Wrong recovery code.");
                return;
            }

            // Kontrollera att det nya lösenordet är giltigt
            if (!acc.CheckPassword(newPwd))
            {
                UI.Error("New password does not meet the requirements.");
                return;
            }

            // Uppdatera kontots lösenord och spara
            UI.WithStatus("Updating password...", () =>
            {
                acc.Password = newPwd;
                acc.RecoveryCode = Util.GenerateRecoveryCode(); // Skapa ny kod för säkerhet
                AccountStore.Update(acc);
                AccountStore.Save();
            });

            UI.Success("Password reset! A new recovery code has been generated.");
        }

        // === Paus efter visning/åtgärd ===
        private static void Pause()
        {
            // Enkel metod för att låta användaren läsa klart innan nästa meny visas
            AnsiConsole.MarkupLine("\n[grey]Press [bold]ENTER[/] to continue...[/]");
            Console.ReadLine();
        }
    }
}
