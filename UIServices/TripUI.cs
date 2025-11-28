using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using Travel_Journal.Data;
using Travel_Journal.Models;
using Travel_Journal.Services;

namespace Travel_Journal.UIServices
{
    /// <summary>
    /// TripUI hanterar all interaktion med användaren.
    /// Här finns logiken för "Wizards" (steg-för-steg formulär), tabeller och felhantering/loggning.
    /// </summary>
    public class TripUI
    {
        // Referens till vår Service. UI är beroende av Service för att fungera.
        private readonly TripService _service;

        // Dependency Injection: Vi får in servicen via konstruktorn.
        public TripUI(TripService service)
        {
            _service = service;
        }

        // ============================================================
        // ===              ADD UPCOMING TRIP (WIZARD)              ===
        // ============================================================
        public void AddUpcomingTrip()
        {
            // Variabler som fylls på under formulärets gång
            int step = 0;
            string country = "";
            string city = "";
            decimal budget = 0;
            DateTime startDate = default;
            DateTime endDate = default;
            int passengers = 0;

            // While-loop möjliggör att användaren kan backa (step--) om de skrivit fel
            while (step < 6)
            {
                // Visar headern med aktuell data
                UI.ShowFormHeader("Add Upcoming Trip ✈️", country, city, budget, startDate, endDate, passengers);

                switch (step)
                {
                    case 0: // Land
                        var c = UI.AskWithBack("Which country are you visiting");
                        if (c == null) return; // Användaren valde "Back" -> Avsluta
                        country = c;
                        step++;
                        break;

                    case 1: // Stad
                        var ci = UI.AskStep("Which city?");
                        if (ci == null) { step--; UI.BackOneStep(); break; }
                        city = ci;
                        step++;
                        break;

                    case 2: // Budget
                        var b = UI.AskStepDecimal("Planned budget?");
                        if (b == null) { step--; UI.BackOneStep(); break; }
                        budget = b.Value;
                        step++;
                        break;

                    case 3: // Avresa
                        var s = UI.AskStepDate("Departure date (YYYY-MM-DD)");
                        if (s == null) { step--; UI.BackOneStep(); break; }
                        startDate = s.Value;
                        step++;
                        break;

                    case 4: // Hemresa
                        var e = UI.AskStepDate("Return date (YYYY-MM-DD)");
                        if (e == null) { step--; UI.BackOneStep(); break; }

                        // Validering: Hemresa får inte ske före avresa
                        if (startDate > e)
                        {
                            UI.Warn("Return date must be after departure date.");
                            // Vi gör break utan att plussa på step, så vi stannar på detta steg
                            break;
                        }
                        endDate = e.Value;
                        step++;
                        break;

                    case 5: // Passagerare
                        var p = UI.AskStepInt("How many passengers?");
                        if (p == null) { step--; UI.BackOneStep(); break; }
                        passengers = p.Value;
                        step++;
                        break;
                }
            }
            // Visar slutgiltig header med passengers inkluderat
            UI.ShowFormHeader("Add Upcoming Trip ✈️", country, city, budget, startDate, endDate, passengers);

            // När loopen är klar skapar vi objektet
            var newTrip = new Trip
            {
                Country = country,
                City = city,
                PlannedBudget = budget,
                StartDate = startDate,
                EndDate = endDate,
                NumberOfPassengers = passengers
            };

            // Använd hjälpmetoden för att spara
            SaveTripSafe(newTrip, $"Trip to {city}, {country} added successfully!");
        }

