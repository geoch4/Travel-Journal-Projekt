using System;
using System.Collections.Generic;
using Spectre.Console;

namespace Travel_Journal
{
    public class TripService // Class containing methods for handling trips
    {
        public List<Trip> trips = new List<Trip>();

        // === Add a new trip ===
        public void AddUpcomingTrip()
        {
            UI.Transition("Add New Trip ✈️");

            // Ask user for trip information
            string country = AnsiConsole.Ask<string>("Which [bold]country[/] are you visiting?");
            string city = AnsiConsole.Ask<string>("Which [bold]city[/]?");
            string currency = AnsiConsole.Ask<string>("What [bold]currency[/] does the country use? (e.g. SEK, USD, EUR)");

            // Cost and budget
            //decimal cost = AnsiConsole.Ask<decimal>("What was the total [bold]cost[/] of the trip?");
            decimal budget = AnsiConsole.Ask<decimal>("What is your planned [bold]budget[/]?");

            // Dates
            DateTime startDate;
            DateTime endDate;
            while (true)
            {
                startDate = AnsiConsole.Ask<DateTime>("Day of departure (format: YYYY-MM-DD):");
                endDate = AnsiConsole.Ask<DateTime>("Return date (format: YYYY-MM-DD):");

                if (startDate > endDate)
                {
                    // Show warning and repeat
                    AnsiConsole.MarkupLine("[red]❌ Invalid date![/]");
                    AnsiConsole.MarkupLine("[yellow]The start date must be before the end date. Try again.[/]\n");
                }
                else
                {
                    // Valid input — break out of the loop
                    break;
                }
            }

            // Number of travelers
            int passengers = AnsiConsole.Ask<int>("How many [bold]people[/] are joining?");

            //// Rating and review
            //int score = AnsiConsole.Ask<int>("How would you rate this trip (1–5)?");
            //string review = AnsiConsole.Ask<string>("Write a short [bold]review[/] or memory from the trip:");

            // Create a new Trip object
            var newTrip = new Trip
            {
                Country = country,
                City = city,
                Currency = currency,
                //Cost = cost,
                PlannedBudget = budget,
                StartDate = startDate,
                EndDate = endDate,
                NumberOfPassengers = passengers,
                //Score = score,
                //Review = review,
                TimeZone = "Europe/Stockholm" // Placeholder or can be made dynamic later
            };

            // Add to list
            trips.Add(newTrip);

            // Confirmation panel
            var panel = new Panel(
                $"[green]✅ Trip to [bold]{city}, {country}[/] has been added successfully![/]\n" +
                $"[grey]Budget:[/] {budget} {currency}\n" +
                //$"[grey]Rating:[/] {score}/10\n" +
                $"[grey]Dates:[/] {startDate:yyyy-MM-dd} → {endDate:yyyy-MM-dd}")
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader("Trip Saved", Justify.Center),
                BorderStyle = new Style(Color.Green)
            };

            AnsiConsole.Write(panel);
        }

        public void AddPreviousTrip()
        {
            UI.Transition("Add Previous Trip ✈️");

            // Ask user for trip information
            string country = AnsiConsole.Ask<string>("Which [bold]country[/] did you visit?");
            string city = AnsiConsole.Ask<string>("Which [bold]city[/]?");
            string currency = AnsiConsole.Ask<string>("What [bold]currency[/] does the country use? (e.g. SEK, USD, EUR)");

            // Cost and budget
            decimal budget = AnsiConsole.Ask<decimal>("What was your planned [bold]budget[/]?");
            decimal cost = AnsiConsole.Ask<decimal>("What was the total [bold]cost[/] of the trip?");

            // Dates
            DateTime startDate;
            DateTime endDate;
            while (true)
            {
                startDate = AnsiConsole.Ask<DateTime>("Day of departure (format: YYYY-MM-DD):");
                endDate = AnsiConsole.Ask<DateTime>("Return date (format: YYYY-MM-DD):");

                if (startDate > endDate)
                {
                    // Show warning and repeat
                    AnsiConsole.MarkupLine("[red]❌ Invalid date![/]");
                    AnsiConsole.MarkupLine("[yellow]The start date must be before the end date. Try again.[/]\n");
                }
                else
                {
                    // Valid input — break out of the loop
                    break;
                }
            }

            // Number of travelers
            int passengers = AnsiConsole.Ask<int>("How many [bold]people[/] were on the trip?");

            //// Rating and review
            int score = AnsiConsole.Ask<int>("How would you rate this trip (1–5)?");
            string review = AnsiConsole.Ask<string>("Write a short [bold]review[/] or memory from the trip:");

            // Create a new Trip object
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
                //TimeZone = "Europe/Stockholm" //  Om vi får tid över
            };

