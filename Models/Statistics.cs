using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Travel_Journal.Data;
using Travel_Journal.Services;
using Travel_Journal.UIServices;

namespace Travel_Journal.Models
{
    public class Statistics
    {
        //Skapa en metod som sorterar resor efter betyg.
        private readonly TripService _tripService;

        // Konstruktor som tar emot TripService via dependency injection
        public Statistics(TripService tripService)
        {
            _tripService = tripService;
        }

        //Metod som sorterar resor från högsta till lägsta betyg vi använder LINQ för att sortera listan.
        public void SortTripsByRatingDescending()
        {
            AnsiConsole.Clear();
            // Om det inte finns några resor för användaren registrerade.
            var trips = _tripService.GetAllTrips(); // Hämtar alla resor
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
        //Skapa en metod som sorterar resor från lägsta till högsta betyG vi använder LINQ för att sortera listan.
        public void SortTripsByRatingAscending()
        {
            AnsiConsole.Clear();
            var trips = _tripService.GetAllTrips();
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
            AnsiConsole.Clear();
            var trips = _tripService.GetAllTrips();
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
    }
}
