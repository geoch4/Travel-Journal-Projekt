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
    public class AdminService
    {
        public void ShowAllUsers()
        {
            var accounts = AccountStore.GetAll();

            if (!accounts.Any())
            {
                UI.Warn("No accounts found.");
                UI.Pause();
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey50)
                .Centered();

            table.AddColumn("[cyan]Username[/]");
            table.AddColumn("[cyan]Admin[/]");

            foreach (var acc in accounts)
            {
                table.AddRow(
                    acc.UserName ?? "-",
                    acc.IsAdmin ? "[green]Yes[/]" : "[grey]No[/]"
                );
            }

            AnsiConsole.Write(table);
            UI.Pause();
        }
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

            foreach (var acc in accounts)
            {
                table.AddRow(
                    acc.UserName ?? "-",
                    acc.IsAdmin ? "[green]Yes[/]" : "[red]No[/]"
                );
            }

            AnsiConsole.Write(table);
            UI.Pause(); // så du hinner se listan innan du skriver username

            // Nu frågar vi vem som ska raderas
            var username = AnsiConsole.Ask<string>("[red]Enter username of user to delete:[/]");

            var account = accounts.FirstOrDefault(a => a.UserName == username);

            if (account == null)
            {
                UI.Error("User not found.");
                UI.Pause();
                return;
            }

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
