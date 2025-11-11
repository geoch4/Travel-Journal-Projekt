using Spectre.Console;
using System;
using System.Linq;

namespace Travel_Journal
{
    public class BudgetService
    {
        private readonly Account _account;
        private readonly TripService _tripService;

        // Konstruktor tar in aktuell användare och hens resor
        public BudgetService(Account account, TripService tripService)
        {
            _account = account;
            _tripService = tripService;
        }

        // === Huvudmeny för budgetfunktionen ===
        public void ShowBudgetMenu()
        {
            while (true)
            {
                UI.Transition("💰 Travel Savings Account");

                // Visa saldo längst upp
                AnsiConsole.MarkupLine($"[bold green]Current balance:[/] {_account.Savings} SEK\n");

                // Menyalternativ
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold cyan]What would you like to do?[/]")
                        .HighlightStyle(new Style(Color.Chartreuse1))
                        .AddChoices("➕ Deposit money", "➖ Withdraw money", "📊 View trip budgets", "↩ Back")
                );

                if (choice == "➕ Deposit money")
                {
                    Deposit();
                }
                else if (choice == "➖ Withdraw money")
                {
                    Withdraw();
                }
                else if (choice == "📊 View trip budgets")
                {
                    ShowTripBudgets();
                }
                else break;
            }
        }

        // === Sätta in pengar ===
        private void Deposit()
        {
            decimal amount = AnsiConsole.Ask<decimal>("How much would you like to [green]deposit[/]?");
            if (amount <= 0)
            {
                UI.Warn("Amount must be greater than zero.");
                return;
            }

            _account.Savings += amount;
            AccountStore.Update(_account);
            AccountStore.Save();
            UI.Success($"Deposited {amount} SEK. New balance: {_account.Savings} SEK");
        }

        // === Ta ut pengar ===
        private void Withdraw()
        {
            decimal amount = AnsiConsole.Ask<decimal>("How much would you like to [red]withdraw[/]?");
            if (amount <= 0)
            {
                UI.Warn("Amount must be greater than zero.");
                return;
            }

            if (amount > _account.Savings)
            {
                UI.Error("You don't have enough balance!");
                return;
            }

            _account.Savings -= amount;
            AccountStore.Update(_account);
            AccountStore.Save();
            UI.Success($"Withdrew {amount} SEK. New balance: {_account.Savings} SEK");
        }

        // === Visa resor med planerad budget och faktisk kostnad ===
        private void ShowTripBudgets()
        {
            var trips = _tripService.GetTrips(); // Hämtar alla resor
            if (trips.Count == 0)
            {
                UI.Warn("No trips found yet.");
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
                string budget = $"{trip.PlannedBudget} {trip.Currency}";
                string cost = trip.Cost > 0 ? $"{trip.Cost} {trip.Currency}" : "[grey]—[/]";

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
        }

    }
}
    
