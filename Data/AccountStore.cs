using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Travel_Journal.Models;

namespace Travel_Journal.Data
{
    public static class AccountStore 
    {
        private static List<Account> accounts = new();

        public static void LoadWithProgress()
        {
            // Skapar datamappen om den inte redan finns
            Directory.CreateDirectory(Paths.DataDir);

            // Startar en progressbar med Spectre.Console för att visa laddning i terminalen
            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
            // Kolumn för beskrivning av vad som laddas
            new TaskDescriptionColumn(),
            // Kolumn som visar själva progress-stapeln
            new ProgressBarColumn(),
            // Kolumn som visar procentvärdet
            new PercentageColumn(),
            // Kolumn som visar en snurrande animation medan laddning pågår
            new SpinnerColumn(Spinner.Known.Dots)
                })
                .Start(ctx =>
                {
                    // Skapar en ny uppgift ("task") för att representera laddningen
                    var t = ctx.AddTask("🧭 Preparing your travel profile", maxValue: 100);

                    // Simulerar laddning i steg om 25%, med kort paus mellan varje
                    for (int i = 0; i <= 100; i += 25)
                    {
                        t.Value = i; // Uppdaterar progressens nuvarande värde
                        Thread.Sleep(500); // Väntar 60 millisekunder för visuell effekt
                    }

                    // Om användarfilen existerar, läs in den
                    if (File.Exists(Paths.UsersFile))
                    {
                        var json = File.ReadAllText(Paths.UsersFile); // Läser in hela JSON-filen som text
                                                                      // Försöker deserialisera JSON-texten till en lista av Account-objekt
                                                                      // Om deserialiseringen misslyckas (t.ex. tom fil) används en tom lista istället
                        accounts = JsonSerializer.Deserialize<List<Account>>(json) ?? new();
                    }
                });
        }



        public static void Save()
        {
            // Skapar mappen där data ska sparas om den inte redan finns
            Directory.CreateDirectory(Paths.DataDir);

            // Om listan med konton är null, avsluta metoden direkt (inget att spara)
            if (accounts == null) return;

            // Serialiserar listan av Account-objekt till en JSON-sträng
            var json = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });

            // Skriver JSON-strängen till filen (sparar användardata)
            File.WriteAllText(Paths.UsersFile, json);
        }



        // Kontrollerar om ett konto med det angivna användarnamnet redan finns i listan
        public static bool Exists(string username) => accounts.Any(a => a.UserName == username);

        // Lägger till ett nytt konto i listan över konton
        public static void Add(Account acc) => accounts.Add(acc);

        // Hämtar ett konto baserat på användarnamnet (returnerar null om inget hittas)
        public static Account? Get(string username) => accounts.FirstOrDefault(a => a.UserName == username);

        // Uppdaterar ett befintligt konto i listan om det redan finns
        public static void Update(Account acc)
        {
            // Hittar indexet för kontot som har samma användarnamn
            var idx = accounts.FindIndex(a => a.UserName == acc.UserName);

            // Om kontot finns (index >= 0), ersätt det gamla kontot med det nya
            if (idx >= 0) accounts[idx] = acc;
        }

        // Hämtar alla konton i listan hjälper för bland annat för Admin-funktionalitet/Panel
        public static List<Account> GetAll()
        {
            return accounts;
        }

    }
}




