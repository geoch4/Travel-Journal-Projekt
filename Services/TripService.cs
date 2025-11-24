using System;
using System.Collections.Generic;
using System.Linq;
using Travel_Journal.Data;
using Travel_Journal.Models;

namespace Travel_Journal.Services
{
    /// <summary>
    /// TripService är programmets "Motor".
    /// Den ansvarar för att hålla koll på listan med resor (state) och prata med databasen/filen.
    /// Den innehåller INGEN kod för Spectre.Console (UI).
    /// </summary>
    public class TripService
    {
        // === Fält ===

        // Huvudlistan som håller alla resor i minnet medan programmet körs
        private List<Trip> trips = new();

        // Användarnamnet (behövs för att veta vilken fil vi ska ladda/spara)
        private readonly string username;

        // DataStore sköter själva "grovjobbet" med att läsa/skriva JSON
        private readonly DataStore<Trip> store;

        // Vi exponerar användarnamnet så att UI kan visa det i rubriker (t.ex. "All Trips for User X")
        public string UserName => username;

        // === Konstruktor ===
        public TripService(string username)
        {
            // Spara det inkommande användarnamnet i klassens fält så att det blir tillgängligt för hela klassen (inte bara i konstruktorn).
            this.username = username;

            // Koppla till json-filen: "användarnamn_trips.json"
            store = new DataStore<Trip>($"{username}_trips.json");

            // Försök ladda resor direkt när servicen startar.
            try
            {
                trips = store.Load();
            }
            catch
            {
                // Om filen inte finns eller är trasig, startar vi med en tom lista
                // så att programmet inte kraschar.
                trips = new List<Trip>();
            }
        }

        // === Data-metoder (API mot UI) ===

        /// <summary>
        /// Returnerar en referens till listan med alla resor.
        /// </summary>
        public List<Trip> GetAllTrips()
        {
            return trips;
        }

        /// <summary>
        /// Lägger till en ny resa i listan och sparar direkt till filen.
        /// </summary>
        public void AddTrip(Trip trip)
        {
            trips.Add(trip);
            Save(); // Viktigt: Spara direkt så ingen data går förlorad vid krasch
        }

        /// <summary>
        /// Tar bort en resa från listan och uppdaterar filen.
        /// </summary>
        public void DeleteTrip(Trip trip)
        {
            trips.Remove(trip);
            Save();
        }

        /// <summary>
        /// Tvingar fram en sparning till JSON-filen.
        /// Denna är 'public' så att UI kan anropa den efter att ha redigerat en resa 
        /// (t.ex. ändrat budget på en befintlig resa).
        /// </summary>
        public void Save()
        {
            // Vi gör ingen try-catch här. Om det blir fel (t.ex. slut på diskutrymme),
            // låter vi felet "bubbla upp" till UI-klassen som får visa felmeddelandet för användaren.
            store.Save(trips);
        }

        // === Logik för Världskartan ===

        /// <summary>
        /// Denna metod bearbetar data för världskartan.
        /// Den filtrerar ut slutförda resor och översätter landsnamn till GeoJSON-format.
        /// </summary>
        public List<string> GetVisitedCountryNamesForMap()
        {
            // 1. Filtrera: Hämta bara resor som är markerade som "Completed"
            var completed = trips
                .Where(t => t.IsCompleted)
                .Select(t => t.Country?.Trim())         // Hämta landsnamnet och ta bort mellanslag
                .Where(c => !string.IsNullOrWhiteSpace(c)) // Ignorera tomma rader
                .Distinct(StringComparer.OrdinalIgnoreCase) // Ta bort dubbletter (Har man varit i Norge 2 ggr visas det bara en gång)
                .ToList();

            var result = new List<string>();

            // 2. Mappa: Översätt vanliga namn till officiella namn (t.ex. "USA" -> "United States of America")
            foreach (var country in completed)
            {
                var key = country!.ToLowerInvariant();

                if (CountryAlias.TryGetValue(key, out var mapped))
                    result.Add(mapped); // Använd det officiella namnet från vår lista
                else
                    result.Add(country); // Använd namnet användaren skrev in
            }
            return result;
        }

        // Dictionary för lands-alias. 
        // Static readonly eftersom listan ser likadan ut för alla användare.
        private static readonly Dictionary<string, string> CountryAlias = new()
        {
            ["usa"] = "United States of America",
            ["us"] = "United States of America",
            ["united states"] = "United States of America",
            ["uk"] = "United Kingdom",
            ["england"] = "United Kingdom",
            ["scotland"] = "United Kingdom",
            ["great britain"] = "United Kingdom",
            ["south korea"] = "Korea, Republic of",
            ["north korea"] = "Korea, Democratic People's Republic of",
            ["laos"] = "Lao People's Democratic Republic",
            ["vietnam"] = "Viet Nam",
            ["iran"] = "Iran, Islamic Republic of",
            ["bolivia"] = "Bolivia, Plurinational State of",
            ["tanzania"] = "Tanzania, United Republic of",
            ["moldova"] = "Moldova, Republic of",
            ["venezuela"] = "Venezuela, Bolivarian Republic of",
            ["syria"] = "Syrian Arab Republic"
        };
    }
}