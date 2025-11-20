using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Travel_Journal
{
    public class Statistics
    {
        //SKapa en metod som sorterar resor efter betyg.
        private readonly TripService _tripService;

        public Statistics(TripService tripService)
        {
            _tripService = tripService;
        }

        public void SortTripsByRatingDescending()
        {
            // Om det inte finns några resor för användaren registrerade.
            var trips = _tripService.GetTrips(); // Hämtar alla resor
            if (trips.Count == 0)
            {
                UI.Warn("No trips found.");
                Logg.Log($"No trips found for user {_tripService.UserName} in SortTripsByRatingDescending");
                
                return;
            }
            var sortedTrips = trips.OrderByDescending(trip => trip.Score).ToList();
            foreach (var trip in sortedTrips)
            {
                AnsiConsole.MarkupLine($"Country: {trip.Country}, Score: {trip.Score}, Cost: {trip.Cost}");
            }
        }
        public void SortTripsByRatingAscending()
        {
            var trips = _tripService.GetTrips();
            if (trips.Count == 0)
            {
                UI.Warn("No trips found.");
                Logg.Log($"No trips found for user {_tripService.UserName} in SortTripsByRatingAscending");

                return;
            }
            var sortedTrips = trips.OrderBy(trip => trip.Score).ToList();
            foreach (var trip in sortedTrips)
            {
                AnsiConsole.MarkupLine($"Country: {trip.Country}, Score: {trip.Score}, Cost: {trip.Cost}");
            }
        }
        //Skapa en metod som visar dyrast till billigaste resan.
        public void SortTripsByPriceDescending()
        {
            var trips = _tripService.GetTrips();
            if (trips.Count == 0)
            {
                UI.Warn("No trips found.");
                Logg.Log($"No trips found for user {_tripService.UserName} in SortTripsByPriceDescending");

                return;
            }
            var sortedTrips = trips.OrderByDescending(trip => trip.Cost).ToList();
            foreach (var trip in sortedTrips)
            {
                AnsiConsole.MarkupLine($"Country: {trip.Country}, Cost: {trip.Cost}, Score: {trip.Score}");
            }
        }
        public void StatsMenu()
        {
            AnsiConsole.Clear();
            var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold cyan] Choose an option:[/]")
                        .HighlightStyle(new Style(Color.DeepSkyBlue1))
                        .AddChoices(
                            "📈 Sort by rating (highest to lowest)",
                            "📉 Sort by rating (lowest to highest)",
                            "💰 Sort by price (highest to lowest)",
                            "🔙 Back to Main Menu"

                        )
                );
            switch (choice)
            {
                case "📈 Sort by rating (highest to lowest)":
                    SortTripsByRatingDescending();
                    UserSession.Pause();
                    break;
                case "📉 Sort by rating (lowest to highest)":
                    SortTripsByRatingAscending();
                    UserSession.Pause();
                    break;
                case "💰 Sort by price (highest to lowest)":
                    SortTripsByPriceDescending();
                    UserSession.Pause();
                    break;
            }
        }
    }
}
