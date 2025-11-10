using Spectre.Console;
using System;
using System.Threading;

namespace Travel_Journal
{
    public static class App
    {
        // === 🚀 Huvudmetod: startar hela programmet ===
        public static void Run()
        {

            // Visa en snygg splashscreen med titel
            UI.Splash();

            // Ladda användarkonton från users.json med progressbar
            AccountStore.LoadWithProgress();

            // Skapa en AuthService-instans (utan AccountStore.Instance – vi använder statisk AccountStore)
            var auth = new AuthService();

            // 5️⃣ Starta huvudmenyn – körs tills användaren väljer "Exit"
            while (true)
            {
                var choice = UI.MainMenu(); // Meny: Register / Login / Forgot password / Exit

                switch (choice)
                {
                    case "Register":
                        UI.Transition("Register Account");

                        var user = AnsiConsole.Ask<string>("Username:");
                        var pass = AnsiConsole
                            .Prompt(new TextPrompt<string>("Password:").Secret());

                        auth.Register(user, pass);
                        break;

                    case "Login":
                        UI.Transition("Login");

                        var u = AnsiConsole.Ask<string>("Username:");
                        var p = AnsiConsole
                            .Prompt(new TextPrompt<string>("Password:").Secret());

                        var acc = auth.Login(u, p);

                        if (acc != null)
                        {
                            // Om inloggning lyckades – starta inloggad session
                            var session = new UserSession(acc);
                            session.Start();
                        }
                        break;

                    case "Forgot password":
                        UI.Transition("Forgot Password");

                        var name = AnsiConsole.Ask<string>("Username:");
                        var code = AnsiConsole.Ask<string>("Recovery code:");
                        var newPwd = AnsiConsole
                            .Prompt(new TextPrompt<string>("New password:").Secret());
                        var confirm = AnsiConsole
                            .Prompt(new TextPrompt<string>("Confirm password:").Secret());

                        auth.ResetPassword(name, code, newPwd, confirm);
                        break;

                    default:
                        UI.Transition("Exiting...");
                        AnsiConsole.MarkupLine("[green]Thank you for using Travel Journal![/]");
                        Thread.Sleep(1000);
                        return;
                }
            }
        }
    }
}
