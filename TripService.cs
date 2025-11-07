using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Spectre.Console;

namespace Travel_Journal
{
    public class TripService
    {
        // === Fält ===
        // Denna lista håller alla resor som laddas eller läggs till under programmets gång.
        private List<Trip> trips = new();

        // Filvägen till den JSON-fil där användarens resor sparas (t.ex. data/andre_trips.json)
        private readonly string filePath;

        // Användarnamnet som används för att identifiera rätt fil
        private readonly string username;

        // === Konstruktor ===
        public TripService(string username)
        {
            // Spara användarnamnet så vi vet vem resorna tillhör
            this.username = username;

            // Skapa mappen "data" om den inte finns
            Directory.CreateDirectory(Paths.DataDir);

            // Sätt ihop hela sökvägen till användarens personliga JSON-fil
            filePath = Path.Combine(Paths.DataDir, $"{username}_trips.json");

            // Försök läsa in tidigare resor från filen
            LoadTrips();
        }

        // === Sparar alla resor till JSON-fil ===
        private void SaveTrips()
        {
            try
            {
                // Konvertera listan "trips" till JSON-text med snygg indentering
                var json = JsonSerializer.Serialize(trips, new JsonSerializerOptions { WriteIndented = true });

                // Skriv JSON-texten till användarens fil
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                // Om något går fel, visa felmeddelande
                UI.Error($"Failed to save trips: {ex.Message}");
            }
        }

        // === Läser in alla resor från användarens JSON-fil ===
        private void LoadTrips()
        {
            // Kolla först att filen faktiskt finns
            if (File.Exists(filePath))
            {
                try
                {
                    // Läs in all text från JSON-filen
                    var json = File.ReadAllText(filePath);

                    // Försök omvandla texten till en lista av Trip-objekt
                    trips = JsonSerializer.Deserialize<List<Trip>>(json) ?? new();
                }
                catch (Exception ex)
                {
                    // Om något går fel vid inläsning, visa fel och starta med tom lista
                    UI.Error($"Failed to load trips: {ex.Message}");
                    trips = new();
                }
            }
        }

        // === Hjälpmetod: ber användaren om ett decimaltal (ex. budget) ===
        private decimal AskDecimal(string message)
        {
            while (true)
            {
                try
                {
                    // Spectre.Console hanterar input snyggt med färger och validering
                    return AnsiConsole.Ask<decimal>(message);
                }
                catch
                {
                    // Om användaren skriver något ogiltigt (t.ex. bokstäver) visas fel
                    UI.Error("Invalid number. Try again.");
                }
            }
        }

        // === Hjälpmetod: ber användaren om ett heltal (ex. antal personer) ===
        private int AskInt(string message)
        {
            while (true)
            {
                try
                {
                    return AnsiConsole.Ask<int>(message);
                }
                catch
                {
                    UI.Error("Please enter a valid number.");
                }
            }
        }

        // === Lägger till en framtida resa (planerad resa) ===
        public void AddUpcomingTrip()
        {
            // Snygg övergångsrubrik
            UI.Transition("Add Upcoming Trip ✈️");

            // Frågor till användaren
            string country = AnsiConsole.Ask<string>("Which [bold]country[/] are you visiting?");
            string city = AnsiConsole.Ask<string>("Which [bold]city[/]?");
            string currency = AnsiConsole.Ask<string>("What [bold]currency[/]? (e.g. SEK, USD, EUR)");
            decimal budget = AskDecimal("What is your planned [bold]budget[/]?");

            // Kolla så att datumen är logiska (start före slut)
            DateTime startDate, endDate;
            while (true)
            {
                startDate = AnsiConsole.Ask<DateTime>("Departure date (YYYY-MM-DD):");
                endDate = AnsiConsole.Ask<DateTime>("Return date (YYYY-MM-DD):");

                if (startDate > endDate)
                    UI.Warn("❌ Invalid date! Start date must be before end date.");
                else
                    break;
            }

            int passengers = AskInt("How many [bold]people[/] are joining?");

            // Skapa ett nytt Trip-objekt baserat på användarens input
            var newTrip = new Trip
            {
                Country = country,
                City = city,
                Currency = currency,
                PlannedBudget = budget,
                StartDate = startDate,
                EndDate = endDate,
                NumberOfPassengers = passengers,
                TimeZone = "Europe/Stockholm"
            };

            // Lägg till i listan och spara till fil
            trips.Add(newTrip);
            SaveTrips();

            // Bekräftelse till användaren
            var panel = new Panel(
                $"[green]✅ Trip to [bold]{city}, {country}[/] added successfully![/]\n" +
                $"[grey]Budget:[/] {budget} {currency}\n" +
                $"[grey]Dates:[/] {startDate:yyyy-MM-dd} → {endDate:yyyy-MM-dd}")
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader("Trip Saved", Justify.Center),
                BorderStyle = new Style(Color.Green)
            };
            AnsiConsole.Write(panel);
        }

