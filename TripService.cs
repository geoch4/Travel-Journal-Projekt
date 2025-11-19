using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

namespace Travel_Journal
{
    public class TripService
    {
        // === Fält ===
        // Lista med alla resor för användaren
        private List<Trip> trips = new();

        // Användarens namn (för filidentifiering)
        private readonly string username;

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

                    var p = UI.AskStepDecimal("How many passengers?");
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
            UserSession.Pause();
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

                    var p = UI.AskStepDecimal("How many passengers?");
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
            UserSession.Pause();
        }


        public void ShowManageTripsMenu() 
        {
            while(true)
    {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[aqua]Add Trips[/]")
                        .PageSize(8)
                        .AddChoices(new[]
                        {
                    "➕ Add Upcoming Trip",
                    "🕰 Add Previous Trip",
              
                    "↩ Back"
                        })
                );

                switch (choice)
                {
                    case "➕ Add Upcoming Trip":
                        AddUpcomingTrip();
                        break;

                    case "🕰 Add Previous Trip":
                        AddPreviousTrip();
                        break;

                    case "↩ Back":
                        return; // tillbaka till huvudmenyn
                }
            }
        }

        // === Visar alla resor i tabellform ===
        public void ShowAllTrips()
        {
            UI.Transition($"All Trips for {username} 🌍");

            if (!trips.Any())
            {
                UI.Warn("No trips found for this account.");
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
        public void UpdateTrips(List<Trip> updatedTrips) // Hjälpmetod för att uppdatera resor.
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]🛠️ Choose what you want to update or manage: [/]")
                    .HighlightStyle(new Style(Color.DeepSkyBlue1))
                    .AddChoices(
                        "⭐ Rating",
                        "🛫 Depart Date",
                        "🛬 Return Date",
                        "💰 Budget",
                        "💸 Cost",
                        "👥 Number of Passengers",
                        "🗑️ Delete Trip",
                        "↩️ Return"
                    )
            );

            switch (choice)
            {
                case "⭐ Rating":
                    UpdateRating();
                    SaveTrips();
                    break;

                case "🛫 Depart Date":
                    UpdateDepartDate();
                    SaveTrips();
                    break;

                case "🛬 Return Date":
                    UpdateReturnDate();
                    SaveTrips();
                    break;

                case "💰 Budget":
                    UpdateBudget();
                    SaveTrips();
                    break;

                case "💸 Cost":
                    UpdateCost();
                    SaveTrips();
                    break;

                case "👥 Number of Passengers":
                    UpdateNumberOfPassengers();
                    SaveTrips();
                    break;

                case "🗑️ Delete Trip":
                    DeleteTrip();
                    SaveTrips();
                    break;

                case "↩️ Return":
                    // Går tillbaka utan att göra ändringar
                    break;
            }


            void UpdateRating()
            {
                if (updatedTrips is null || updatedTrips.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                    UserSession.Pause();
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
                        .Validate(s => s is >= 1 and <= 5
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Rating must be between 1 and 5[/]"))
                );

                var confirm = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Do you want to update the rating for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] to [bold]{newScore}[/]?")
                        .AddChoices("✅ Yes", "❌ No")
                );

                if (confirm == "❌ No")
                {
                    AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                    UserSession.Pause();
                    return;
                }


                selectedTrip.Score = newScore;
                AnsiConsole.MarkupLine($"[green]✅ Rating updated to {newScore}/5 for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]");
                UserSession.Pause();
            }

            void UpdateDepartDate()
            {
                if (updatedTrips is null || updatedTrips.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                    UserSession.Pause();
                    return;
                }

                var selectedTrip = AnsiConsole.Prompt(
                    new SelectionPrompt<Trip>()
                        .Title("[bold]Select a trip to update the date of depart:[/]")
                        .HighlightStyle(new Style(Color.DeepSkyBlue1))
                        .UseConverter(t => $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd})")
                        .AddChoices(updatedTrips)
                );

                var newDateOfDepart = AnsiConsole.Prompt(
                    new TextPrompt<DateTime>("Enter the new date [[YYYY-MM-DD]]:")
                        .Validate(s => s != null
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Date must be in format YYYY-MM-DD[/]"))
                );

                var confirm = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Do you want to update the departure date for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] from {selectedTrip.StartDate:yyyy-MM-dd} to [bold]{newDateOfDepart:yyyy-MM-dd}[/]?")
                        .AddChoices("✅ Yes", "❌ No")
                );

                if (confirm == "❌ No")
                {
                    AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                    UserSession.Pause();
                    return;
                }

                selectedTrip.StartDate = newDateOfDepart;
                AnsiConsole.MarkupLine($"[green]✅ Departure date updated for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]");
                UserSession.Pause();
            }

            void UpdateReturnDate()
            {
                if (updatedTrips is null || updatedTrips.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                    UserSession.Pause();
                    return;
                }

                var selectedTrip = AnsiConsole.Prompt(
                    new SelectionPrompt<Trip>()
                        .Title("[bold]Select a trip to update the return date:[/]")
                        .HighlightStyle(new Style(Color.DeepSkyBlue1))
                        .UseConverter(t => $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd})")
                        .AddChoices(updatedTrips)
                );

                var newReturnDate = AnsiConsole.Prompt(
                    new TextPrompt<DateTime>("Enter the new return date [[YYYY-MM-DD]]:")
                        .Validate(s => s != null
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Date must be in format YYYY-MM-DD[/]"))
                );

                var confirm = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Do you want to update the return date for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] from {selectedTrip.EndDate:yyyy-MM-dd} to [bold]{newReturnDate:yyyy-MM-dd}[/]?")
                        .AddChoices("✅ Yes", "❌ No")
                );

                if (confirm == "❌ No")
                {
                    AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                    UserSession.Pause();
                    return;
                }

                selectedTrip.EndDate = newReturnDate;
                AnsiConsole.MarkupLine($"[green]✅ Return date updated for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]");
                UserSession.Pause();
            }

            void UpdateBudget()
            {
                if (updatedTrips is null || updatedTrips.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                    UserSession.Pause();
                    return;
                }

                var selectedTrip = AnsiConsole.Prompt(
                    new SelectionPrompt<Trip>()
                        .Title("[bold]Select a trip to update its budget:[/]")
                        .HighlightStyle(new Style(Color.DeepSkyBlue1))
                        .UseConverter(t => $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd}) | Budget: {t.PlannedBudget}")
                        .AddChoices(updatedTrips)
                );

                var newBudget = AnsiConsole.Prompt(
                    new TextPrompt<decimal>("Enter the new budget:")
                        .Validate(s => s >= 0
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Budget must be a positive number[/]"))
                );

                var confirm = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Do you want to update the budget for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] from {selectedTrip.PlannedBudget} to [bold]{newBudget}[/]?")
                        .AddChoices("✅ Yes", "❌ No")
                );

                if (confirm == "❌ No")
                {
                    AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                    UserSession.Pause();
                    return;
                }

                selectedTrip.PlannedBudget = newBudget;
                AnsiConsole.MarkupLine($"[green]✅ Budget updated for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]");
                UserSession.Pause();
            }

            void UpdateCost()
            {
                if (updatedTrips is null || updatedTrips.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                    UserSession.Pause();
                    return;
                }

                var selectedTrip = AnsiConsole.Prompt(
                    new SelectionPrompt<Trip>()
                        .Title("[bold]Select a trip to update its cost:[/]")
                        .HighlightStyle(new Style(Color.DeepSkyBlue1))
                        .UseConverter(t => $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd}) | Cost: {t.Cost}")
                        .AddChoices(updatedTrips)
                );

                var newCost = AnsiConsole.Prompt(
                    new TextPrompt<decimal>("Enter the new cost:")
                        .Validate(s => s >= 0
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Cost must be a positive number[/]"))
                );

                var confirm = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Do you want to update the total cost for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] from {selectedTrip.Cost} to [bold]{newCost}[/]?")
                        .AddChoices("✅ Yes", "❌ No")
                );

                if (confirm == "❌ No")
                {
                    AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                    UserSession.Pause();
                    return;
                }

                selectedTrip.Cost = newCost;
                AnsiConsole.MarkupLine($"[green]✅ Cost updated for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]");
                UserSession.Pause();
            }

            void UpdateNumberOfPassengers()
            {
                if (updatedTrips is null || updatedTrips.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No trips to update.[/]");
                    UserSession.Pause();
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
                        .Title($"Do you want to update the number of passangers for [bold]{selectedTrip.City}, {selectedTrip.Country}[/] from {selectedTrip.NumberOfPassengers} to [bold]{newNumberOfPassengers}[/]?")
                        .AddChoices("✅ Yes", "❌ No")
                );

                if (confirm == "❌ No")
                {
                    AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                    UserSession.Pause();
                    return;
                }

                selectedTrip.NumberOfPassengers = newNumberOfPassengers;
                AnsiConsole.MarkupLine($"[green]✅ Number of passengers updated for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]![/]");
                UserSession.Pause();
            }

            void DeleteTrip()
            {
                // Kontrollera att det finns resor att ta bort
                if (updatedTrips is null || updatedTrips.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No trips to delete.[/]");
                    UserSession.Pause();
                    return;
                }

                // Visa en meny där användaren väljer vilken resa som ska tas bort
                var selectedTrip = AnsiConsole.Prompt(
                    new SelectionPrompt<Trip>()
                        .Title("[bold red]Select a trip to [underline]delete[/]:[/]")
                        .HighlightStyle(new Style(Color.Red))
                        .UseConverter(t => $"{t.City}, {t.Country} ({t.StartDate:yyyy-MM-dd} - {t.EndDate:yyyy-MM-dd})")
                        .AddChoices(updatedTrips)
                );

                // Bekräfta att användaren verkligen vill ta bort resan
                var confirm = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Are you sure you want to delete the trip for [bold]{selectedTrip.City}, {selectedTrip.Country}[/]?")
                        .AddChoices("✅ Yes", "❌ No")
                );

                if (confirm == "❌ No")
                {
                    AnsiConsole.MarkupLine("[grey]Delete cancelled.[/]");
                    UserSession.Pause();
                    return;
                }

                // Ta bort resan från listan
                updatedTrips.Remove(selectedTrip);

                // Spara uppdaterad lista till fil (via DataStore)
                store.Save(updatedTrips);

                // Bekräftelse till användaren
                AnsiConsole.MarkupLine(
                    $"[green]✅ Trip [bold]{selectedTrip.City}, {selectedTrip.Country}[/] has been deleted successfully![/]");
                UserSession.Pause();
            }
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
