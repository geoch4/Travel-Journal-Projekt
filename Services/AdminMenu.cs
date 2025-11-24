using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Travel_Journal.UIServices;

namespace Travel_Journal.Services
{
    public static class AdminMenu
    {
        public static void ShowAdminMenu(AdminService adminService)
        {
            while (true)
            {
                UI.Transition("Admin Panel 🛠");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[red]Admin Panel[/]")
                        .PageSize(7)
                        .AddChoices(new[]
                        {
                            "👥 View all users",
                            "🗑  Delete user",
                            "↩ Back"
                        })
                );

                switch (choice)
                {
                    case "👥 View all users":
                        adminService.ShowAllUsers();
                        UI.Pause();
                        break;

                    case "🗑  Delete user":
                        adminService.DeleteUser();
                        break;

                    case "↩ Back":
                        return;
                }
            }
        }
    }
}

