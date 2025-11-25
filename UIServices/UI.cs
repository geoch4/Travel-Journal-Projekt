using Spectre.Console;
using System;
using Travel_Journal.Data;
using Travel_Journal.Models;

namespace Travel_Journal.UIServices
{
    public static class UI
    {
        // ============================
        //   STARTSKÄRM / INTRO
        // ============================

        // Visar en startskärm med ASCII-figlet, linje och skrivmaskinseffekt.
        // Syfte: Ge användaren en snygg introduktion till programmet.
        public static void Splash()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Travel Journal").Centered().Color(Color.MediumPurple));
            AnsiConsole.Write(new Rule("[grey] Team 1 — Code Commanders[/]").LeftJustified());
            TypeOut("Navigate using the menu below.");
        }

        // Metod för att visa profil
        public static void ShowProfile(Account account)
        {
            // Skapa en tabell med Spectre.Console
            var t = new Table()
                .Border(TableBorder.Rounded)
                .BorderStyle(new Style(Color.DarkViolet));

            // Kolumner
            t.AddColumn("Attribute");
            t.AddColumn("Details");

            // Lägg till data från kontot
            t.AddRow("Username:", account.UserName);
            t.AddRow("Created:", account.CreatedAt == default
                ? "—"
                : account.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
            t.AddRow("Savings:", $"{account.Savings} kr");

            // Skriv ut tabellen i terminalen
            AnsiConsole.Write(t);
        }

        // ============================
        //   AVDELARE / RUBRIKER
        // ============================

        // Visar en snygg linje med titel. Används när man byter del i appen (t.ex. Login → Add Trip).
        public static void Transition(string title)
        {
            AnsiConsole.Write(new Rule($"[white]{title}[/]").RuleStyle("grey50"));
        }

        // ============================
        //   LOADING-SPINNER
        // ============================

        // Visar en laddningsanimation runt en metod.
        public static void WithStatus(string text, Action action)
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .Start(text, _ =>
                {
                    action();
                    Thread.Sleep(300);
                });
        }

        // ============================
        //    NY METOD: ASYNC STATUS (För e-post mm.)
        // ============================