        // ============================================================
        // ===              ADD PREVIOUS TRIP (WIZARD)              ===
        // ============================================================
        public void AddPreviousTrip()
        {
            // Variabler
            int step = 0;
            string country = "";
            string city = "";
            decimal budget = 0;
            decimal cost = 0;
            DateTime startDate = default;
            DateTime endDate = default;
            int passengers = 0;
            int score = 0;
            string review = "";

            // Loopar tills alla 9 steg är klara
            while (step < 9)
            {
                // VIKTIGT: Vi visar headern här, en gång, istället för i varje case.
                // Detta gör koden mycket renare (DRY - Don't Repeat Yourself).
                UI.ShowFormHeader("Add Previous Trip 🧳", country, city, budget, startDate, endDate, passengers, cost, score, review);

                switch (step)
                {
                    case 0: // Land
                        var c = UI.AskWithBack("Which country did you visit");
                        if (c == null) return; // Avsluta om användaren backar från första steget
                        country = c;
                        step++;
                        break;

                    case 1: // Stad
                        var ci = UI.AskStep("Which city?");
                        if (ci == null) { step--; UI.BackOneStep(); break; }
                        city = ci;
                        step++;
                        break;

                    case 2: // Budget
                        var b = UI.AskStepDecimal("Planned budget?");
                        if (b == null) { step--; UI.BackOneStep(); break; }
                        budget = b.Value;
                        step++;
                        break;

                    case 3: // Kostnad
                        var cst = UI.AskStepDecimal("Total cost?");
                        if (cst == null) { step--; UI.BackOneStep(); break; }
                        cost = cst.Value;
                        step++;
                        break;

                    case 4: // Startdatum
                        var s = UI.AskStepDate("Departure date (YYYY-MM-DD)");
                        if (s == null) { step--; UI.BackOneStep(); break; }
                        startDate = s.Value;
                        step++;
                        break;

                    case 5: // Slutdatum (med validering)
                        var e = UI.AskStepDate("Return date (YYYY-MM-DD)");
                        if (e == null) { step--; UI.BackOneStep(); break; }

                        if (startDate > e)
                        {
                            UI.Warn("Return date must be after departure date.");
                            Logg.Log($"User '{_service.UserName}' entered invalid return date in AddPreviousTrip.");
                            // Vi stannar kvar på detta steg (ingen step++)
                            break;
                        }
                        endDate = e.Value;
                        step++;
                        break;

                    case 6: // Passagerare
                        var p = UI.AskStepInt("How many passengers?");
                        if (p == null) { step--; UI.BackOneStep(); break; }
                        passengers = p.Value;
                        step++;
                        break;

                    case 7: // Betyg (Specifikt för Previous Trip)
                        var r = UI.AskStepDecimal("Trip rating (1–5)");
                        if (r == null) { step--; UI.BackOneStep(); break; }

                        int rating = (int)r.Value;
                        if (rating < 1 || rating > 5)
                        {
                            UI.Warn("Rating must be between 1–5.");
                            Logg.Log($"User '{_service.UserName}' entered invalid rating in AddPreviousTrip.");
                            break;
                        }
                        score = rating;
                        step++;
                        break;

                    case 8: // Recension
                        var rv = UI.AskStep("Write a short review");
                        if (rv == null) { step--; UI.BackOneStep(); break; }
                        review = rv;
                        step++;
                        break;
                }
            }

            // Spara resan när loopen är klar
            var newTrip = new Trip
            {
                Country = country,
                City = city,
                PlannedBudget = budget,
                Cost = cost,
                StartDate = startDate,
                EndDate = endDate,
                NumberOfPassengers = passengers,
                Score = score,
                Review = review
            };

            var notificationService = new NotificationService();//skapar ett nytt objekt och lagrar den i en variabel
            var notificationType = notificationService.AssessBudgetOutcome(newTrip.PlannedBudget, newTrip.Cost);// Använder servicen för att bedöma budgetutfallet
                                                                                                                // Detta tar hand om inputen-vad som kommer in
            NotificationUI.ShowBudgetNotification(notificationType, newTrip.PlannedBudget, newTrip.Cost);//Använder NotificationUI för att visa meddelandet
                                                                                                         //Detta tar hand om outputen...vad som användaren ser

            SaveTripSafe(newTrip, $"Previous trip to {city}, {country} saved successfully!");
        }

