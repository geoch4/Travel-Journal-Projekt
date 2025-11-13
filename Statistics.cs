using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var trips = _tripService.GetTrips();
            var sortedTrips = trips.OrderByDescending(trip => trip.Score).ToList();
            foreach (var trip in sortedTrips)
            {
                AnsiConsole.MarkupLine($"Country: {trip.Country}, Score: {trip.Score}, Cost: {trip.Cost}");
            }
        }
        public void SortTripsByRatingAscending()
        {
            var trips = _tripService.GetTrips();
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
            var sortedTrips = trips.OrderByDescending(trip => trip.Cost).ToList();
            foreach (var trip in sortedTrips)
            {
                AnsiConsole.MarkupLine($"Country: {trip.Country}, Cost: {trip.Cost}, Score: {trip.Score}");
            }
        }
        public void StatsMenu()
        {
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
                    break;
                case "📉 Sort by rating (lowest to highest)":
                    SortTripsByRatingAscending();
                    break;
                case "💰 Sort by price (highest to lowest)":
                    SortTripsByPriceDescending();
                    break;
            }
        }
    }
}
