using Spectre.Console;
using System;
using System.Threading.Tasks;

namespace Travel_Journal
{
    /// <summary>
    /// App-klassen är startpunkten för programmet. 
    /// Här hanteras huvudmenyn: Register, Login, Forgot password och Exit.
    /// När användaren loggar in startas en ny UserSession.
    /// </summary>
    public static class App
    {
        // === 🚀 Huvudmetod: startar hela programmet ===
        public static async Task Run()
        {
            // 🖼️ Visar en snygg startskärm / splash med animation och titel
            UI.Splash();

            // 💾 Laddar alla användarkonton från users.json-filen
            // Progressbar visas med Spectre.Console för snygg effekt
            AccountStore.LoadWithProgress();

            // 🔐 Skapar AuthService-instans (hanterar inloggning, registrering, lösenord)
            var auth = new AuthService();

            // 🔁 Programmet körs i en evig loop tills användaren väljer "Exit"
            while (true)
            {
                // 🧭 Visar huvudmenyn med Spectre.Console och sparar användarens val
                var choice = UI.MainMenu(); // Alternativ: Register / Login / Forgot password / Exit

                switch (choice)
                {
                    case "Register":
                        // === Registrera nytt konto ===
                        UI.Transition("Register Account"); // Snygg övergångstext

                        // Fråga efter användarnamn och lösenord
                        var user = AnsiConsole.Ask<string>("Username:");
                        var pass = AnsiConsole
                            .Prompt(new TextPrompt<string>("Password:").Secret());

                        // Skicka vidare till AuthService för att skapa kontot
                        auth.Register(user, pass);
                        break;

                    case "Login":
                        // === Logga in på befintligt konto ===
                        UI.Transition("Login");

                        // Fråga användaren om inloggningsuppgifter
                        var u = AnsiConsole.Ask<string>("Username:");
                        var p = AnsiConsole
                            .Prompt(new TextPrompt<string>("Password:").Secret());

                        // Försök hitta matchande konto via AuthService
                        var acc = auth.Login(u, p);

                        if (acc != null)
                        {
                            // ✅ Inloggningen lyckades!
                            // Skapa en UserSession som hanterar allt när användaren är inloggad
                            var session = new UserSession(acc);

                            // 🕒 Vänta (await) på att sessionen är klar innan huvudmenyn visas igen
                            // Detta gör att huvudmenyn "pausas" tills användaren loggar ut
                            await session.Start();
                        }
                        break;

                    case "Forgot password":
                        // === Återställ lösenord ===
                        UI.Transition("Forgot Password");

                        // Fråga användaren om användarnamn, återställningskod och nytt lösenord
                        var name = AnsiConsole.Ask<string>("Username:");
                        var code = AnsiConsole.Ask<string>("Recovery code:");
                        var newPwd = AnsiConsole
                            .Prompt(new TextPrompt<string>("New password:").Secret());
                        var confirm = AnsiConsole
                            .Prompt(new TextPrompt<string>("Confirm password:").Secret());

                        // Försök återställa via AuthService
                        auth.ResetPassword(name, code, newPwd, confirm);
                        break;

                    default:
                        // === Avslutar programmet ===
                        UI.Transition("Exiting...");
                        AnsiConsole.MarkupLine("[green]Thank you for using Travel Journal![/]");

                        // Kort paus innan konsolen stängs för snygg exit-animation
                        await Task.Delay(1000);
                        return; // Avslutar Run() och programmet
                }
            }
        }
    }
}