            // Add to list
            trips.Add(newTrip);

            // Confirmation panel
            var panel = new Panel(
                $"[green]✅ Trip to [bold]{city}, {country}[/] has been added successfully![/]\n" +
                $"[grey]Budget:[/] {budget} {currency}\n" +
                $"[grey]Rating:[/] {score}/5\n" +
                $"[grey]Dates:[/] {startDate:yyyy-MM-dd} → {endDate:yyyy-MM-dd}")
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader("Trip Saved", Justify.Center),
                BorderStyle = new Style(Color.Green)
            };

            AnsiConsole.Write(panel);
        }

        public void ShowAllTrips()
        {
            UI.Transition("All Saved Trips 🌍");

            // If there are no trips, show warning and return
            if (trips.Count == 0)
            {
                UI.Warn("No trips found! Add one first.");
                return;
            }

            // Create a Spectre.Console table
            var table = new Table()
                .Centered()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey50);

            // Add columns
            table.AddColumn("[bold cyan]Country[/]");
            table.AddColumn("[bold cyan]City[/]");
            table.AddColumn("[bold cyan]Dates[/]");
            table.AddColumn("[bold cyan]Budget[/]");
            table.AddColumn("[bold cyan]Cost[/]");
            table.AddColumn("[bold cyan]Rating[/]");
            table.AddColumn("[bold cyan]Review[/]");

            // Add each trip as a row
            foreach (var trip in trips)
            {
                // Format dates and values
                string dateRange = $"{trip.StartDate:yyyy-MM-dd} → {trip.EndDate:yyyy-MM-dd}";
                string budget = $"{trip.PlannedBudget} {trip.Currency}";
                string cost = trip.Cost > 0 ? $"{trip.Cost} {trip.Currency}" : "[grey]—[/]";
                string rating = trip.Score > 0 ? $"{trip.Score}/5" : "[grey]—[/]";
                string review = string.IsNullOrWhiteSpace(trip.Review) ? "[grey]No review[/]" : trip.Review;

                // Choose color depending on if trip is upcoming or past
                var color = trip.StartDate > DateTime.Now ? Color.Green : Color.Grey;

                table.AddRow(
                    new Markup($"[{color}]{trip.Country}[/]"),
                    new Markup($"[{color}]{trip.City}[/]"),
                    new Markup($"[{color}]{dateRange}[/]"),
                    new Markup($"[{color}]{budget}[/]"),
                    new Markup($"[{color}]{cost}[/]"),
                    new Markup($"[{color}]{rating}[/]"),
                    new Markup($"[{color}]{review}[/]")
                );
            }

            // Write table to console
            AnsiConsole.Write(table);

            // Small footer
            AnsiConsole.MarkupLine($"\n[grey]Total trips:[/] {trips.Count}");
        }

    }
}


//AddTrip(Trip trip)
//UpdateTrip(Guid id, Trip updatedTrip)
//DeleteTrip(Guid id)
//GetTripsByScore(TripStatus status)
//FilterTrips(string city, DateTime? from, DateTime? to)