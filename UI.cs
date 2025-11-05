using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Travel_Journal
{
    public static class UI
    {
        // Visar en startskärm (splash) med titel och välkomsttext
        public static void Splash()
        {
            AnsiConsole.Clear(); // Rensar terminalen
            AnsiConsole.Write(new FigletText("Travel Journal").Centered().Color(Color.MediumPurple)); // Stor titel med färg
            AnsiConsole.Write(new Rule("[grey] Team 1 — Code Commanders[/]").LeftJustified()); // Tunn linje med text
            TypeOut("Welcome! Navigate using the menu below."); // Skriver ut text långsamt för snygg effekt
        }

        // Visar huvudmenyn och returnerar användarens val
        public static string MainMenu()
        {
            return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("[bold cyan]Choose:[/]") // Titel ovanför menyn
            .HighlightStyle(new Style(Color.DeepSkyBlue1)) // Färg på markerat val
            .AddChoices("Register", "Login", "Forgot password", "Exit")); // Alternativ i menyn
        }

        // Visar en avdelare/övergångsrubrik i terminalen
        public static void Transition(string title)
        {
            AnsiConsole.Write(new Rule($"[white]{title}[/]").RuleStyle("grey50"));
        }

        // Visar en statusindikator med spinner (används vid laddning eller sparning)
        public static void WithStatus(string text, Action action)
        {
            AnsiConsole.Status().Spinner(Spinner.Known.Dots2).Start(text, _ =>
            {
                action(); // Kör den åtgärd som skickats in
                System.Threading.Thread.Sleep(300); // Kort paus för snygg effekt
            });
        }

        // (Avkommenterad funktion för att visa sökvägar — används ej just nu)
        //public static void ShowPath()
        //{
        //    var p = new Panel($"[grey]Datamapp:[/] {Paths.DataDir}\n[grey]Users JSON:[/] {Paths.UsersFile}")
        //    {
        //        Border = BoxBorder.Rounded,
        //        BorderStyle = new Style(Color.Grey50),
        //        Header = new PanelHeader("Sökvägar", Justify.Center)
        //    };
        //    AnsiConsole.Write(p);
        //}

        // Visar ett grönt meddelande i panel med ikon för framgång
        public static void Success(string msg)
        {
            var panel = new Panel($"[green]✅ {msg}[/]") { Border = BoxBorder.Rounded, BorderStyle = new Style(Color.Green) };
            AnsiConsole.Write(panel);
        }

        // Visar ett rött meddelande i panel med felikon
        public static void Error(string msg)
        {
            var panel = new Panel($"[red]❌ {msg}[/]") { Border = BoxBorder.Rounded, BorderStyle = new Style(Color.Red) };
            AnsiConsole.Write(panel);
        }

        // Visar ett gult varningsmeddelande i panel
        public static void Warn(string msg)
        {
            var panel = new Panel($"[yellow]⚠ {msg}[/]") { Border = BoxBorder.Rounded, BorderStyle = new Style(Color.Yellow) };
            AnsiConsole.Write(panel);
        }

        // Skriver ut text tecken för tecken (skrivmaskinseffekt)
        public static void TypeOut(string text)
        {
            foreach (var ch in text)
            {
                // Skriver ut varje tecken i grå färg med liten fördröjning
                AnsiConsole.Markup($"[grey]{Markup.Escape(ch.ToString())}[/]");
                System.Threading.Thread.Sleep(6);
            }
            AnsiConsole.WriteLine(); // Radbrytning efter texten
        }
    }
}
