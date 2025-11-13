using Spectre.Console;
using System;

namespace Travel_Journal
{
    public static class UI
    {
        // === Splash ===
        // Visar en "startskärm" när programmet startar.
        // Den rensar terminalen, skriver en snygg titel i stort textformat,
        // och visar en liten välkomsttext med skrivmaskinseffekt.
        public static void Splash()
        {
            AnsiConsole.Clear(); // Rensar hela terminalfönstret
            AnsiConsole.Write(new FigletText("Travel Journal").Centered().Color(Color.MediumPurple)); // Stor titel med färg och centrerad
            AnsiConsole.Write(new Rule("[grey] Team 1 — Code Commanders[/]").LeftJustified()); // Tunn linje med text under titeln
            TypeOut("Welcome! Navigate using the menu below."); // Skriver ut text tecken för tecken (för animationseffekt)
        }

        // === MainMenu ===
        // Visar huvudmenyn där användaren kan välja vad de vill göra.
        // Returnerar det val användaren gör (t.ex. "Login").
        public static string MainMenu()
        {
            // Använder Spectre.Console:s SelectionPrompt för att skapa en interaktiv meny.
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[bold cyan]Choose:[/]") // Menyrubrik
                .HighlightStyle(new Style(Color.DeepSkyBlue1)) // Färg för det markerade alternativet
                .AddChoices("Register", "Login", "Forgot password", "Exit") // Menyalternativ
            );
        }

        // === Transition ===
        // Används för att visa en snygg "avdelare" (en linje med rubrik)
        // för att markera att vi byter del i programmet (t.ex. Login, Add Trip).
        public static void Transition(string title)
        {
            AnsiConsole.Write(new Rule($"[white]{title}[/]").RuleStyle("grey50"));
        }

        // === WithStatus ===
        // Visar en liten "spinner" (snurrande indikator) medan något görs i bakgrunden.
        // Används t.ex. när man sparar, laddar eller verifierar data.
        public static void WithStatus(string text, Action action)
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2) // Väljer en stil på spinnern
                .Start(text, _ =>
                {
                    action(); // Kör den metod/åtgärd som skickas in (t.ex. AccountStore.Save)
                    System.Threading.Thread.Sleep(300); // Kort paus efteråt för att hinna se animationen
                });
        }

        // === Success ===
        // Visar ett lyckat meddelande i en grön ruta med ikon ✅
        // Används t.ex. efter registrering, inloggning, sparad resa.
        public static void Success(string msg)
        {
            var panel = new Panel($"[green]✅ {msg}[/]")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            };
            AnsiConsole.Write(panel);
        }

        // === Error ===
        // Visar ett felmeddelande i en röd ruta med ikon ❌
        // Används t.ex. när lösenordet är fel eller kontot inte finns.
        public static void Error(string msg)
        {
            var panel = new Panel($"[red]❌ {msg}[/]")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Red)
            };
            AnsiConsole.Write(panel);
        }

        // === Warn ===
        // Visar ett varningsmeddelande i en gul ruta ⚠️
        // Används t.ex. om användaren skriver in ogiltiga datum eller siffror.
        public static void Warn(string msg)
        {
            var panel = new Panel($"[yellow]⚠ {msg}[/]")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Yellow)
            };
            AnsiConsole.Write(panel);
        }

        // === Info ===
        // Visar neutrala informationsmeddelanden i blå färg ℹ️
        // (Lades till för att användas i App.cs vid t.ex. utloggning.)
        public static void Info(string msg)
        {
            var panel = new Panel($"[deepskyblue1]ℹ {msg}[/]")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.DeepSkyBlue1)
            };
            AnsiConsole.Write(panel);
        }

        // === TypeOut ===
        // Skriver ut text tecken för tecken med liten fördröjning (skrivmaskinseffekt).
        // Används i introtexten ("Welcome! Navigate using the menu below.").
        public static void TypeOut(string text)
        {
            foreach (var ch in text)
            {
                // Varje tecken skrivs ut i grå färg med en minimal paus
                AnsiConsole.Markup($"[grey]{Markup.Escape(ch.ToString())}[/]");
                System.Threading.Thread.Sleep(6);
            }

            // Avslutar med radbrytning efter texten
            AnsiConsole.WriteLine();
        }
        // I UI-klassen
        public static T WithStatus<T>(string message, Func<T> action)
        {
            T result = default!;
            WithStatus(message, () => { result = action(); });
            return result;
        }
        public static string? AskWithBack(string prompt)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>($"[green]{prompt}[/] ([red]Press 0 to go back[/]):")
                    .PromptStyle("white")
            );

            if (input == "0")
            {
                UI.Warn("Going back...");
                return null; // SIGNAL till koden att användaren avbröt
            }

            return input;
        }
    }
}
