using System.Diagnostics;
using System.Text.Json;
using Travel_Journal.Data;
using Travel_Journal.Models;
using Travel_Journal.UIServices;
using Travel_Journal.Services;

namespace Travel_Journal.Services
{
    // Tjänst för att generera och visa en världskarta med besökta länder som dom tar från listorna i TripService
    public class WorldMapService
    {
        // private fält för TripService
        private readonly TripService _tripService;

        // Hjälpmetod för att hämta alla resor
        private List<Trip> trips => _tripService.GetAllTrips();

        // / === Konstruktor ===
        public WorldMapService(TripService tripService)
        {
            _tripService = tripService;
        }

        // / Öppnar världskartan i webbläsaren
        public void OpenWorldMap()
        {
            // === 1. Projektets rotmapp ===
            string projectDir = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;

            // Template i projektet
            string projectMapDir = Path.Combine(projectDir, "map");

            // Output i bin/
            string outputDir = Path.Combine(AppContext.BaseDirectory, "map");
            Directory.CreateDirectory(outputDir);

            string template = Path.Combine(projectMapDir, "worldmap_template.html");
            string output = Path.Combine(outputDir, "worldmap.html");

            // === Kontroll: template finns ===
            if (!File.Exists(template))
            {
                UI.Error($"Fel: worldmap_template.html saknas!\nLetade i: {template}");
                Logg.Log($"WorldMapService.OpenWorldMap: worldmap_template.html saknas i {template}");
                return;
            }

            // === 2. Hämta besökta länder ===
            var visited = GetVisitedCountryNamesForMap();

            // === 3. JS-array ===
            string jsArray = JsonSerializer.Serialize(visited);

            // === 4. Ersätt placeholder ===
            string html = File.ReadAllText(template);
            html = html.Replace("VISITED_PLACEHOLDER", jsArray);

            // === 5. Spara output-fil ===
            File.WriteAllText(output, html);

            // === 6. Öppna kartan ===
            Process.Start(new ProcessStartInfo
            {
                FileName = output,
                UseShellExecute = true
            });

            // === 7. Vänta på ENTER och återgå ===
            Console.WriteLine();
            Console.WriteLine("📍 World Map opened in your browser.");
            Console.WriteLine("Press ENTER to return to Travel Journal...");
            Console.ReadLine();
        }

        // === Logik för Världskartan ===

        /// <summary>
        /// Denna metod bearbetar data för världskartan.
        /// Den filtrerar ut slutförda resor och översätter landsnamn till GeoJSON-format.
        /// </summary>
        public List<string> GetVisitedCountryNamesForMap()
        {
            // 1. Filtrera: Hämta bara resor som är markerade som "Completed"
            var completed = trips
                .Where(t => t.IsCompleted)
                .Select(t => t.Country?.Trim())         // Hämta landsnamnet och ta bort mellanslag
                .Where(c => !string.IsNullOrWhiteSpace(c)) // Ignorera tomma rader
                .Distinct(StringComparer.OrdinalIgnoreCase) // Ta bort dubbletter (Har man varit i Norge 2 ggr visas det bara en gång)
                .ToList();

            var result = new List<string>();

            // 2. Mappa: Översätt vanliga namn till officiella namn (t.ex. "USA" -> "United States of America")
            foreach (var country in completed)
            {
                var key = country!.ToLowerInvariant();

                if (CountryAlias.TryGetValue(key, out var mapped))
                    result.Add(mapped); // Använd det officiella namnet från vår lista
                else
                    result.Add(country); // Använd namnet användaren skrev in
            }
            return result;
        }

        // Dictionary för lands-alias. 
        // Static readonly eftersom listan ser likadan ut för alla användare.
        private static readonly Dictionary<string, string> CountryAlias = new()
        {
            ["usa"] = "United States of America",
            ["us"] = "United States of America",
            ["united states"] = "United States of America",
            ["uk"] = "United Kingdom",
            ["england"] = "United Kingdom",
            ["scotland"] = "United Kingdom",
            ["great britain"] = "United Kingdom",
            ["south korea"] = "Korea, Republic of",
            ["north korea"] = "Korea, Democratic People's Republic of",
            ["laos"] = "Lao People's Democratic Republic",
            ["vietnam"] = "Viet Nam",
            ["iran"] = "Iran, Islamic Republic of",
            ["bolivia"] = "Bolivia, Plurinational State of",
            ["tanzania"] = "Tanzania, United Republic of",
            ["moldova"] = "Moldova, Republic of",
            ["venezuela"] = "Venezuela, Bolivarian Republic of",
            ["syria"] = "Syrian Arab Republic"
        };
    }
}
