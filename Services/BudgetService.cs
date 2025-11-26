using Spectre.Console;
using System;
using System.Linq;
using Travel_Journal.Data;
using Travel_Journal.Models;
using Travel_Journal.UIServices;

namespace Travel_Journal.Services
{
    // Tjänst för budgethantering och hantera resebudgetar och drömresa
    public class BudgetService
    {
        private readonly Account _account;
        private readonly TripService _tripService;

        public Account Account => _account;

        // Konstruktor tar in aktuell användare och hens resor
        public BudgetService(Account account, TripService tripService)
        {
            _account = account;
            _tripService = tripService;
        }

        // === Sätta in pengar ===
        public void Deposit()
        {
            decimal amount = UI.AskDecimal("How much would you like to [green]deposit[/]?");
            if (amount <= 0)
            {
                UI.Warn("Amount must be greater than zero.");
                Logg.Log($"User attempted to deposit invalid amount: {amount}");
                UI.Pause();
                return;
            }

            _account.Savings += amount;
            AccountStore.Update(_account);
            AccountStore.Save();
            UI.Success($"Deposited {amount} SEK. New balance: {_account.Savings} SEK");
            UI.Pause();
        }

        // === Ta ut pengar ===
        public void Withdraw()
        {
            decimal amount = UI.AskDecimal("How much would you like to [red]withdraw[/]?");
            if (amount <= 0)
            {
                UI.Warn("Amount must be greater than zero.");
                Logg.Log($"User attempted to withdraw invalid amount: {amount}");
                return;
            }

            if (amount > _account.Savings)
            {
                UI.Error("You don't have enough balance!");
                Logg.Log($"Insufficient balance: tried to withdraw {amount} but only {_account.Savings} available.");
                return;
            }

            _account.Savings -= amount;
            AccountStore.Update(_account);
            AccountStore.Save();
            UI.Success($"Withdrew {amount} SEK. New balance: {_account.Savings} SEK");
            UI.Pause();
        }

        // === Visa resor med planerad budget och faktisk kostnad ===
        public void ShowTripBudgets()
        {
            var trips = _tripService.GetAllTrips(); // Hämtar alla resor
            if (trips.Count == 0)
            {
                UI.Warn("No trips found.");
                Logg.Log($"No trips found for user '{_account.UserName}' in ShowTripBudgets.");
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey50)
                .Centered();

            table.AddColumn("[bold cyan]Destination[/]");
            table.AddColumn("[bold cyan]Budget[/]");
            table.AddColumn("[bold cyan]Cost[/]");
            table.AddColumn("[bold cyan]Status[/]");

            foreach (var trip in trips)
            {
                string destination = $"{trip.City}, {trip.Country}";
                string budget = $"{trip.PlannedBudget}";
                string cost = trip.Cost > 0 ? $"{trip.Cost}" : "[grey]—[/]";

                string status;
                Color color;

                if (trip.Cost == 0)
                {
                    status = "Upcoming";
                    color = Color.Green;
                }
                else if (trip.Cost <= trip.PlannedBudget)
                {
                    status = "✅ Within budget";
                    color = Color.Green;
                }
                else
                {
                    status = "❌ Over budget";
                    color = Color.Red;
                }

                table.AddRow(
                    new Markup($"[{color}]{destination}[/]"),
                    new Markup($"[{color}]{budget}[/]"),
                    new Markup($"[{color}]{cost}[/]"),
                    new Markup($"[{color}]{status}[/]")
                );
            }

            AnsiConsole.Write(table);
            UI.Pause();
        }
        // === Hantera drömresa ===
        public void DreamVacation()
        {
            AnsiConsole.Clear();

            UI.Transition("✨ Dream Vacation"); // Titelövergång

            // Kontrollera om användaren redan har en sparad drömresa
            if (_account.DreamDestination != null && _account.DreamBudget != null)
            {
                ShowExistingDreamVacation(); // Visa befintlig drömresa
                return; // Avsluta metoden
            }

            CreateOrUpdateDreamVacation(); // Skapa ny drömresa om ingen finns
        }
        // Visa befintlig drömresa
        private void ShowExistingDreamVacation()
        {
            // Beräkna saknad summa
            decimal missingAmount = (_account.DreamBudget ?? 0) - _account.Savings;

            // Välj färg beroende på om man har råd
            string statusText;
            string statusColor;

            if (missingAmount <= 0)
            {
                statusText = "You already have enough savings! 🎉";
                statusColor = "green";
                missingAmount = 0; // man har tillräckligt med pengar för resan
            }
            else
            {
                statusText = $"You need [red]{missingAmount} SEK[/] more to afford this trip.";
                statusColor = "yellow";
            }

            // Skapa panel med drömresan + status
            var panel = new Panel(
                $"[bold cyan]{_account.DreamDestination}[/]\n\n" +                // Drömdestination
                $"[yellow]Ideal Budget:[/] [bold green]{_account.DreamBudget} SEK[/]\n\n" + // Drömbudget
                $"[{statusColor}]{statusText}[/]"                                // Sparstatus
            )
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 1),
                BorderStyle = new Style(Color.Gold1),
                Header = new PanelHeader("🌍 Your Dream Vacation")
            };

            AnsiConsole.Write(panel);

            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title($"\nDo you want to update your dream vacation?")
                .AddChoices("✅ Yes", "❌ No")
                );

            if (confirm == "❌ No")
            {
                AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                UI.Pause();
                return;
            }
            else
            {
                CreateOrUpdateDreamVacation();
            }
        }

        // Skapa eller uppdatera drömresa
        private void CreateOrUpdateDreamVacation()
        {
            // Fråga om destination
            var destination = UI.AskWithBack("[cyan]Where is your dream destination?[/]");
            if (destination == null) // Går tillbaka till budgetmenyn om användare trycker b
            {
                return;
            }

            // Fråga om ideal budget
            var budget = AnsiConsole.Ask<decimal>(
                "[green]What is your ideal budget for this dream vacation?[/]"
            );

            // Spara till kontot
            _account.DreamDestination = destination; // Sätt destination
            _account.DreamBudget = budget;           // Sätt budget

            // Uppdatera lagrad användardata
            AccountStore.Update(_account); // Ersätter användaren i listan
            AccountStore.Save();           // Sparar till Users.json

            UI.Success("Your dream vacation has been saved!"); // Bekräftelse
        }
    }
}
    
