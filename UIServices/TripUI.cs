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
                // STEG 0: Land
                if (step == 0)
                {
                    // Visar en snygg header med vad som är ifyllt hittills
                    UI.ShowFormHeader("Add Upcoming Trip ✈️", country, city, budget, startDate, endDate, passengers);

                    var c = UI.AskWithBack("Which country are you visiting");
                    if (c == null) return; // Användaren valde "Back" -> Avsluta

                    country = c;
                    step++;
                }
                // STEG 1: Stad
                else if (step == 1)
                {
                    UI.ShowFormHeader("Add Upcoming Trip ✈️", country, city, budget, startDate, endDate, passengers);
                    var c = UI.AskStep("Which city?");

                    // Om användaren vill backa härifrån...
                    if (c == null)
                    {
                        step--; // ...gå tillbaka till steg 0
                        UI.BackOneStep();
                        continue;
                    }
                    city = c;
                    step++;
                }
                // STEG 2: Budget
                else if (step == 2)
                {
                    UI.ShowFormHeader("Add Upcoming Trip ✈️", country, city, budget, startDate, endDate, passengers);
                    var b = UI.AskStepDecimal("Planned budget?");
                    if (b == null) { step--; UI.BackOneStep(); continue; }
                    budget = b.Value;
                    step++;
                }
                // STEG 3: Avresa
                else if (step == 3)
                {
                    UI.ShowFormHeader("Add Upcoming Trip ✈️", country, city, budget, startDate, endDate, passengers);
                    var s = UI.AskStepDate("Departure date (YYYY-MM-DD)");
                    if (s == null) { step--; UI.BackOneStep(); continue; }
                    startDate = s.Value;
                    step++;
                }
                // STEG 4: Hemresa
                else if (step == 4)
                {
                    UI.ShowFormHeader("Add Upcoming Trip ✈️", country, city, budget, startDate, endDate, passengers);
                    var e = UI.AskStepDate("Return date (YYYY-MM-DD)");
                    if (e == null) { step--; UI.BackOneStep(); continue; }

                    // Validering: Hemresa får inte ske före avresa
                    if (startDate > e)
                    {
                        UI.Warn("Return date must be after departure date.");
                        continue; // Låt användaren försöka igen på samma steg
                    }
                    endDate = e.Value;
                    step++;
                }
                // STEG 5: Passagerare
                else if (step == 5)
                {
                    UI.ShowFormHeader("Add Upcoming Trip ✈️", country, city, budget, startDate, endDate, passengers);
                    var p = UI.AskStepInt("How many passengers?");
                    if (p == null) { step--; UI.BackOneStep(); continue; }
                    passengers = p.Value;
                    step++;
                }
            }

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

            // Använd hjälpmetoden för att spara (den sköter try-catch och loggning)
            SaveTripSafe(newTrip, $"Trip to {city}, {country} added successfully!");
        }

        // ============================================================
        // ===              ADD PREVIOUS TRIP (WIZARD)              ===
        // ============================================================
        public void AddPreviousTrip()
        {
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

            // Samma logik som ovan, men med 9 steg (inkluderar betyg och recension)
            while (step < 9)
            {
                if (step == 0)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳", country, city, budget, startDate, endDate, passengers, cost, score, review);
                    var c = UI.AskWithBack("Which country did you visit");
                    if (c == null) return;
                    country = c;
                    step++;
                }
                else if (step == 1)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳", country, city, budget, startDate, endDate, passengers, cost, score, review);
                    var c = UI.AskStep("Which city?");
                    if (c == null) { step--; UI.BackOneStep(); continue; }
                    city = c;
                    step++;
                }
                else if (step == 2)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳", country, city, budget, startDate, endDate, passengers, cost, score, review);
                    var b = UI.AskStepDecimal("Planned budget?");
                    if (b == null) { step--; UI.BackOneStep(); continue; }
                    budget = b.Value;
                    step++;
                }
                else if (step == 3)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳", country, city, budget, startDate, endDate, passengers, cost, score, review);
                    var cst = UI.AskStepDecimal("Total cost?");
                    if (cst == null) { step--; UI.BackOneStep(); continue; }
                    cost = cst.Value;
                    step++;
                }
                else if (step == 4)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳", country, city, budget, startDate, endDate, passengers, cost, score, review);
                    var s = UI.AskStepDate("Departure date (YYYY-MM-DD)");
                    if (s == null) { step--; UI.BackOneStep(); continue; }
                    startDate = s.Value;
                    step++;
                }
                else if (step == 5)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳", country, city, budget, startDate, endDate, passengers, cost, score, review);
                    var e = UI.AskStepDate("Return date (YYYY-MM-DD)");
                    if (e == null) { step--; UI.BackOneStep(); continue; }
                    if (startDate > e)
                    {
                        UI.Warn("Return date must be after departure date.");
                        Logg.Log($"User '{_service.UserName}' entered invalid return date in AddPreviousTrip.");
                        continue;
                    }
                    endDate = e.Value;
                    step++;
                }
                else if (step == 6)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳", country, city, budget, startDate, endDate, passengers, cost, score, review);
                    var p = UI.AskStepInt("How many passengers?");
                    if (p == null) { step--; UI.BackOneStep(); continue; }
                    passengers = p.Value;
                    step++;
                }
                // Specifikt för Previous Trip: Betyg
                else if (step == 7)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳", country, city, budget, startDate, endDate, passengers, cost, score, review);
                    var r = UI.AskStepDecimal("Trip rating (1–5)");
                    if (r == null) { step--; UI.BackOneStep(); continue; }

                    int rating = (int)r.Value;
                    if (rating < 1 || rating > 5)
                    {
                        UI.Warn("Rating must be between 1–5.");
                        Logg.Log($"User '{_service.UserName}' entered invalid rating in AddPreviousTrip.");
                        continue;
                    }
                    score = rating;
                    step++;
                }
                else if (step == 8)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳", country, city, budget, startDate, endDate, passengers, cost, score, review);
                    var rv = UI.AskStep("Write a short review");
                    if (rv == null) { step--; UI.BackOneStep(); continue; }
                    review = rv;
                    step++;
                }
            }

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

        // ============================================================
        // ===         GENERISK UPPDATERINGS-METOD (Smartare kod)   ===
        // ============================================================

        /// <summary>
        /// En generisk metod som hanterar uppdatering av alla typer av data (int, decimal, DateTime).
        /// Denna metod ersätter alla repetitiva UpdateX-metoder och minskar koden drastiskt.
        /// </summary>
        /// <typeparam name="T">Datatypen vi jobbar med (t.ex. decimal eller int)</typeparam>
        private void UpdateTripProperty<T>(
            string propertyName, // Vad heter egenskapen? T.ex. "Budget"
            Func<Trip, string> displaySelector, // Hur ska resan visas i listan?
            Func<string, (bool IsValid, T Value, string ErrorMsg)> validatorAndParser, // Funktion som validerar och loggar fel
            Action<Trip, T> updateAction) // Funktion som sätter det nya värdet
        {
            // Hämta data via servicen
            var trips = _service.GetAllTrips();

            if (trips == null || !trips.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                Logg.Log($"No trips available to update {propertyName} for user '{_service.UserName}'.");
                UI.Pause();
                return;
            }

            // Låt användaren välja en resa
            var selectedTrip = AnsiConsole.Prompt(
                new SelectionPrompt<Trip>()
                    .Title($"[bold]Select a trip to update its {propertyName}:[/]")
                    .HighlightStyle(new Style(Color.DeepSkyBlue1))
                    .UseConverter(displaySelector)
                    .AddChoices(trips)
            );

            // Ta emot input som STRÄNG först, för att kunna logga exakt vad användaren skrev om det är fel
            var rawInput = AnsiConsole.Prompt(
                new TextPrompt<string>($"Enter the new {propertyName.ToLower()}:")
                    .Validate(input =>
                    {
                        // Kör valideringsfunktionen vi fick in via parametern
                        var result = validatorAndParser(input);

                        if (result.IsValid) return ValidationResult.Success();

                        // Om ogiltig: Felet är redan loggat inuti validatorn, returnera bara meddelandet till skärmen
                        return ValidationResult.Error(result.ErrorMsg);
                    })
            );

            // Konvertera till rätt typ (nu vet vi att det är säkert)
            var parsedValue = validatorAndParser(rawInput).Value;

            // Bekräfta ändring
            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Do you want to update {propertyName} for [bold]{selectedTrip.City}[/] to [bold]{parsedValue}[/]?")
                    .AddChoices("✅ Yes", "❌ No")
            );

            if (confirm == "❌ No")
            {
                AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                Logg.Log($"User '{_service.UserName}' cancelled {propertyName} update.");
                UI.Pause();
                return;
            }

            // Utför uppdateringen och spara
            try
            {
                updateAction(selectedTrip, parsedValue);
                _service.Save(); // Spara via Service
                AnsiConsole.MarkupLine($"[green]✅ {propertyName} updated successfully![/]");
            }
            catch (Exception ex)
            {
                UI.Error($"Failed to save: {ex.Message}");
                Logg.Log($"ERROR saving {propertyName} update: {ex.Message}");
            }
            UI.Pause();
        }

        // ============================================================
        // ===         PUBLIKA UPDATE-METODER (Använder generiska)  ===
        // ============================================================

        public void UpdateBudget()
        {
            UpdateTripProperty<decimal>("Budget",
                t => $"{t.City}, {t.Country} | Budget: {t.PlannedBudget}",
                input =>
                {
                    // Här ligger logiken för validering OCH loggning av fel
                    if (!decimal.TryParse(input, out var val))
                    {
                        Logg.Log($"Invalid budget input: '{input}'. Not a number.");
                        return (false, 0, "[red]Must be a number[/]");
                    }
                    if (val < 0)
                    {
                        Logg.Log($"Invalid budget input: '{input}'. Negative.");
                        return (false, 0, "[red]Must be positive[/]");
                    }
                    return (true, val, "");
                },
                // Allt gick bra! Skicka tillbaka det konverterade värdet (val).
                (t, val) => t.PlannedBudget = val
            );
        }

        public void UpdateCost()
        {
            UpdateTripProperty<decimal>("Cost",
                t => $"{t.City}, {t.Country} | Cost: {t.Cost}",
                input =>
                {
                    if (!decimal.TryParse(input, out var val))
                    {
                        Logg.Log($"Invalid cost input: '{input}'. Not a number.");
                        return (false, 0, "[red]Must be a number[/]");
                    }
                    if (val < 0)
                    {
                        Logg.Log($"Invalid cost input: '{input}'. Negative.");
                        return (false, 0, "[red]Must be positive[/]");
                    }
                    return (true, val, "");
                },
                (t, val) => t.Cost = val
            );
        }

        public void UpdateRating()
        {
            UpdateTripProperty<int>("Rating",
                t => $"{t.City} | Score: {t.Score}/5",
                input =>
                {
                    if (!int.TryParse(input, out var val))
                    {
                        Logg.Log($"Invalid rating input: '{input}'. Not an integer.");
                        return (false, 0, "[red]Must be a whole number[/]");
                    }
                    if (val < 1 || val > 5)
                    {
                        Logg.Log($"Invalid rating input: '{val}'. Out of range.");
                        return (false, 0, "[red]Rating must be 1-5[/]");
                    }
                    return (true, val, "");
                },
                (t, val) => t.Score = val
            );
        }

        public void UpdateNumberOfPassengers()
        {
            UpdateTripProperty<int>("Passengers",
                t => $"{t.City} | Pax: {t.NumberOfPassengers}",
                input =>
                {
                    if (!int.TryParse(input, out var val))
                    {
                        Logg.Log($"Invalid pax input: '{input}'.");
                        return (false, 0, "[red]Must be a number[/]");
                    }
                    if (val < 1)
                    {
                        Logg.Log($"Invalid pax input: '{val}'. Too low.");
                        return (false, 0, "[red]Must be at least 1[/]");
                    }
                    return (true, val, "");
                },
                (t, val) => t.NumberOfPassengers = val
            );
        }

        public void UpdateDepartDate()
        {
            UpdateTripProperty<DateTime>("Departure Date",
                t => $"{t.City} | Starts: {t.StartDate:yyyy-MM-dd}",
                input =>
                {
                    if (!DateTime.TryParse(input, out var val))
                    {
                        Logg.Log($"Invalid date input: '{input}'.");
                        return (false, default, "[red]Format: YYYY-MM-DD[/]");
                    }
                    return (true, val, "");
                },
                (t, val) => t.StartDate = val
            );
        }

        public void UpdateReturnDate()
        {
            UpdateTripProperty<DateTime>("Return Date",
                t => $"{t.City} | Ends: {t.EndDate:yyyy-MM-dd}",
                input =>
                {
                    if (!DateTime.TryParse(input, out var val))
                    {
                        Logg.Log($"Invalid date input: '{input}'.");
                        return (false, default, "[red]Format: YYYY-MM-DD[/]");
                    }
                    return (true, val, "");
                },
                (t, val) => t.EndDate = val
            );
        }

        // DeleteTrip är lite speciell (tar bort istället för uppdaterar), så den ligger separat
        public void DeleteTrip()
        {
            var trips = _service.GetAllTrips();
            if (!trips.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No trips to delete.[/]");
                return;
            }

            var selectedTrip = AnsiConsole.Prompt(
                new SelectionPrompt<Trip>()
                    .Title("[bold red]Select a trip to [underline]delete[/]:[/]")
                    .HighlightStyle(new Style(Color.Red))
                    .UseConverter(t => $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd})")
                    .AddChoices(trips)
            );

            if (AnsiConsole.Confirm($"Are you sure you want to delete {selectedTrip.City}?"))
            {
                try
                {
                    _service.DeleteTrip(selectedTrip);
                    AnsiConsole.MarkupLine($"[green]Trip deleted successfully![/]");
                }
                catch (Exception ex)
                {
                    Logg.Log($"Error deleting trip: {ex.Message}");
                    UI.Error("Could not delete trip.");
                }
            }
            else
            {
                Logg.Log($"User '{_service.UserName}' cancelled deletion.");
            }
            UI.Pause();
        }
    }
}