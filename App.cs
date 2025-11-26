using Microsoft.Extensions.Logging;
using Spectre.Console;
using Travel_Journal.Data;
using Travel_Journal.Models;
using Travel_Journal.Services;
using Travel_Journal.UIServices;

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
            //Lägger till en loggrad
            Logg.Log("Application started."); //Loggar att appen har startat-testar loggern med en enkel logg vid start
            // 🖼️ Visar en snygg startskärm / splash med animation och titel
            UI.Splash();

            // 💾 Laddar alla användarkonton från users.json-filen
            // Progressbar visas med Spectre.Console för snygg effekt
            AccountStore.LoadWithProgress();

            // 🔐 Skapar AuthService-instans (hanterar inloggning, registrering, lösenord)
            var auth = new AuthService();
            // 📂 Skapar DataStore för konton (används av AdminService)
            var adminService = new AdminService();

            // 🔁 Programmet körs i en evig loop tills användaren väljer "Exit"
            while (true)
            {
                // 🧭 Visar huvudmenyn med Spectre.Console och sparar användarens val
                var choice = MenuService.MainMenu(); // Alternativ: Register / Login / Forgot password / Exit

                switch (choice)
                {
                    case "Register":
                        await auth.RegisterWithEmailVerificationAsync();
                        break;

                    case "Login":
                        // Försöker logga in användaren och få tillbaka ett Account-objekt
                        var acc = auth.Login();

                        // Om acc är null betyder det att användaren avbröt eller misslyckades
                        if (acc != null)
                        {
                            // Admin-kontroll
                            if (acc.IsAdmin)
                            {
                                AdminMenu.ShowAdminMenu(adminService);
                                break;
                            }

                            // ✅ Inloggningen lyckades!
                            // Starta sessionen för användaren
                            var session = new UserSession(acc);
                            await session.Start();
                        }
                        break;

                    case "Forgot password":
                        await auth.ForgotPasswordAsync();
                        break;

                    default:
                        // === Avslutar programmet ===
                        UI.Transition("Exiting...");
                        Logg.Log("Application ended."); //Loggar att appen har avslutats
                        AnsiConsole.MarkupLine("[green]Thank you for using Travel Journal![/]");

                        // Kort paus innan konsolen stängs för snygg exit-animation
                        await Task.Delay(1000);
                        return; // Avslutar Run() och programmet
                }
            }
        }
    }
}