        // === Lägger till en tidigare resa (redan genomförd) ===
        public void AddPreviousTrip()
        {
            UI.Transition("Add Previous Trip 🧳");

            // Fråga användaren om detaljer från resan
            string country = AnsiConsole.Ask<string>("Which [bold]country[/] did you visit?");
            string city = AnsiConsole.Ask<string>("Which [bold]city[/]?");
            string currency = AnsiConsole.Ask<string>("What [bold]currency[/]? (e.g. SEK, USD, EUR)");
            decimal budget = AskDecimal("What was your planned [bold]budget[/]?");
            decimal cost = AskDecimal("What was the total [bold]cost[/]?");

            // Kontrollera giltiga datum
            DateTime startDate, endDate;
            while (true)
            {
                startDate = AnsiConsole.Ask<DateTime>("Departure date (YYYY-MM-DD):");
                endDate = AnsiConsole.Ask<DateTime>("Return date (YYYY-MM-DD):");

                if (startDate > endDate)
                    UI.Warn("❌ Invalid date! Start date must be before end date.");
                else
                    break;
            }

            int passengers = AskInt("How many [bold]people[/] were on the trip?");
            int score = AskInt("How would you rate this trip (1–5)?");
            string review = AnsiConsole.Ask<string>("Write a short [bold]review[/]:");

            // Skapa ett nytt Trip-objekt med data
            var newTrip = new Trip
            {
                Country = country,
                City = city,
                Currency = currency,
                Cost = cost,
                PlannedBudget = budget,
                StartDate = startDate,
                EndDate = endDate,
                NumberOfPassengers = passengers,
                Score = score,
                Review = review,
                TimeZone = "Europe/Stockholm"
            };

            // Lägg till resan i listan och spara
            trips.Add(newTrip);
            SaveTrips();

            // Visa bekräftelse för användaren
            var panel = new Panel(
                $"[green]✅ Trip to [bold]{city}, {country}[/] added successfully![/]\n" +
                $"[grey]Budget:[/] {budget} {currency}\n" +
                $"[grey]Cost:[/] {cost} {currency}\n" +
                $"[grey]Rating:[/] {score}/5\n" +
                $"[grey]Dates:[/] {startDate:yyyy-MM-dd} → {endDate:yyyy-MM-dd}")
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader("Trip Saved", Justify.Center),
                BorderStyle = new Style(Color.Green)
            };
            AnsiConsole.Write(panel);
        }

        // === Visar alla resor i tabellform ===
        public void ShowAllTrips()
        {
            // Rubrik/avdelare
            UI.Transition($"All Trips for {username} 🌍");

            // Om användaren inte har några resor än
            if (trips.Count == 0)
            {
                UI.Warn("No trips found for this account.");
                return;
            }

            // Skapar en snygg tabell med Spectre.Console
            var table = new Table()
                .Centered()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey50);

            // Kolumnrubriker
            table.AddColumn("[bold cyan]Country[/]");
            table.AddColumn("[bold cyan]City[/]");
            table.AddColumn("[bold cyan]Dates[/]");
            table.AddColumn("[bold cyan]Budget[/]");
            table.AddColumn("[bold cyan]Cost[/]");
            table.AddColumn("[bold cyan]Status[/]");
            table.AddColumn("[bold cyan]Rating[/]");
            table.AddColumn("[bold cyan]Review[/]");

            // Fyll tabellen med data
            foreach (var trip in trips.OrderBy(t => t.StartDate))
            {
                string dateRange = $"{trip.StartDate:yyyy-MM-dd} → {trip.EndDate:yyyy-MM-dd}";
                string budget = $"{trip.PlannedBudget} {trip.Currency}";
                string cost = trip.Cost > 0 ? $"{trip.Cost} {trip.Currency}" : "[grey]—[/]";
                string rating = trip.Score > 0 ? $"{trip.Score}/5" : "[grey]—[/]";
                string review = string.IsNullOrWhiteSpace(trip.Review) ? "[grey]No review[/]" : trip.Review;

                // Bestäm färg beroende på status
                string statusText;
                Color color;

                if (trip.IsUpcoming)
                {
                    statusText = "Upcoming";
                    color = Color.Green;
                }
                else if (trip.IsCompleted)
                {
                    statusText = "Completed";
                    color = Color.Grey;
                }
                else
                {
                    statusText = "Ongoing";
                    color = Color.Yellow;
                }

                // Lägg till rad i tabellen
                table.AddRow(
                    new Markup($"[{color}]{trip.Country}[/]"),
                    new Markup($"[{color}]{trip.City}[/]"),
                    new Markup($"[{color}]{dateRange}[/]"),
                    new Markup($"[{color}]{budget}[/]"),
                    new Markup($"[{color}]{cost}[/]"),
                    new Markup($"[{color}]{statusText}[/]"),
                    new Markup($"[{color}]{rating}[/]"),
                    new Markup($"[{color}]{review}[/]")
                );
            }

            // Skriv ut tabellen i terminalen
            AnsiConsole.Write(table);

            // Skriv ut total antal resor som avslutning
            AnsiConsole.Write(new Rule($"[grey]Total trips: {trips.Count}[/]"));
        }

        public List<Trip> GetTrips()
        {
            return trips;
        }
    }
}
