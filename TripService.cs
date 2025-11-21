using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Travel_Journal
{
    public class TripService
    {
        // === Fält ===
        // Lista med alla resor för användaren
        private List<Trip> trips = new();

        // Användarens namn (för filidentifiering)
        private readonly string username;
        public string UserName => username;

        // Hanterar laddning och sparning av trips till JSON
        private readonly DataStore<Trip> store;

        // === Konstruktor ===
        public TripService(string username)
        {
            this.username = username;

            // DataStore använder automatiskt ./data
            store = new DataStore<Trip>($"{username}_trips.json");

            // Ladda resor om filen finns
            trips = store.Load();
        }

        // === Spara alla resor till JSON-fil ===
        private void SaveTrips()
        {
            try
            {
                store.Save(trips);
            }
            catch (Exception ex)
            {
                UI.Error($"Failed to save trips: {ex.Message}");
                Logg.Log($"ERROR saving trips for '{username}': {ex.Message}");
            }
        }


        // ============================================================
        // === Add Upcoming Trip (Framtida resa) =======================
        // ============================================================
        // === Add Upcoming Trip (separerat datum) ===
        public void AddUpcomingTrip()
        {
            int step = 0;

            string country = "";
            string city = "";
            decimal budget = 0;
            DateTime startDate = default;
            DateTime endDate = default;
            int passengers = 0;

            while (step < 6)
            {
                // === STEP 0 — COUNTRY ===
                if (step == 0)
                {
                    UI.ShowFormHeader("Add Upcoming Trip ✈️",
                        country, city, budget, startDate, endDate, passengers);

                    var c = UI.AskWithBack("Which country are you visiting");
                    if (c == null) return;

                    country = c;
                    step++;
                }

                // === STEP 1 — CITY ===
                else if (step == 1)
                {
                    UI.ShowFormHeader("Add Upcoming Trip ✈️",
                        country, city, budget, startDate, endDate, passengers);

                    var c = UI.AskStep("Which city?");
                    if (c == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    city = c;
                    step++;
                }

                // === STEP 2 — BUDGET ===
                else if (step == 2)
                {
                    UI.ShowFormHeader("Add Upcoming Trip ✈️",
                        country, city, budget, startDate, endDate, passengers);

                    var b = UI.AskStepDecimal("Planned budget?");
                    if (b == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    budget = b.Value;
                    step++;
                }

                // === STEP 3 — DEPARTURE DATE ===
                else if (step == 3)
                {
                    UI.ShowFormHeader("Add Upcoming Trip ✈️",
                        country, city, budget, startDate, endDate, passengers);

                    var s = UI.AskStepDate("Departure date (YYYY-MM-DD)");
                    if (s == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    startDate = s.Value;
                    step++;
                }

                // === STEP 4 — RETURN DATE ===
                else if (step == 4)
                {
                    UI.ShowFormHeader("Add Upcoming Trip ✈️",
                        country, city, budget, startDate, endDate, passengers);

                    var e = UI.AskStepDate("Return date (YYYY-MM-DD)");
                    if (e == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    if (startDate > e)
                    {
                        UI.Warn("Return date must be after departure date.");
                        continue;
                    }

                    endDate = e.Value;
                    step++;
                }

                // === STEP 5 — PASSENGERS ===
                else if (step == 5)
                {
                    UI.ShowFormHeader("Add Upcoming Trip ✈️",
                        country, city, budget, startDate, endDate, passengers);

                    var p = UI.AskStepInt("How many passengers?");
                    if (p == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    passengers = (int)p.Value;
                    step++;
                }
            }

            var newTrip = new Trip
            {
                Country = country,
                City = city,
                PlannedBudget = budget,
                StartDate = startDate,
                EndDate = endDate,
                NumberOfPassengers = passengers
            };

            trips.Add(newTrip);
            SaveTrips();

            UI.Success($"Trip to {city}, {country} added successfully!");
            UI.Pause();
        }



        // ============================================================
        // === Add Previous Trip (Genomförd resa) ======================
        // ============================================================
        // === Lägger till en tidigare resa (Previous Trip) — nu separerade datumfält ===
        public void AddPreviousTrip()
        {
            int step = 0; // Håller koll på vilket steg vi befinner oss i

            // Variabler som fylls i steg för steg
            string country = "";
            string city = "";
            decimal budget = 0;
            decimal cost = 0;
            DateTime startDate = default;
            DateTime endDate = default;
            int passengers = 0;
            int score = 0;
            string review = "";

            // Totalt 9 steg (land, stad, budget, cost, avresa, hemresa, pass, rating, review)
            while (step < 9)
            {
                // === STEG 0 — LAND ===
                if (step == 0)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳",
                        country, city, budget, startDate, endDate, passengers,
                        cost, score, review);

                    var c = UI.AskWithBack("Which country did you visit");
                    if (c == null)
                        return; // back to menu

                    country = c;
                    step++;
                }

                // === STEG 1 — STAD ===
                else if (step == 1)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳",
                        country, city, budget, startDate, endDate, passengers,
                        cost, score, review);

                    var c = UI.AskStep("Which city?");
                    if (c == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    city = c;
                    step++;
                }

                // === STEG 2 — PLANERAD BUDGET ===
                else if (step == 2)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳",
                        country, city, budget, startDate, endDate, passengers,
                        cost, score, review);

                    var b = UI.AskStepDecimal("Planned budget?");
                    if (b == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    budget = b.Value;
                    step++;
                }

                // === STEG 3 — TOTAL KOSTNAD ===
                else if (step == 3)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳",
                        country, city, budget, startDate, endDate, passengers,
                        cost, score, review);

                    var cst = UI.AskStepDecimal("Total cost?");
                    if (cst == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    cost = cst.Value;
                    step++;
                }

                // === STEG 4 — AVLÄSNING DATUM (SEPARAT) ===
                else if (step == 4)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳",
                        country, city, budget, startDate, endDate, passengers,
                        cost, score, review);

                    var s = UI.AskStepDate("Departure date (YYYY-MM-DD)");
                    if (s == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    startDate = s.Value;
                    step++;
                }

                // === STEG 5 — HEMRESA DATUM (SEPARAT) ===
                else if (step == 5)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳",
                        country, city, budget, startDate, endDate, passengers,
                        cost, score, review);

                    var e = UI.AskStepDate("Return date (YYYY-MM-DD)");
                    if (e == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    if (startDate > e)
                    {
                        UI.Warn("Return date must be after departure date.");
                        Logg.Log($"User '{username}' entered invalid return date in AddPreviousTrip.");
                        continue;
                    }

                    endDate = e.Value;
                    step++;
                }

                // === STEG 6 — PASSAGERARE ===
                else if (step == 6)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳",
                        country, city, budget, startDate, endDate, passengers,
                        cost, score, review);

                    var p = UI.AskStepInt("How many passengers?");
                    if (p == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    passengers = (int)p.Value;
                    step++;
                }

                // === STEG 7 — BETYG ===
                else if (step == 7)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳",
                        country, city, budget, startDate, endDate, passengers,
                        cost, score, review);

                    var r = UI.AskStepDecimal("Trip rating (1–5)");
                    if (r == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    int rating = (int)r.Value;

                    if (rating < 1 || rating > 5)
                    {
                        UI.Warn("Rating must be between 1–5.");
                        Logg.Log($"User '{username}' entered invalid rating in AddPreviousTrip.");
                        continue;
                    }

                    score = rating;
                    step++;
                }

                // === STEG 8 — RECENSION ===
                else if (step == 8)
                {
                    UI.ShowFormHeader("Add Previous Trip 🧳",
                        country, city, budget, startDate, endDate, passengers,
                        cost, score, review);

                    var rv = UI.AskStep("Write a short review");
                    if (rv == null)
                    {
                        step--;
                        UI.BackOneStep();
                        continue;
                    }

                    review = rv;
                    step++;
                }
            }

            // === SKAPA & SPARA RESAN ===
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

            trips.Add(newTrip);
            SaveTrips();

            UI.Success($"Previous trip to {city}, {country} saved successfully!");
            UI.Pause();
        }

        // === Visar alla resor i tabellform ===
        public void ShowAllTrips()
        {
            UI.Transition($"All Trips for {username} 🌍");

            if (!trips.Any())
            {
                UI.Warn("No trips found for this account.");
                Logg.Log($"No trips found for user '{username}' in ShowAllTrips.");
                return;
            }

            // --- 1. Skapa kategorier ---
            var categories = new List<(string Title, Color Color, List<Trip> List)>
    {
        ("Ongoing Trips",   Color.Yellow, trips.Where(t => !t.IsUpcoming && !t.IsCompleted).OrderBy(t => t.StartDate).ToList()),
        ("Upcoming Trips",  Color.Green,  trips.Where(t => t.IsUpcoming).OrderBy(t => t.StartDate).ToList()),
        ("Completed Trips", Color.Grey,   trips.Where(t => t.IsCompleted).OrderBy(t => t.StartDate).ToList())
    };

            // --- 2. Loopa och visa varje kategori ---
            foreach (var (title, color, tripList) in categories)
            {
                AnsiConsole.WriteLine();

                var rule = new Rule($"[bold {color}]{title}[/]")
                {
                    Justification = Justify.Center
                };
                AnsiConsole.Write(rule);

                if (!tripList.Any())
                {
                    AnsiConsole.MarkupLine("[grey]No trips in this category.[/]");
                    Logg.Log($"No trips found in category '{title}' for user '{username}'.");
                    continue;
                }

                var table = new Table()
                    .Centered()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Grey50);

                // Kolumner
                table.AddColumn("[bold cyan]Country[/]");
                table.AddColumn("[bold cyan]City[/]");
                table.AddColumn("[bold cyan]Dates[/]");
                table.AddColumn("[bold cyan]Budget[/]");
                table.AddColumn("[bold cyan]Cost[/]");
                table.AddColumn("[bold cyan]Status[/]");
                table.AddColumn("[bold cyan]Rating[/]");
                table.AddColumn("[bold cyan]Review[/]");
                table.AddColumn("[bold cyan]Pax[/]");

                // Fyll tabell
                foreach (var trip in tripList)
                {
                    string dateRange = $"{trip.StartDate:yyyy-MM-dd} → {trip.EndDate:yyyy-MM-dd}";
                    string budget = $"{trip.PlannedBudget}";
                    string cost = trip.Cost > 0 ? $"{trip.Cost}" : "[grey]—[/]";
                    string rating = trip.Score > 0 ? $"{trip.Score}/5" : "[grey]—[/]";
                    string review = string.IsNullOrWhiteSpace(trip.Review) ? "[grey]No review[/]" : trip.Review;
                    string passengers = $"{trip.NumberOfPassengers}";

                    string statusText = trip.IsUpcoming ? "Upcoming" :
                                        trip.IsCompleted ? "Completed" : "Ongoing";

                    table.AddRow(
                        new Markup($"[{color}]{trip.Country}[/]"),
                        new Markup($"[{color}]{trip.City}[/]"),
                        new Markup($"[{color}]{dateRange}[/]"),
                        new Markup($"[{color}]{budget}[/]"),
                        new Markup($"[{color}]{cost}[/]"),
                        new Markup($"[{color}]{statusText}[/]"),
                        new Markup($"[{color}]{rating}[/]"),
                        new Markup($"[{color}]{review}[/]"),
                        new Markup($"[{color}]{passengers}[/]")
                    );
                }

                AnsiConsole.Write(table);
            }

            // ---- Footer ----
            var footer = new Rule($"[grey]Total trips: {trips.Count}[/]")
            {
                Justification = Justify.Center
            };
            AnsiConsole.Write(footer);
        }
        public List<Trip> GetTrips() //Hjälpmetod för att hämta resor.
        {
            return trips;
        }

        // === ⭐ Uppdatera betyg ===
        public void UpdateRating(List<Trip> updatedTrips)
        {
            if (updatedTrips is null || updatedTrips.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                Logg.Log($"No trips available to update rating for user '{username}'.");
                UI.Pause();
                return;
            }

            var selectedTrip = AnsiConsole.Prompt(
                new SelectionPrompt<Trip>()
                    .Title("[bold]Select a trip to update its rating:[/]")
                    .HighlightStyle(new Style(Color.DeepSkyBlue1))
                    .UseConverter(t => $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd}) | Score: {t.Score}")
                    .AddChoices(updatedTrips)
            );

            var newScore = AnsiConsole.Prompt(
                new TextPrompt<int>("Enter the new rating [[1-5]]:")
                    .Validate(s =>
                    {
                        if (s is >= 1 and <= 5)
                            return ValidationResult.Success();
                        Logg.Log($"Invalid rating input: '{s}'. Must be between 1 and 5.");
                        return ValidationResult.Error("[red]Rating must be between 1 and 5[/]");
                    })
            );

            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Do you want to update the rating for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] to [bold]{newScore}[/]?")
                    .AddChoices("✅ Yes", "❌ No")
            );

            if (confirm == "❌ No")
            {
                AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                Logg.Log($"User '{username}' cancelled rating update for trip to '{selectedTrip.City}, {selectedTrip.Country}'.");
                UI.Pause();
                return;
            }

            selectedTrip.Score = newScore;
            AnsiConsole.MarkupLine($"[green]✅ Rating updated to {newScore}/5 for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]");
            UI.Pause();
        }

        // Metod för att uppdatera rating
        public void UpdateDepartDate(List<Trip> updatedTrips)
        {
            if (updatedTrips is null || updatedTrips.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                Logg.Log($"No trips available to update departure date for user '{username}'.");
                UI.Pause();
                return;
            }

            var selectedTrip = AnsiConsole.Prompt(
                new SelectionPrompt<Trip>()
                    .Title("[bold]Select a trip to update the date of depart:[/]")
                    .HighlightStyle(new Style(Color.DeepSkyBlue1))
                    .UseConverter(t => $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd})")
                    .AddChoices(updatedTrips)
            );

            var rawDate = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the new date [[YYYY-MM-DD]]:")
                    .Validate(input =>
                    {
                        if (DateTime.TryParse(input, out _))
                            return ValidationResult.Success();

                        Logg.Log($"Invalid date input: '{input}'. Expected format YYYY-MM-DD.");
                        return ValidationResult.Error("[red]Date must be in format YYYY-MM-DD[/]");
                    })
            );

            var newDateOfDepart = DateTime.Parse(rawDate);

            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Do you want to update the departure date for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] from {selectedTrip.StartDate:yyyy-MM-dd} to [bold]{newDateOfDepart:yyyy-MM-dd}[/]?")
                    .AddChoices("✅ Yes", "❌ No")
            );

            if (confirm == "❌ No")
            {
                AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                Logg.Log($"User '{username}' cancelled departure date update for trip to '{selectedTrip.City}, {selectedTrip.Country}'.");
                UI.Pause();
                return;
            }

            selectedTrip.StartDate = newDateOfDepart;
            AnsiConsole.MarkupLine($"[green]✅ Departure date updated for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]");
            UI.Pause();
        }

        // Metod för att uppdatera return date
        public void UpdateReturnDate(List<Trip> updatedTrips)
        {
            if (updatedTrips is null || updatedTrips.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                Logg.Log($"No trips available to update return date for user '{username}'.");
                UI.Pause();
                return;
            }

            var selectedTrip = AnsiConsole.Prompt(
                new SelectionPrompt<Trip>()
                    .Title("[bold]Select a trip to update the return date:[/]")
                    .HighlightStyle(new Style(Color.DeepSkyBlue1))
                    .UseConverter(t => $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd})")
                    .AddChoices(updatedTrips)
            );

            var rawDate = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the new return date [[YYYY-MM-DD]]:")
                    .Validate(input =>
                    {
                        if (DateTime.TryParse(input, out _))
                            return ValidationResult.Success();

                        Logg.Log($"Invalid return date input: '{input}'. Expected format YYYY-MM-DD.");
                        return ValidationResult.Error("[red]Date must be in format YYYY-MM-DD[/]");
                    })
            );

            var newReturnDate = DateTime.Parse(rawDate);

            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(
                        $"Do you want to update the return date for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] " +
                        $"from {selectedTrip.EndDate:yyyy-MM-dd} to [bold]{newReturnDate:yyyy-MM-dd}[/]?"
                    )
                    .AddChoices("✅ Yes", "❌ No")
            );

            if (confirm == "❌ No")
            {
                AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                Logg.Log(
                    $"User '{username}' cancelled return date update for trip to " +
                    $"'{selectedTrip.City}, {selectedTrip.Country}'."
                );
                UI.Pause();
                return;
            }

            selectedTrip.EndDate = newReturnDate;

            AnsiConsole.MarkupLine(
                $"[green]✅ Return date updated for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]"
            );
            UI.Pause();
        }

        // Metod för att uppdatera budget
        public void UpdateBudget(List<Trip> updatedTrips)
        {
            if (updatedTrips is null || updatedTrips.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                Logg.Log($"No trips available to update budget for user '{username}'.");
                UI.Pause();
                return;
            }

            var selectedTrip = AnsiConsole.Prompt(
                new SelectionPrompt<Trip>()
                    .Title("[bold]Select a trip to update its budget:[/]")
                    .HighlightStyle(new Style(Color.DeepSkyBlue1))
                    .UseConverter(t =>
                        $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd}) | Budget: {t.PlannedBudget}")
                    .AddChoices(updatedTrips)
            );

            var rawBudget = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the new budget:")
                    .Validate(input =>
                    {
                        if (!decimal.TryParse(input, out var parsed))
                        {
                            Logg.Log($"Invalid budget input: '{input}'. Expected a numeric value.");
                            return ValidationResult.Error("[red]You must enter a number.[/]");
                        }

                        if (parsed < 0)
                        {
                            Logg.Log($"Invalid budget input: '{input}'. Budget must be a positive number.");
                            return ValidationResult.Error("[red]Budget must be a positive number[/]");
                        }

                        return ValidationResult.Success();
                    })
            );

            var newBudget = decimal.Parse(rawBudget);

            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(
                        $"Do you want to update the budget for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] " +
                        $"from {selectedTrip.PlannedBudget} to [bold]{newBudget}[/]?"
                    )
                    .AddChoices("✅ Yes", "❌ No")
            );

            if (confirm == "❌ No")
            {
                AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                Logg.Log("Budget update cancelled by user.");
                UI.Pause();
                return;
            }

            selectedTrip.PlannedBudget = newBudget;

            AnsiConsole.MarkupLine(
                $"[green]✅ Budget updated for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]"
            );
            UI.Pause();
        }

        // Metod för att uppdatera kostnad
        public void UpdateCost(List<Trip> updatedTrips)
        {
            if (updatedTrips is null || updatedTrips.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                Logg.Log($"No trips available to update cost for user '{username}'.");
                UI.Pause();
                return;
            }

            var selectedTrip = AnsiConsole.Prompt(
                new SelectionPrompt<Trip>()
                    .Title("[bold]Select a trip to update its cost:[/]")
                    .HighlightStyle(new Style(Color.DeepSkyBlue1))
                    .UseConverter(t =>
                        $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd}) | Cost: {t.Cost}")
                    .AddChoices(updatedTrips)
            );

            var rawCost = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the new cost:")
                    .Validate(input =>
                    {
                        if (!decimal.TryParse(input, out var parsed))
                        {
                            Logg.Log($"Invalid cost input: '{input}'. Expected a numeric value.");
                            return ValidationResult.Error("[red]You must enter a number.[/]");
                        }

                        if (parsed < 0)
                        {
                            Logg.Log($"Invalid cost input: '{input}'. Cost must be a positive number.");
                            return ValidationResult.Error("[red]Cost must be a positive number[/]");
                        }

                        return ValidationResult.Success();
                    })
            );

            var newCost = decimal.Parse(rawCost);

            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(
                        $"Do you want to update the total cost for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] " +
                        $"from {selectedTrip.Cost} to [bold]{newCost}[/]?"
                    )
                    .AddChoices("✅ Yes", "❌ No")
            );

            if (confirm == "❌ No")
            {
                AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                Logg.Log($"User '{username}' cancelled cost update for trip to '{selectedTrip.City}, {selectedTrip.Country}'.");
                UI.Pause();
                return;
            }

            selectedTrip.Cost = newCost;

            AnsiConsole.MarkupLine(
                $"[green]✅ Cost updated for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]"
            );
            UI.Pause();
        }

        // Metod för att uppdatera antal resenärer
        public void UpdateNumberOfPassengers(List<Trip> updatedTrips)
        {
            if (updatedTrips is null || updatedTrips.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                Logg.Log($"No trips available to update number of passengers for user '{username}'.");
                UI.Pause();
                return;
            }

            var selectedTrip = AnsiConsole.Prompt(
                new SelectionPrompt<Trip>()
                    .Title("[bold]Select a trip to update its number of passengers:[/]")
                    .HighlightStyle(new Style(Color.DeepSkyBlue1))
                    .UseConverter(t => $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd}) | Passengers: {t.NumberOfPassengers}")
                    .AddChoices(updatedTrips)
            );

            var newNumberOfPassengers = AnsiConsole.Prompt(
                new TextPrompt<int>("Enter the new number of passengers:")
                    .Validate(s => s >= 1
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Number of passengers must be at least 1[/]"))
            );

            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(
                        $"Do you want to update the number of passangers for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] from {selectedTrip.NumberOfPassengers} to [bold]{newNumberOfPassengers}[/]?"
                    )
                    .AddChoices("✅ Yes", "❌ No")
            );

            if (confirm == "❌ No")
            {
                AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                Logg.Log($"User '{username}' cancelled number of passengers update for trip to '{selectedTrip.City}, {selectedTrip.Country}'.");
                UI.Pause();
                return;
            }

            selectedTrip.NumberOfPassengers = newNumberOfPassengers;
            AnsiConsole.MarkupLine($"[green]✅ Number of passengers updated for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]");
            UI.Pause();
        }

        // Metod för att radera resa
        public void DeleteTrip(List<Trip> updatedTrips)
        {
            if (updatedTrips is null || updatedTrips.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No trips to delete.[/]");
                Logg.Log($"No trips available to delete for user '{username}'.");
                UI.Pause();
                return;
            }

            var selectedTrip = AnsiConsole.Prompt(
                new SelectionPrompt<Trip>()
                    .Title("[bold red]Select a trip to [underline]delete[/]:[/]")
                    .HighlightStyle(new Style(Color.Red))
                    .UseConverter(t => $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd})")
                    .AddChoices(updatedTrips)
            );

            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Are you sure you want to delete the trip for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]?")
                    .AddChoices("✅ Yes", "❌ No")
            );

            if (confirm == "❌ No")
            {
                AnsiConsole.MarkupLine("[grey]Delete cancelled.[/]");
                Logg.Log($"User '{username}' cancelled deletion of trip to '{selectedTrip.City}, {selectedTrip.Country}'.");
                UI.Pause();
                return;
            }

            updatedTrips.Remove(selectedTrip);
            store.Save(updatedTrips);

            AnsiConsole.MarkupLine(
                $"[green]✅ Trip [bold]{selectedTrip.City}, {selectedTrip.Country}[/] has been deleted successfully![/]"
            );
            UI.Pause();
        }


        // ============================================================
        // ===   HÄMTA BESÖKTA LÄNDER FÖR VÄRLDSKARTAN              ===
        // ============================================================

        /// <summary>
        /// Returnerar en lista med landsnamn (i rätt format för kartan)
        /// baserat på användarens completed trips.
        /// </summary>
        public List<string> GetVisitedCountryNamesForMap()
        {
            // Hämta completed trips (justera om din property heter något annat)
            var completed = trips
                .Where(t => t.IsCompleted)
                .Select(t => t.Country?.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var result = new List<string>();

            foreach (var country in completed)
            {
                var key = country!.ToLowerInvariant();

                // Om alias finns → använd rätt GeoJSON-namn
                if (CountryAlias.TryGetValue(key, out var mapped))
                    result.Add(mapped);
                else
                    result.Add(country); // annars använd originalnamnet
            }

            return result;
        }

        // ============================================================
        // ===     ALIAS FÖR LÄNDER (mappar till GeoJSON-namn)      ===
        // ============================================================

        private static readonly Dictionary<string, string> CountryAlias = new()
        {
            // USA
            ["usa"] = "United States of America",
            ["us"] = "United States of America",
            ["united states"] = "United States of America",

            // UK
            ["uk"] = "United Kingdom",
            ["england"] = "United Kingdom",
            ["scotland"] = "United Kingdom",
            ["great britain"] = "United Kingdom",

            // Korea
            ["south korea"] = "Korea, Republic of",
            ["north korea"] = "Korea, Democratic People's Republic of",

            // Övriga vanliga specialnamn
            ["russia"] = "Russian Federation",
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
