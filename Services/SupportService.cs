using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Travel_Journal.Data;
using Travel_Journal.Models;
using Travel_Journal.UIServices;

namespace Travel_Journal.Services
{
    /// <summary>
    /// SupportService hanterar allt som rör "Support & Hjälp"-menyn.
    /// Inkluderar visning av kontaktinfo, villkor, FAQ samt radering av konto.
    /// </summary>
    public class SupportService
    {
        // === 🗑️ Radera konto och tillhörande data ===
        // Returnerar true om kontot har raderats (så att UserSession kan avslutas).
        public bool DeleteAccountFlow(Account account)
        {
            UI.Transition("⚠ Account Deletion");

            AnsiConsole.MarkupLine("[red bold]This will permanently delete your account and all saved trips.[/]");
            AnsiConsole.WriteLine();

            // --- Första bekräftelsen ---
            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Are you sure you want to continue?[/]")
                    .HighlightStyle(new Style(Color.Red))
                    .AddChoices("Yes, delete my account", "No, cancel")
            );

            if (confirm == "No, cancel")
            {
                UI.Info("Deletion cancelled.");
                return false;
            }

            // --- Extra säkerhetskontroll ---
            var final = AnsiConsole.Ask<string>("[red]Type delete to confirm:[/]");
            if (!string.Equals(final, "delete", StringComparison.OrdinalIgnoreCase))
            {
                UI.Warn("Cancelled — confirmation text didn’t match.");
                Logg.Log($"Account deletion cancelled for user '{account.UserName}' — confirmation text mismatch.");
                return false;
            }

            try
            {
                string username = account.UserName;

                // === Spinner under själva raderingen ===
                UI.WithStatus("Deleting account and data...", () =>
                {
                    // --- Ta bort användarens resefil ---
                    string tripFile = Path.Combine(Paths.DataDir, $"{username}_trips.json");
                    if (File.Exists(tripFile))
                    {
                        File.Delete(tripFile);
                        UI.Info($"Deleted [yellow]{Path.GetFileName(tripFile)}[/]");
                    }


                    // === 🧩 Laddar in, filtrerar och sparar kontodata ===
                    // Skapar ett DataStore-objekt som hanterar alla användarkonton.
                    // Paths.UsersFile pekar på den centrala användarfilen (t.ex. "data/users.json")
                    var store = new DataStore<Account>(Paths.UsersFile);

                    // Läser in hela listan med alla konton från filen (alla registrerade användare)
                    var accounts = store.Load();

                    // Filtrerar bort det konto som ska tas bort (behåll alla utom det med matchande användarnamn)
                    var updated = accounts
                        .Where(a => a.UserName != username) // LINQ: behåll alla konton där UserName ≠ det vi raderar
                        .ToList(); // Gör om resultatet till en ny lista

                    // Sparar den uppdaterade listan tillbaka till JSON-filen.
                    // Resultatet: filen skrivs över utan det raderade kontot.
                    store.Save(updated);

                    // Kort paus för snygg effekt
                    Thread.Sleep(300);
                });

                // === Bekräftelse ===
                UI.Success("✅ Account and all related data deleted successfully.");
                UI.Info("You will now be logged out...");

                // Vänta kort så användaren hinner läsa
                AnsiConsole.MarkupLine("\n[grey]Press ENTER to exit...[/]");
                Console.ReadLine();

                // Returnerar true → signalerar att kontot är raderat
                return true;
            }
            catch (Exception ex)
            {
                UI.Error($"Failed to delete account: {ex.Message}");
                Logg.Log($"Error in deleting account for user '{account.UserName}': {ex}");
                return false;
            }
        }
        // === ❓ FAQ - Vanliga frågor och svar ===
        public void FAQInfo()
        {
            AnsiConsole.MarkupLine("[yellow][b]Frequently Asked Questions[/][/]\n");

            AnsiConsole.MarkupLine("[bold white]1. How is my data stored?[/]");
            AnsiConsole.MarkupLine("[grey]All your data is stored locally on your device in secure JSON files. Nothing is uploaded or synced to external servers.[/]\n");

            AnsiConsole.MarkupLine("[bold white]2. Can I add past trips?[/]");
            AnsiConsole.MarkupLine("[grey]Yes. You can enter older trips at any time to build a complete travel history.[/]\n");

            AnsiConsole.MarkupLine("[bold white]3. Can I edit or delete a trip after saving it?[/]");
            AnsiConsole.MarkupLine("[grey]Absolutely. Any trip can be updated or removed directly from the Edit Trip menu.[/]\n");

            AnsiConsole.MarkupLine("[bold white]4. How does the budget planning work?[/]");
            AnsiConsole.MarkupLine("[grey]You can plan and set a budget for your dream trip.[/]\n");

            AnsiConsole.MarkupLine("[bold white]5. Does the app support multiple accounts on one device?[/]");
            AnsiConsole.MarkupLine("[grey]Yes. Multiple users can create and access their accounts on the same device.[/]\n");

            AnsiConsole.MarkupLine("[bold white]6. Is my personal information shared with anyone?[/]");
            AnsiConsole.MarkupLine("[grey]No. All data stays locally on your device and is never shared externally.[/]\n");

            AnsiConsole.MarkupLine("[bold white]7. What happens if I forget my password?[/]");
            AnsiConsole.MarkupLine("[grey]You can use the 'Forgot Password' feature to reset your password securely.[/]\n");

            AnsiConsole.MarkupLine("[bold white]8. Why do I have a password recovery option?[/]");
            AnsiConsole.MarkupLine("[grey]The 'Forgot Password' system allows you to regain access to your account if you ever lose or forget your password.[/]\n");

            AnsiConsole.MarkupLine("[bold white]9. Can I export my trips or data?[/]");
            AnsiConsole.MarkupLine("[grey]Not yet, but exporting data (CSV, JSON or PDF) is planned for future updates.[/]\n");

            AnsiConsole.MarkupLine("[bold white]10. How do I contact support?[/]");
            AnsiConsole.MarkupLine("[grey]You can reach our support team anytime via email: [bold aqua]codecommanders25@gmail.com[/].[/]\n");

            AnsiConsole.MarkupLine("[yellow]If you have additional questions, feel free to contact our support team![/]");
        }

        // === 📧 Kontaktinformation för support ===
        public void EmailInfo()
        {
            var content =
                "[white]If you need help or have any questions, you can contact us at:[/]\n\n" +
                "[bold aqua]codecommanders25@gmail.com[/]\n\n" +
                "[grey]We do our best to respond as quickly as possible.[/]";

            var panel = new Panel(content)
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 1),
                Header = new PanelHeader("[yellow]📩 Contact Support[/]")
            };

            AnsiConsole.Write(panel);
        }

        // === 📜 Villkor & Integritetspolicy ===
        public void TermsAndPrivacy()
        {
            AnsiConsole.MarkupLine(@"
[blue bold]📜 Terms & Privacy[/]

Your data in [bold]Travel Journal[/] is stored locally on your device.  
Nothing is uploaded or shared externally.  

[b]We store:[/]
• Your account info  
• Your trips and notes  
• Budget & statistics  

[b]You control your data:[/]
• You can edit or delete your account anytime  
• Deleting your account removes all trips permanently  

[b]Security:[/]
Your data is saved as local JSON files.  
Protect your device if your information is sensitive.

[yellow]Thank you for using Travel Journal![/]
");
        }
    }
}