        // Denna variant tar emot en Func<Task> istället för Action,
        // vilket gör att vi kan använda "await" inuti den.
        public static async Task WithStatusAsync(string text, Func<Task> action)
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .StartAsync(text, async ctx =>
                {
                    // Här väntar vi på att jobbet (t.ex. skicka mail) blir klart
                    await action();
                });
        }

        // ============================
        //   MEDDELANDERUTOR
        // ============================

        // Grön ruta för lyckade åtgärder.
        public static void Success(string msg)
        {
            var panel = new Panel($"[green]✅ {msg}[/]")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            };
            AnsiConsole.Write(panel);
        }

        // Röd ruta för fel.
        public static void Error(string msg)
        {
            var panel = new Panel($"[red]❌ {msg}[/]")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Red)
            };
            AnsiConsole.Write(panel);
        }

        // Gul ruta för varningar.
        public static void Warn(string msg)
        {
            var panel = new Panel($"[yellow]⚠ {msg}[/]")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Yellow)
            };
            AnsiConsole.Write(panel);
        }

        // Blå ruta för neutral info.
        public static void Info(string msg)
        {
            var panel = new Panel($"[deepskyblue1]ℹ {msg}[/]")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.DeepSkyBlue1)
            };
            AnsiConsole.Write(panel);
        }

        // ============================
        //   ASCII-SKRIVMASKIN
        // ============================

        // Skriver text tecken-för-tecken som en skrivmaskin.
        public static void TypeOut(string text)
        {
            foreach (var ch in text)
            {
                AnsiConsole.Markup($"[grey]{Markup.Escape(ch.ToString())}[/]");
                Thread.Sleep(6);
            }

            AnsiConsole.WriteLine();
        }

        // ============================
        //   INPUT METODER — MED BACK-LOGIK + LOGGNING
        // ============================

        // Används på första steget — B = gå tillbaka till föregående meny.
        // Returnerar null → signalerar att användaren backade.
        public static string? AskWithBack(string prompt)
        {
            AnsiConsole.MarkupLine("[red](B = Back to menu)[/]");

            var input = AnsiConsole.Prompt(
                new TextPrompt<string>($"{prompt}:")
                    .PromptStyle("white")
            );

            if (input.Trim().Equals("b", StringComparison.OrdinalIgnoreCase))
            {
                Logg.Log($"User pressed B → Back to menu from '{prompt}'");
                Warn("Returning to previous menu...");
                return null;
            }

            return input;
        }

        // ============================
        //   FORM-HEADER FÖR STEG-FÖR-STEG INPUT
        // ============================

        public static void ShowFormHeader(
            string title,
            string country,
            string city,
            decimal budget,
            DateTime start,
            DateTime end,
            int passengers,
            decimal? cost = null,
            int? score = null,
            string? review = null)
        {
            AnsiConsole.Clear();

            // Titel överst
            var header = new Panel("")
            {
                Border = BoxBorder.None,
                Header = new PanelHeader($"[bold]{title}[/]", Justify.Center)
            };
            AnsiConsole.Write(header);

            // Dynamiska informationsrader
            var lines = new List<string>
    {
        $"[grey]Country:[/]        {(string.IsNullOrWhiteSpace(country) ? "-" : country)}",
        $"[grey]City:[/]           {(string.IsNullOrWhiteSpace(city) ? "-" : city)}",
        $"[grey]Budget:[/]         {(budget == 0 ? "-" : budget.ToString())}",
        $"[grey]Dates:[/]          {(start == default ? "-" : start.ToString("yyyy-MM-dd"))} → {(end == default ? "-" : end.ToString("yyyy-MM-dd"))}",
        $"[grey]Passengers:[/]     {(passengers == 0 ? "-" : passengers.ToString())}"
    };

            if (cost.HasValue)
                lines.Add($"[grey]Cost:[/]           {cost.Value}");

            if (score.HasValue)
                lines.Add($"[grey]Rating:[/]         {score.Value}/5");

            if (!string.IsNullOrWhiteSpace(review))
                lines.Add($"[grey]Review:[/]         {review}");

            // Panelen som visar trip-detaljer
            var panel = new Panel(string.Join("\n", lines))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Grey),
                Header = new PanelHeader(title, Justify.Center) // <-- FIX HÄR
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }


        // ============================
        //   STEG-BAKÅT ANIMATION
        // ============================

        public static void BackOneStep()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[yellow]↩ Back to previous step...[/]\n");
        }

        // ============================
        //   STEG-PROMPTAR (STRING / DECIMAL / DATUM)
        // ============================

        // String-frågor där 0 = gå tillbaka ett steg.
        public static string? AskStep(string prompt)
        {
            AnsiConsole.MarkupLine("[grey]← Press [red]0[/] to go back one step[/]");
            var input = AnsiConsole.Ask<string>($"{prompt}:");

            if (input.Trim() == "0")
            {
                Logg.Log($"User pressed 0 → Step back from '{prompt}'");
                return null;
            }

            return input;
        }

        // Decimal-prompt där 0 = back + loggning vid felaktig siffra.
        public static decimal? AskStepDecimal(string prompt)
        {
            AnsiConsole.MarkupLine("[grey]← Press [red]0[/] to go back one step[/]");

            var input = AnsiConsole.Ask<string>($"{prompt}:");

            if (input.Trim() == "0")
            {
                Logg.Log($"User pressed 0 → Step back from decimal prompt '{prompt}'");
                return null;
            }

            if (!decimal.TryParse(input, out var val))
            {
                Logg.Log($"Invalid decimal input '{input}' for '{prompt}'");
                Error("Invalid number.");
                return AskStepDecimal(prompt);
            }

            return val;
        }

        // Datum-prompt där 0 = back + loggning vid felaktigt datum
        public static DateTime? AskStepDate(string prompt, bool showBackText = true)
        {
            if (showBackText)
                AnsiConsole.MarkupLine("[grey]← Press [red]0[/] to go back one step[/]");

            var input = AnsiConsole.Ask<string>($"{prompt}:");

            if (input.Trim() == "0")
            {
                Logg.Log($"User pressed 0 → Step back from date prompt '{prompt}'");
                return null;
            }

            if (!DateTime.TryParse(input, out var dt))
            {
                Logg.Log($"Invalid date input '{input}' for '{prompt}'");
                Error("Invalid date. Use YYYY-MM-DD.");
                return AskStepDate(prompt, showBackText);
            }

            return dt;
        }
        // Ask step int metod där 0 = back + loggning vid felaktigt heltal
        public static int? AskStepInt(string prompt)
        {
            AnsiConsole.MarkupLine("[grey]← Press [red]0[/] to go back one step[/]");
            var input = AnsiConsole.Ask<string>($"{prompt}:");
            if (input.Trim() == "0")
            {
                Logg.Log($"User pressed 0 → Step back from int prompt '{prompt}'");
                return null;
            }
            if (!int.TryParse(input, out var val))
            {
                Logg.Log($"Invalid integer input '{input}' for '{prompt}'");
                Error("Invalid number.");
                return AskStepInt(prompt);
            }
            return val;
        }
        // === Decimal-prompt med samma felhantering som AskStepDate ===
        // Läser ett decimalvärde, loggar fel och loopar tills användaren skriver rätt.
        public static decimal AskDecimal(string prompt)
        {
            // Läs in rå text
            string input = AnsiConsole.Ask<string>($"{prompt}: ");

            // Försök konvertera
            if (!decimal.TryParse(input, out var val))
            {
                // Logga exakt samma format som i datumversionen
                Logg.Log($"Invalid decimal input '{input}' for '{prompt}'");

                // Samma felmeddelande
                Error("Invalid number.");

                // Loop via rekursion – exakt som din andra metod
                return AskDecimal(prompt);
            }

            return val;
        }
        public static int AskInt(string prompt)
        {
            // Läs in rå text
            string input = AnsiConsole.Ask<string>($"{prompt}: ");
            // Försök konvertera
            if (!int.TryParse(input, out var val))
            {
                // Logga fel
                Logg.Log($"Invalid integer input '{input}' for '{prompt}'");
                // Samma felmeddelande
                Error("Invalid number.");
                // Loop via rekursion
                return AskInt(prompt);
            }
            return val;
        }

        //Enkel paus metod
        public static void Pause()
        {
            AnsiConsole.MarkupLine("\n[grey]Press [bold]ENTER[/] to continue...[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }
    }
}
