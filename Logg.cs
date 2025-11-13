using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Travel_Journal
{
    public static class Logg
    {
    
        // Loggfilen hamnar i samma data-mapp som users.json
        private static readonly string LogFile = Path.Combine(Paths.DataDir, "log.json");

        public static void Log(string message) //skickar in själva texten vi vill logga
        {
            try
            {
                // 1. Se till att data-mappen finns
                Directory.CreateDirectory(Paths.DataDir);

                // 2. Bygg en enkel loggrad (JSON-liknande)
                string line = $"{{\"time\": \"{DateTime.Now}\", \"msg\": \"{message}\"}}";

                // 3. Lägg till raden längst ner i log.json
                File.AppendAllText(LogFile, line + Environment.NewLine);
            }
            catch (Exception ex) 
            {
                // Viktigt: loggning får aldrig krascha programmet
                UI.Error($"Failed to save to Logg{ex.Message}");//catch kommer ge oss detta meddelandet
            }
        }
    }
}


//try...catch för att testa att skriva loggen samt fånga eventuella fel utan att krascha programmet
//loggning är alltid något extra, får inte slå ut appen
//Paths.DataDir---sökväg till vår datamapp(finns redan i projektet)
//Directory.CreateDirectory(...): Innan vi skriver en fil måste mappen finnas, Om mappen inte finns → skapas den.
//Om mappen finns → händer inget, det är safe att anropa ändå.
//static---slipper skapa objekt
//string line----vi sparar 2 saker varje gång(VAR det hände och VAD hände)-lägger den på en rad som ser ut som JSON
//“Append” betyder: lägg till text på slutet.
//Om filen inte finns → den skapas.
//Om filen finns → raden läggs längst ner.
//Environment.NewLine ger en radbrytning så att varje loggpost hamnar på egen rad.
//tom catch---om något går fel då catch körs