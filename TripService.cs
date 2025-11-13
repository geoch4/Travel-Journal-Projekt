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

        // Användarnamnet som används för att identifiera rätt fil
        private readonly string username;

        // DataStore ansvarar för JSON-hantering (ersätter filePath + manuella metoder)
        private readonly DataStore<Trip> store;

        // === Konstruktor ===
        public TripService(string username)
        {
            // Spara användarnamnet så vi vet vem resorna tillhör
            this.username = username;

            // Skapa ett DataStore-objekt som automatiskt sparar i "data/{username}_trips.json"
            store = new DataStore<Trip>($"{username}_trips.json");

            // Läs in befintliga resor via DataStore (om filen finns)
            trips = store.Load();
        }

        // === Sparar alla resor till JSON-fil ===
        private void SaveTrips()
        {
            try
            {
                // Använd generiska DataStore istället för manuell serialisering
                store.Save(trips);
            }
            catch (Exception ex)
            {
                // Om något går fel, visa felmeddelande
                UI.Error($"Failed to save trips: {ex.Message}");
                Logg.Log($"ERROR saving trips for ´{username}´:{ex.Message}");// Logg som loggar fel vis sparning av resor
            }
        }

        // === Hjälpmetod: ber användaren om ett decimaltal (ex. budget) ===
        private decimal AskDecimal(string message)
        {
            while (true)
            {
                string input = AnsiConsole.Ask<string>(message);
                try
                {
                    return decimal.Parse(input);
                   
                }
                catch(Exception ex)
                {

                    // Om användaren skriver något ogiltigt (t.ex. bokstäver) visas fel
                    Logg.Log($"⚠️ {input} {ex}");
                    
                    UI.Error("⚠️ not a number");
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
            var country = UI.AskWithBack("Which [bold]country[/] are you visiting");
            if (country == null)
                return; // eller gå till föregående meny
            string city = AnsiConsole.Ask<string>("Which [bold]city[/]?");
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
                PlannedBudget = budget,
                StartDate = startDate,
                EndDate = endDate,
                NumberOfPassengers = passengers,
            };

            // Lägg till i listan och spara till fil
            trips.Add(newTrip);
            SaveTrips();

            // Bekräftelse till användaren
            var panel = new Panel(
                $"[green]✅ Trip to [bold]{city}, {country}[/] added successfully![/]\n" +
                $"[grey]Budget:[/] {budget}\n" +
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
            var country = UI.AskWithBack("Which [bold]country[/] did you visit");
            if (country == null)
                return; // eller gå till föregående meny
            string city = AnsiConsole.Ask<string>("Which [bold]city[/]?");
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
                Cost = cost,
                PlannedBudget = budget,
                StartDate = startDate,
                EndDate = endDate,
                NumberOfPassengers = passengers,
                Score = score,
                Review = review,
            };

            // Lägg till resan i listan och spara
            trips.Add(newTrip);
            SaveTrips();

            // Visa bekräftelse för användaren
            var panel = new Panel(
                $"[green]✅ Trip to [bold]{city}, {country}[/] added successfully![/]\n" +
                $"[grey]Budget:[/] {budget}\n" +
                $"[grey]Cost:[/] {cost}\n" +
                $"[grey]Rating:[/] {score}/5\n" +
                $"[grey]Dates:[/] {startDate:yyyy-MM-dd} → {endDate:yyyy-MM-dd}")
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader("Trip Saved", Justify.Center),
                BorderStyle = new Style(Color.Green)
            };
            AnsiConsole.Write(panel);
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
                    AnsiConsole.MarkupLine("[yellow]Inga resor att uppdatera.[/]");
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
                    AnsiConsole.MarkupLine("[yellow]Inga resor att uppdatera.[/]");
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
                    AnsiConsole.MarkupLine("[yellow]Inga resor att uppdatera.[/]");
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
                    AnsiConsole.MarkupLine("[yellow]Inga resor att uppdatera.[/]");
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
                    AnsiConsole.MarkupLine("[yellow]Inga resor att uppdatera.[/]");
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
                    AnsiConsole.MarkupLine("[yellow]Inga resor att uppdatera.[/]");
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
                    AnsiConsole.MarkupLine("[yellow]Inga resor att ta bort.[/]");
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
    }
}
