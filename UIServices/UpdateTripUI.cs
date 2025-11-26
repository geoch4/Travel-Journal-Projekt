using OpenAI.Chat;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Travel_Journal.Data;
using Travel_Journal.Models;
using Travel_Journal.Services;

namespace Travel_Journal.UIServices
{
    public class UpdateTripUI // Hanterar all uppdaterings-logik i UI
    {
        // Referens till vår Service. UI är beroende av Service för att fungera.
        private readonly TripService _service;

        // Dependency Injection: Vi får in servicen via konstruktorn.
        public UpdateTripUI(TripService service)
        {
            _service = service;
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
                trip => $"{trip.City}, {trip.Country} | Budget: {trip.PlannedBudget}",
                input =>
                {
                    // Här ligger logiken för validering OCH loggning av fel
                    if (!decimal.TryParse(input, out var choice))
                    {
                        Logg.Log($"Invalid budget input: '{input}'. Not a number.");
                        return (false, 0, "[red]Must be a number[/]");
                    }
                    if (choice < 0)
                    {
                        Logg.Log($"Invalid budget input: '{input}'. Negative.");
                        return (false, 0, "[red]Must be positive[/]");
                    }
                    return (true, choice, "");
                },
                // Allt gick bra! Skicka tillbaka det konverterade värdet (choice).
                (trip, choice) => trip.PlannedBudget = choice
            );
        }

        // Metod för att uppdatera kostnad på en resa
        public void UpdateCost()
        {
            UpdateTripProperty<decimal>("Cost",
                trip => $"{trip.City}, {trip.Country} | Cost: {trip.Cost}",
                input =>
                {
                    if (!decimal.TryParse(input, out var choice))
                    {
                        Logg.Log($"Invalid cost input: '{input}'. Not a number.");
                        return (false, 0, "[red]Must be a number[/]");
                    }
                    if (choice < 0)
                    {
                        Logg.Log($"Invalid cost input: '{input}'. Negative.");
                        return (false, 0, "[red]Must be positive[/]");
                    }
                    return (true, choice, "");
                },
                (trip, choice) => trip.Cost = choice
            );
        }

        // Metod för att uppdatera betyg på en resa
        public void UpdateRating()
        {
            UpdateTripProperty<int>("Rating",
                trip => $"{trip.City} | Score: {trip.Score}/5",
                input =>
                {
                    if (!int.TryParse(input, out var choice))
                    {
                        Logg.Log($"Invalid rating input: '{input}'. Not an integer.");
                        return (false, 0, "[red]Must be a whole number[/]");
                    }
                    if (choice < 1 || choice > 5)
                    {
                        Logg.Log($"Invalid rating input: '{choice}'. Out of range.");
                        return (false, 0, "[red]Rating must be 1-5[/]");
                    }
                    return (true, choice, "");
                },
                (trip, choice) => trip.Score = choice
            );
        }

        public void UpdateNumberOfPassengers()
        {
            UpdateTripProperty<int>("Passengers",
                trip => $"{trip.City} | Pax: {trip.NumberOfPassengers}",
                input =>
                {
                    if (!int.TryParse(input, out var choice))
                    {
                        Logg.Log($"Invalid pax input: '{input}'.");
                        return (false, 0, "[red]Must be a number[/]");
                    }
                    if (choice < 1)
                    {
                        Logg.Log($"Invalid pax input: '{choice}'. Too low.");
                        return (false, 0, "[red]Must be at least 1[/]");
                    }
                    return (true, choice, "");
                },
                (trip, choice) => trip.NumberOfPassengers = choice
            );
        }

        public void UpdateDepartDate()
        {
            UpdateTripProperty<DateTime>("Departure Date",
                trip => $"{trip.City} | Starts: {trip.StartDate:yyyy-MM-dd}",
                input =>
                {
                    if (!DateTime.TryParse(input, out var choice))
                    {
                        Logg.Log($"Invalid date input: '{input}'.");
                        return (false, default, "[red]Format: YYYY-MM-DD[/]");
                    }
                    return (true, choice, "");
                },
                (trip, choice) => trip.StartDate = choice
            );
        }

        public void UpdateReturnDate()
        {
            UpdateTripProperty<DateTime>("Return Date",
                trip => $"{trip.City} | Ends: {trip.EndDate:yyyy-MM-dd}",
                input =>
                {
                    if (!DateTime.TryParse(input, out var choice))
                    {
                        Logg.Log($"Invalid date input: '{input}'.");
                        return (false, default, "[red]Format: YYYY-MM-DD[/]");
                    }
                    return (true, choice, "");
                },
                (trip, choice) => trip.EndDate = choice
            );
        }

        // DeleteTrip är lite speciell (tar bort istället för uppdaterar), så den ligger separat
        public void DeleteTrip()
        {
            var trips = _service.GetAllTrips();
            if (!trips.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No trips to delete.[/]");
                UI.Pause();
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
