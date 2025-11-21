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