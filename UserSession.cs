using Spectre.Console;
using System;
using System.Threading.Tasks;
using Travel_Journal.Data;
using Travel_Journal.Models;
using Travel_Journal.UIServices;

namespace Travel_Journal.Services
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
        // === Fält ===
        // Den aktuella inloggade användaren
        private readonly Account _account;

        // Hanterar alla resor för användaren (CRUD + JSON)
        private readonly TripService _tripService;

        // Hanterar UTSEENDE (Menyer & Input)
        private readonly TripUI _tripUI;
        private readonly UpdateTripUI _updateTripUI;

        // Förifyllda services för att slippa new i switchen
        private readonly BudgetService _budget;
        private readonly StatisticsUI _stats;
        private readonly SupportService _support;
        private readonly WorldMapService _map;

        // === Konstruktor ===
        public UserSession(Account account)
        {
            // Spara användaren som är inloggad
            _account = account;

            // Skapa TripService som laddar användarens resor baserat på användarnamn
            _tripService = new TripService(account.UserName);

            // 2. Skapa TripUI och ge den servicen (Ansiktet) 
            _tripUI = new TripUI(_tripService);
            _updateTripUI = new UpdateTripUI(_tripService);

            // Initiera alla services som behövs i sessionen
            _budget = new BudgetService(account, _tripService);
            _stats = new StatisticsUI(_tripService);
            _support = new SupportService();
            _map = new WorldMapService(_tripService);
        }

        // === 🧭 Huvudloop för inloggad användare ===
        // Denna körs tills användaren väljer "Log out"
        public async Task Start()
        {
            while (true)
            {
                // Hämta menyvalet
                string choice = MenuService.LoggedInMenu(_account.UserName);

                switch (choice)
                {
                    // === Profil ===
                    case "👤 View Profile":
                        UI.ShowProfile(_account);
                        UI.Pause();
                        break;

                    // === Lägg till resor ===
                    case "📘 Add Trips":
                        MenuService.ShowTripMenu(_tripUI);
                        break;

                    // === Visa alla resor ===
                    case "📋 Show All Trips":
                        _tripUI.ShowAllTrips();
                        UI.Pause();
                        break;

                    // === Budget ===
                    case "💰 Budget & Savings":
                        MenuService.BudgetMenu(_budget);
                        break;

                    // === Statistik ===
                    case "📊 Statistics":
                        MenuService.StatsMenu(_stats);
                        break;

                    // === Uppdatera resor ===
                    case "🔄 Edit Trips":
                        MenuService.ShowTripEditMenu(_updateTripUI);
                        break;

                    // === AI-assistent ===
                    case "🤖✈️ AI Travel Assistant":
                        await RunAIAssistant();
                        continue; // fortsätt loopen direkt utan break

                    // === Världskarta ===
                    case "🌍 World Map (Visited Countries)":
                        try
                        {
                            _map.OpenWorldMap(); // den pausar själv
                        }
                        catch (Exception ex)
                        {
                            UI.Error($"Failed to generate world map: {ex.Message}");
                            Logg.Log($"World Map error for user {_account.UserName}: {ex}");
                        }
                        continue;

                    // === Support ===
                    case "🔧 Support & Help":
                        bool exit = MenuService.ShowSupportMenu(_support, _account);

                        if (exit)
                            return; // Användaren valde Delete Account → avsluta session

                        break;

                    // === Logga ut ===
                    case "🚪 Log out":
                        UI.Transition("Logging out...");
                        UI.Info($"Goodbye, {_account.UserName}! 👋");
                        return;

                    // === Unknown ===
                    default:
                        UI.Error("Unknown menu selection.");
                        break;
                }
            }
        }

        // === 🧠 Extraherad AI-metod (samma logik, bara flyttad) ===
        private async Task RunAIAssistant()
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
                UI.Error($"AI Travel Assistant failed: {ex.Message}");
                Logg.Log($"AI Travel Assistant error for user {_account.UserName}: {ex}");
            }

            // Vänta på ENTER innan menyn visas igen
            UI.Pause();
        }
    }
}
