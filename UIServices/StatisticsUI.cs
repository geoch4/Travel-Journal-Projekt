using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using Travel_Journal.Data;
using Travel_Journal.Models;
using Travel_Journal.Services;

namespace Travel_Journal.UIServices
{
    public class StatisticsUI
    {
        // Private fält för TripService
        private readonly TripService _tripService;

        // Konstruktor för att injicera TripService
        public StatisticsUI(TripService tripService)
        {
            _tripService = tripService;
        }

        // Metod: Sortera resor efter betyg (Högst till lägst)
        public void SortTripsByRatingDescending()
        {
            // Hämta alla resor för den aktuella användaren
            var trips = _tripService.GetAllTrips();

            // Kontrollerar om listan är tom och avbryter i så fall. Vi använder nameof för att loggen ska veta exakt vilken metod som anropades. 
            if (!HasTrips(trips, nameof(SortTripsByRatingDescending))) return;

            // Sortera resorna efter betyg i fallande ordning
            var sortedTrips = trips.OrderByDescending(trip => trip.Score).ToList();

            AnsiConsole.Clear();
            // Skriv ut rubriken
            AnsiConsole.Write(new Rule("[yellow]Trips by Rating (High to Low)[/]").RuleStyle("grey"));
            // Anropa hjälpmetoden för att skriva ut tabellen
            PrintTrips(sortedTrips);
        }

        // Metod: Sortera resor efter betyg (Lägst till högst)
        public void SortTripsByRatingAscending()
        {
            // Hämta alla resor för den aktuella användaren
            var trips = _tripService.GetAllTrips();

            // Kontrollerar om listan är tom och avbryter i så fall. Vi använder nameof för att loggen ska veta exakt vilken metod som anropades.
            if (!HasTrips(trips, nameof(SortTripsByRatingAscending))) return;

            // Sortera resorna efter betyg i stigande ordning
            var sortedTrips = trips.OrderBy(trip => trip.Score).ToList();

            AnsiConsole.Clear();
            // Skriv ut rubriken
            AnsiConsole.Write(new Rule("[yellow]Trips by Rating (Low to High)[/]").RuleStyle("grey"));
            // Anropa hjälpmetoden för att skriva ut tabellen
            PrintTrips(sortedTrips);
        }

        // Metod: Sortera resor efter pris (Dyrast till billigast)
        public void SortTripsByPriceDescending()
        {
            // Hämta alla resor för den aktuella användaren
            var trips = _tripService.GetAllTrips();

            // Kontrollerar om listan är tom och avbryter i så fall. Vi använder nameof för att loggen ska veta exakt vilken metod som anropades.
            if (!HasTrips(trips, nameof(SortTripsByPriceDescending))) return;

            // Sortera resorna efter pris i fallande ordning
            var sortedTrips = trips.OrderByDescending(trip => trip.Cost).ToList();

            AnsiConsole.Clear();
            // Skriv ut rubriken
            AnsiConsole.Write(new Rule("[yellow]Trips by Price (High to Low)[/]").RuleStyle("grey"));
            // Anropa hjälpmetoden för att skriva ut tabellen
            PrintTrips(sortedTrips);
        }

        // --- Hjälpmetoder (Private) ---

        // Kontrollerar om listan är tom och hanterar varningar/loggning
        private bool HasTrips(List<Trip> trips, string callingMethodName)
        {
            // Kontrollera om listan är tom
            if (trips == null || trips.Count == 0)
            {
                AnsiConsole.Clear();
                UI.Warn("No trips found.");

                // Logga händelsen med metodnamnet som anropade denna kontroll
                Logg.Log($"No trips found for user {_tripService.UserName} in {callingMethodName}"); 
                return false;
            }
            return true; // Listan innehåller resor
        }

        // Ritar ut tabellen - En plats att ändra utseendet på för alla metoder!
        // Ritar ut tabellen - Nu centrerad och med Stad!
        private void PrintTrips(List<Trip> trips)
        {
            // Skapa en tabell
            var table = new Table();

            // Lägg till kolumner
            table.AddColumn("Country");
            table.AddColumn("City");
            table.AddColumn(new TableColumn("Score").Centered());
            table.AddColumn(new TableColumn("Cost").RightAligned());

            // Lägg till rader för varje resa
            foreach (var trip in trips)
            {
                table.AddRow(
                    trip.Country,
                    trip.City,
                    trip.Score.ToString(),
                    $"{trip.Cost:C}"
                );
            }

            // Centrera tabellen
            // Vi använder "Align" för att placera tabellen i mitten av konsolfönstret
            AnsiConsole.Write(new Align(table, HorizontalAlignment.Center));
        }
    }
}