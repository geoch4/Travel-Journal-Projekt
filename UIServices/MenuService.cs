using Spectre.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Travel_Journal.Data;
using Travel_Journal.Models;
using Travel_Journal.Services;

namespace Travel_Journal.UIServices
{
    public static class MenuService // Statisk klass för menyer
    {
        // Visar en interaktiv meny där användaren väljer vad den vill göra.
        // Huvudmenyn
        public static string MainMenu()
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]🔎 Select an Option[/]")
                    .HighlightStyle(new Style(Color.DeepSkyBlue1))
                    .AddChoices("Register", "Login", "Forgot password", "Exit")
            );
        }

        // Inloggad meny
        public static string LoggedInMenu(string username)
        {
            AnsiConsole.Clear();
            UI.Splash();
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold cyan]Welcome, {username}![/]")
                    .HighlightStyle(new Style(Color.Cyan1))
                    .AddChoices(
                        "👤 View Profile",
                        "📘 Add Trips",
                        "📋 Show All Trips",
                        "💰 Budget & Savings",
                        "📊 Statistics",
                        "🔄 Edit Trips",
                        "🤖✈️ AI Travel Assistant",
                        "🌍 World Map (Visited Countries)",
                        "🔧 Support & Help",
                        "🚪 Log out"
                    )
            );
        }

        // Statistik meny
        public static void StatsMenu(StatisticsUI stats)
        {
            while (true) // 🔄 loopar tills användaren väljer "Back"
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold cyan]Choose an option:[/]")
                        .HighlightStyle(new Style(Color.DeepSkyBlue1))
                        .AddChoices(
                            "📈 Sort by rating (highest to lowest)",
                            "📉 Sort by rating (lowest to highest)",
                            "💰 Sort by price (highest to lowest)",
                            "🔙 Back to Main Menu"
                        )
                );

                switch (choice)
                {
                    case "📈 Sort by rating (highest to lowest)":
                        stats.SortTripsByRatingDescending();
                        UI.Pause();
                        break;

                    case "📉 Sort by rating (lowest to highest)":
                        stats.SortTripsByRatingAscending();
                        UI.Pause();
                        break;

                    case "💰 Sort by price (highest to lowest)":
                        stats.SortTripsByPriceDescending();
                        UI.Pause();
                        break;

                    case "🔙 Back to Main Menu":
                        return; // ⛔ avsluta loopen → tillbaka till UserSession
                }
            }
        }

        // === 💬 Support & Hjälpmeny ===
        // Returnerar true om användaren valt att radera sitt konto (så att sessionen kan avslutas).
        public static bool ShowSupportMenu(SupportService support, Account account)
        {
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[aqua]🛟 Support & Help[/]")
                        .PageSize(7)
                        .HighlightStyle(new Style(Color.Cyan1))
                        .AddChoices(
                            "📩 Contact Support",
                            "❓ FAQ - Frequently Asked Questions",
                            "📃 Terms & Privacy",
                            "🗑  Delete Account",
                            "↩ Back"
                        )
                );

                switch (choice)
                {
                    // === Kontaktalternativ ===
                    case "📩 Contact Support":
                        AnsiConsole.Clear();
                        support.EmailInfo();
                        UI.Pause();
                        break;

                    // === FAQ ===
                    case "❓ FAQ - Frequently Asked Questions":
                        AnsiConsole.Clear();
                        support.FAQInfo();
                        UI.Pause();
                        break;

                    // === Villkor & sekretess ===
                    case "📃 Terms & Privacy":
                        AnsiConsole.Clear();
                        support.TermsAndPrivacy();
                        UI.Pause();
                        break;

                    // === Konto-radering ===
                    case "🗑  Delete Account":
                        AnsiConsole.Clear();
                        bool deleted = support.DeleteAccountFlow(account);

                        if (deleted)
                            return true; // 🔹 Avsluta sessionen (användaren är raderad)

                        UI.Pause();
                        break;

                    // === Tillbaka ===
                    case "↩ Back":
                        return false; // 🔹 Tillbaka till UserSession utan att avsluta
                }
            }
        }

        // Meny för att lägga till resor
        public static void ShowTripMenu(TripUI tripUI)
        {
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[aqua]Add Trips[/]")
                        .PageSize(8)
                        .AddChoices(
                            "➕ Add Upcoming Trip",
                            "🕰 Add Previous Trip",
                            "↩ Back"
                        )
                );

                switch (choice)
                {
                    case "➕ Add Upcoming Trip":
                        tripUI.AddUpcomingTrip();
                        break;

                    case "🕰 Add Previous Trip":
                        tripUI.AddPreviousTrip();
                        break;

                    case "↩ Back":
                        return; // Tillbaka till UserSession-menyn
                }
            }
        }

        // Meny för att uppdatera resor
        public static void ShowTripEditMenu(UpdateTripUI updateTripUI)
        {
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold cyan]🛠️ Choose what you want to update or manage:[/]")
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
                        updateTripUI.UpdateRating();
                        break;

                    case "🛫 Depart Date":
                        updateTripUI.UpdateDepartDate();
                        break;

                    case "🛬 Return Date":
                        updateTripUI.UpdateReturnDate();
                        break;

                    case "💰 Budget":
                        updateTripUI.UpdateBudget();
                        break;

                    case "💸 Cost":
                        updateTripUI.UpdateCost();
                        break;

                    case "👥 Number of Passengers":
                        updateTripUI.UpdateNumberOfPassengers();
                        break;

                    case "🗑️ Delete Trip":
                        updateTripUI.DeleteTrip();
                        break;

                    case "↩️ Return":
                        return;
                }
            }
        }

        // Metod för budgetmenyn
        public static void BudgetMenu(BudgetService budget)
        {
            while (true)
            {
                // Visuell övergång för menyn
                UI.Transition("💰 Travel Savings Account");

                // Visa kontosaldo längst upp
                AnsiConsole.MarkupLine($"[bold green]Current balance:[/] {budget.Account.Savings} SEK\n");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold cyan]What would you like to do?[/]")
                        .HighlightStyle(new Style(Color.Chartreuse1))
                        .AddChoices(
                            "➕ Deposit money",
                            "➖ Withdraw money",
                            "📊 View trip budgets",
                            "✨ Dream Vacation",
                            "↩ Back"
                        )
                );

                switch (choice)
                {
                    case "➕ Deposit money":
                        budget.Deposit();
                        break;

                    case "➖ Withdraw money":
                        budget.Withdraw();
                        break;

                    case "📊 View trip budgets":
                        budget.ShowTripBudgets();
                        break;

                    case "✨ Dream Vacation":
                        budget.DreamVacation();
                        break;

                    case "↩ Back":
                        return;
                }
            }
        }
    }
}
