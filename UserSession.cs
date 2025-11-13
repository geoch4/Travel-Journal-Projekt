using Spectre.Console;
using System;
using System.Threading.Tasks;

namespace Travel_Journal
{
    /// <summary>
    /// UserSession körs när en användare är inloggad.
    /// Här hanteras:
    /// - profilvisning
    /// - reshantering (lägga till, visa, uppdatera)
    /// - budget och statistik
    /// - AI-förslag via OpenAI
    /// - utloggning
    /// </summary>
    public class UserSession
    {
        // Den aktuella inloggade användaren
        private readonly Account _account;

        // TripService hanterar alla resor (CRUD + JSON-lagring)
        private readonly TripService _tripService;

       
       

        // === Konstruktor ===
        public UserSession(Account account)
        {
            // Spara användaren som är inloggad
            _account = account;

            // Skapa TripService som laddar användarens resor baserat på användarnamn
            _tripService = new TripService(account.UserName);
        }

        // === 🧭 Huvudloop för inloggad användare ===
        // Denna körs tills användaren väljer "Log out"
        public async Task Start()
        {
            while (true)
            {
                // 🧾 Skapa en meny med val (Spectre.Console gör det snyggt och färgrikt)
                var sub = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold cyan]Welcome, {_account.UserName}![/] Choose an option:")
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices(
                            "👤 View Profile",
                            "📘 Add Trips",
                            "📋 Show All Trips",
                            "💰 Budget & Savings",
                            "📊 Statistics",
                            "🔄 Edit Trips",
                            "🤖✈️ AI Travel Assistant",
                            "🔧 Support & Help",
                            "🚪 Log out"
                        )
                );

                // === Menyval: profil ===
                if (sub == "👤 View Profile")
                {
                    ShowProfile();
                    Pause();
                }
                // === Menyval: för både nya "kommande resor" och "gamla resor" ===
                else if (sub == "📘 Add Trips")
                {
                    
                    _tripService.ShowManageTripsMenu();
                }
              
                // === Menyval: visa alla resor ===
                else if (sub == "📋 Show All Trips")
                {
                    _tripService.ShowAllTrips();
                    Pause();
                }
                // === Menyval: budget ===
                else if (sub == "💰 Budget & Savings")
                {
                    // Skapa en separat service för budget (kopplad till användare och resor)
                    var budgetService = new BudgetService(_account, _tripService);
                    budgetService.ShowBudgetMenu();
                }
                // === Menyval: statistik ===
                else if (sub == "📊 Statistics")
                {
                    var statsService = new Statistics(_tripService);
                    statsService.StatsMenu();
                }
                // === Menyval: uppdatera resor ===
                else if (sub == "🔄 Edit Trips")
                {
                    var trips = _tripService.GetTrips();
                    _tripService.UpdateTrips(trips);
                    //Pause();
                }
                // === Menyval: AI Travel Assistant ===
                else if (sub == "🤖✈️ AI Travel Assistant")
                {
                    var aiAssistant = new AITravelAssistant();
                    try
                    {
                        // Rensa skärmen för ren AI-prompt
                        AnsiConsole.Clear();

                        // Vänta tills AI:n har genererat sitt svar (async)
                        await aiAssistant.ShowAISuggestionAsync();
                    }
                    catch (Exception ex)
                    {
                        // Visa eventuella fel i AI-delen på ett snyggt sätt
                        UI.Error($"AI Travel Assistant failed: {ex.Message}");
                    }

                    // Vänta på ENTER innan menyn visas igen
                    Pause();

                    // 👈 Viktigt: fortsätt loopen utan att avsluta sessionen
                    continue;
                }

                // === 🔧 Menyval: Support & Hjälp ===
                else if (sub == "🔧 Support & Help")
                {
                    // Skapar en ny instans av SupportService
                    var support = new SupportService();

                    // Visar supportmenyn och skickar med aktuell användare (_account)
                    bool exit = support.ShowSupportMenu(_account);

                    // Om användaren raderade sitt konto (ShowSupportMenu returnerar true)
                    if (exit)
                        return; // Avsluta hela UserSession.Start() → användaren loggas ut och återgår till huvudmenyn
                }

                // === Menyval: logga ut ===
                else if (sub == "🚪 Log out")
                {
                    UI.Transition("Logging out...");
                    UI.Info($"Goodbye, {_account.UserName}! 👋");
                    return; // Avslutar sessionen och går tillbaka till huvudmenyn
                }
            }
        }

        // === 👤 Visar användarens profilinformation ===
        private void ShowProfile()
        {
            // Skapa en tabell med Spectre.Console
            var t = new Table()
                .Border(TableBorder.Rounded)
                .BorderStyle(new Style(Color.Grey50));

            // Kolumner
            t.AddColumn("Field");
            t.AddColumn("Value");

            // Lägg till data från kontot
            t.AddRow("Username", _account.UserName);
            t.AddRow("Created", _account.CreatedAt == default ? "—" : _account.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
            t.AddRow("Recovery Code", _account.RecoveryCode);
            t.AddRow("Savings", $"{_account.Savings} kr");

            // Skriv ut tabellen i terminalen
            AnsiConsole.Write(t);
        }

        // === ⏸️ Enkel paus innan nästa meny ===
        // Används efter varje val så att användaren hinner läsa resultatet
        public static void Pause()
        {
            AnsiConsole.MarkupLine("\n[grey]Press [bold]ENTER[/] to continue...[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }
        
    }
}

