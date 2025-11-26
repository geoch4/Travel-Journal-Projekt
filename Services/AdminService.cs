using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Travel_Journal.Data;
using Travel_Journal.Models;
using Travel_Journal.UIServices;

namespace Travel_Journal.Services
{
    // Tjänst för admin-specifika funktioner
    public class AdminService
    {
        // Visa alla användare i en tabell
        public void ShowAllUsers()
        {
            // Hämta alla användare från AccountStore
            var accounts = AccountStore.GetAll();

            if (!accounts.Any())
            {
                UI.Warn("No accounts found.");
                UI.Pause();
                return;
            }
            // Skapa en tabell med Spectre.Console
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey50)
                .Centered();

            table.AddColumn("[cyan]Username[/]");
            table.AddColumn("[cyan]Admin[/]");

            // Lägg till varje användare som en rad i tabellen
            foreach (var acc in accounts)
            {
                table.AddRow(
                    acc.UserName ?? "-",
                    acc.IsAdmin ? "[green]Yes[/]" : "[grey]No[/]" // den funkar som en if sats
                );
            }

            AnsiConsole.Write(table);
            UI.Pause();
        }
        // Radera en användare bara om det är en admin
        public void DeleteUser()
        {
            // Visa först alla användare i en tabell
            var accounts = AccountStore.GetAll();

            if (!accounts.Any())
            {
                UI.Warn("No accounts to delete.");
                UI.Pause();
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey50)
                .Centered();

            table.AddColumn("[cyan]Username[/]");
            table.AddColumn("[cyan]Admin[/]");

            // Lägg till varje användare som en rad i tabellen
            foreach (var acc in accounts)
            {
                table.AddRow(
                    acc.UserName ?? "-",
                    acc.IsAdmin ? "[green]Yes[/]" : "[red]No[/]" // den funkar som en if sats
                );
            }

            AnsiConsole.Write(table);
            UI.Pause(); // så du hinner se listan innan du skriver username

            // Nu frågar vi vem som ska raderas
            var username = AnsiConsole.Ask<string>("[red]Enter username of user to delete:[/]");

            // Hitta kontot baserat på användarnamnet
            var account = accounts.FirstOrDefault(a => a.UserName == username);

            if (account == null)
            {
                UI.Error("User not found.");
                UI.Pause();
                return;
            }

            // Försäkra att admin-konton inte kan raderas härifrån
            if (account.IsAdmin)
            {
                UI.Error("You cannot delete an admin account from here (for safety).");
                UI.Pause();
                return;
            }

            var confirm = AnsiConsole.Confirm($"Are you sure you want to delete [yellow]{username}[/]?");

            if (!confirm) return;

            accounts.Remove(account);

            // Spara ändringarna till users.json
            AccountStore.Save();

            UI.Success("User deleted successfully.");
            UI.Pause();
        }
    }
}
