using System.Diagnostics;
using System.Text.Json;

namespace Travel_Journal
{
    public class WorldMapService
    {
        private readonly TripService _tripService;

        public WorldMapService(TripService tripService)
        {
            _tripService = tripService;
        }

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
                return;
            }

            // === 2. Hämta besökta länder ===
            var visited = _tripService.GetVisitedCountryNamesForMap();

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
    }
}