        /// <summary>
        /// Hjälpmetod för att spara en ny resa och hantera eventuella fel.
        /// </summary>
        private void SaveTripSafe(Trip trip, string successMessage)
        {
            try
            {
                // Anropa servicen för att spara datan
                _service.AddTrip(trip);
                UI.Success(successMessage);
            }
            catch (Exception ex)
            {
                // Om något går fel (t.ex. skrivskyddad fil), fånga felet, visa för användaren och LOGGA det.
                UI.Error($"Failed to save trips: {ex.Message}");
                Logg.Log($"ERROR saving trips for '{_service.UserName}': {ex.Message}");
            }
            UI.Pause();
        }

        // ============================================================
        // ===              SHOW ALL TRIPS                          ===
        // ============================================================
        public void ShowAllTrips()
        {
            AnsiConsole.Clear();
            UI.Transition($"All Trips for {_service.UserName} 🌍");

            // Hämta datan från servicen
            var trips = _service.GetAllTrips();

            // Om ingen resa hittas
            if (!trips.Any())
            {
                UI.Warn("No trips found for this account.");
                Logg.Log($"No trips found for user '{_service.UserName}' in ShowAllTrips.");
                return;
            }

            // Skapa kategorier för tabellen (LINQ)
            var categories = new List<(string Title, Color Color, List<Trip> List)>
            {
                ("Ongoing Trips",   Color.Yellow, trips.Where(t => !t.IsUpcoming && !t.IsCompleted).OrderBy(t => t.StartDate).ToList()),
                ("Upcoming Trips",  Color.Green,  trips.Where(t => t.IsUpcoming).OrderBy(t => t.StartDate).ToList()),
                ("Completed Trips", Color.Grey,   trips.Where(t => t.IsCompleted).OrderBy(t => t.StartDate).ToList())
            };

            // Loopa igenom varje kategori och rita en tabell
            foreach (var (title, color, tripList) in categories)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Rule($"[bold {color}]{title}[/]") { Justification = Justify.Center });

                if (!tripList.Any())
                {
                    AnsiConsole.MarkupLine("[grey]No trips in this category.[/]");
                    continue;
                }

                var table = new Table().Centered().Border(TableBorder.Rounded).BorderColor(Color.Grey50);
                table.AddColumn("[bold cyan]Country[/]");
                table.AddColumn("[bold cyan]City[/]");
                table.AddColumn("[bold cyan]Dates[/]");
                table.AddColumn("[bold cyan]Budget[/]");
                table.AddColumn("[bold cyan]Cost[/]");
                table.AddColumn("[bold cyan]Status[/]");
                table.AddColumn("[bold cyan]Rating[/]");
                table.AddColumn("[bold cyan]Review[/]");
                table.AddColumn("[bold cyan]Pax[/]");
                
                foreach (var trip in tripList)
                {
                    string dateRange = $"{trip.StartDate:yyyy-MM-dd} → {trip.EndDate:yyyy-MM-dd}";
                    string budgetStr = $"{trip.PlannedBudget}";
                    string costStr = trip.Cost > 0 ? $"{trip.Cost}" : "[grey]—[/]";
                    string rating = trip.Score > 0 ? $"{trip.Score}/5" : "[grey]—[/]";
                    string reviewText = string.IsNullOrWhiteSpace(trip.Review) ? "[grey]No review[/]" : trip.Review;
                    string statusText = trip.IsUpcoming ? "Upcoming" : trip.IsCompleted ? "Completed" : "Ongoing";

                    table.AddRow(
                        new Markup($"[{color}]{trip.Country}[/]"),
                        new Markup($"[{color}]{trip.City}[/]"),
                        new Markup($"[{color}]{dateRange}[/]"),
                        new Markup($"[{color}]{budgetStr}[/]"),
                        new Markup($"[{color}]{costStr}[/]"),
                        new Markup($"[{color}]{statusText}[/]"),
                        new Markup($"[{color}]{rating}[/]"),
                        new Markup($"[{color}]{reviewText}[/]"),
                        new Markup($"[{color}]{trip.NumberOfPassengers}[/]")
                    );
                }
                AnsiConsole.Write(table);
            }

            AnsiConsole.Write(new Rule($"[grey]Total trips: {trips.Count}[/]") { Justification = Justify.Center });
        }
    }
}