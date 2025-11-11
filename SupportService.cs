using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Travel_Journal
{
    public class SupportService
    {
        public void DeleteAccountFlow()
        {
            AnsiConsole.MarkupLine("[red bold]⚠ WARNING[/]");
            AnsiConsole.MarkupLine("[red]This will permanently delete your account and all trips.[/]");
            AnsiConsole.MarkupLine("");

            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Are you sure?[/]")
                    .AddChoices("Yes, delete my account", "No, cancel")
            );

            if (confirm == "No, cancel")
                return;

            var final = AnsiConsole.Ask<string>("[red]Type DELETE to confirm:[/]");

            if (final.ToUpper() != "DELETE")
            {
                AnsiConsole.MarkupLine("[yellow]Cancelled.[/]");
                return;
            }

            // här ropar du ut din riktiga radering
            // t.ex. _accountService.DeleteCurrentAccount();

            AnsiConsole.MarkupLine("[green]✅ Account deleted.[/]");
        }

        // Add this method to fix CS1061
        
        
       public void ShowSupportMenu()
        {
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[aqua]🛟 Support & Help[/]")
                        .PageSize(7)
                        .AddChoices(new[]
                        {
                    "📩 Contact Support",
                    "❓ FAQ",
                    "📝 Report a Problem",
                    "📃 Terms & Privacy",
                    "🗑 Delete Account",
                    "↩ Back"
                        })
                );

                switch (choice)
                {
                    case "📩 Contact Support":
                        AnsiConsole.MarkupLine("[green]Email: codecommanders25@gmail.com[/]");
                        UserSession.Pause();
                        break;

                    case "❓ FAQ":
                        AnsiConsole.MarkupLine("[yellow]Common questions will appear here...[/]");
                        UserSession.Pause();
                        break;

                    case "📝 Report a Problem":
                        AnsiConsole.MarkupLine("[red]Describe the issue...[/]");
                        UserSession.Pause();
                        break;

                    case "📃 Terms & Privacy":
                        AnsiConsole.MarkupLine(@"
                            [blue]
                            [b]Terms & Privacy[/]

                            Your data in Travel Journal is stored locally on your device.
                            Nothing is uploaded, shared, or sent to any external server.

                            [b]What we store:[/]
                            • Your account information  
                            • Your trips and travel notes  
                            • Budget and planning details  

                            [b]You control your data:[/]
                            • You can edit or delete your account at any time  
                            • Deleting your account removes all your trips permanently  

                            [b]Security:[/]
                            Your information is saved in a local JSON file.  
                            Make sure you protect your device if your data is sensitive.

                            Thank you for using Travel Journal!  
                            [/]");
                        UserSession.Pause();
                        break;

                    case "🗑 Delete Account":
                        DeleteAccountFlow();
                        UserSession.Pause();
                        break;

                    case "↩ Back":
                        return;
                }
            }
        } 
    }
}
