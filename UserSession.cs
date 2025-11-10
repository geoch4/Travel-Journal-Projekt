using Spectre.Console;
using System;

namespace Travel_Journal
{
    /// <summary>
    /// Hanterar vad som händer när en användare är inloggad:
    /// - visa profil
    /// - hantera trips
    /// - budget
    /// - statistik
    /// - logga ut
    /// </summary>
    public class UserSession
    {
        private readonly Account _account;
        private readonly TripService _tripService;

        public UserSession(Account account)
        {
            _account = account;
            _tripService = new TripService(account.UserName);
        }

        // 🧭 Startar menyn för inloggad användare
        public void Start()
        {
            while (true)
            {
                var sub = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold cyan]Welcome, {_account.UserName}![/] Choose an option:")
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices(
                            "👤 View Profile",
                            "➕ Add Upcoming Trip",
                            "🧳 Add Previous Trip",
                            "📋 Show All Trips",
                            "💰 Budget & Savings",
                            "📊 Statistics",
                            "🔄 Update/Change Trips",
                            "🚪 Log out"
                        )
                );

                if (sub == "👤 View Profile")
                {
                    ShowProfile();
                    Pause();
                }
                else if (sub == "➕ Add Upcoming Trip")
                {
                    _tripService.AddUpcomingTrip();
                    Pause();
                }
                else if (sub == "🧳 Add Previous Trip")
                {
                    _tripService.AddPreviousTrip();
                    Pause();
                }
                else if (sub == "📋 Show All Trips")
                {
                    _tripService.ShowAllTrips();
                    Pause();
                }
                else if (sub == "💰 Budget & Savings")
                {
                    var budgetService = new BudgetService(_account, _tripService);
                    budgetService.ShowBudgetMenu();
                }
                else if (sub == "📊 Statistics")
                {
                    var statsService = new Statistics(_tripService);
                    statsService.StatsMenu();
                    Pause();
                }
                else if (sub == "🔄 Update/Change Trips")
                {
                    var trips = _tripService.GetTrips();
                    _tripService.UpdateTrips(trips);
                    Pause();
                }
               

                else if (sub == "🚪 Log out")
                {
                    UI.Transition("Logging out...");
                    UI.Info($"Goodbye, {_account.UserName}! 👋");
                    return;
                }
            }
        }

        // 👤 Visar profilinfo
        private void ShowProfile()
        {
            var t = new Table()
                .Border(TableBorder.Rounded)
                .BorderStyle(new Style(Color.Grey50));

            t.AddColumn("Field");
            t.AddColumn("Value");

            t.AddRow("Username", _account.UserName);
            t.AddRow("Created", _account.CreatedAt == default ? "—" : _account.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
            t.AddRow("Recovery Code", _account.RecoveryCode);
            t.AddRow("Savings", $"{_account.Savings} kr");

            AnsiConsole.Write(t);
        }

        // ⏸️ Enkel paus innan nästa meny
        private void Pause()
        {
            AnsiConsole.MarkupLine("\n[grey]Press [bold]ENTER[/] to continue...[/]");
            Console.ReadLine();
        }
        
    }
}

