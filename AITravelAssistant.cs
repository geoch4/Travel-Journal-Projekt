using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Spectre.Console;

namespace Travel_Journal
{
    /// <summary>
    /// AITravelAssistant använder OpenAI:s API för att generera reseförslag 
    /// baserat på användarens budget, reslängd och typ av resa.
    /// </summary>
    public class AITravelAssistant
    {
        // HttpClient används för att skicka HTTP-anrop till OpenAI:s API
        private readonly HttpClient _httpClient;

        // API-nyckeln hämtas från miljövariabeln "API_KEY"
        private readonly string _apiKey;

        // === Konstruktor ===
        public AITravelAssistant()
        {
            // Hämta API-nyckeln från datorns miljövariabler
            // (Om den inte finns kastas ett felmeddelande)
            _apiKey = Environment.GetEnvironmentVariable("API_KEY")
                ?? throw new InvalidOperationException("❌ API_KEY not found. Set it with: setx API_KEY \"sk-...\"");

            // Skapa en ny HTTP-klient som ska prata med OpenAI:s API
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };

            // Lägg till auktorisering i headern (krävs för att OpenAI ska godkänna anropet)
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        // === Huvudmetod: Skickar prompt till OpenAI och hämtar svar ===
        public async Task<string> GetTravelSuggestionAsync(decimal budget, string tripType, int durationDays)
        {
            // Här bygger vi själva frågan (prompten) som skickas till AI:n
            // Den innehåller all information användaren angivit
            var prompt = $@"
You are a travel planner AI. 
Suggest one travel destination and a short itinerary based on:
- Budget: {budget} SEK
- Trip type: {tripType}
- Duration: {durationDays} days
Give me 3 parts: 
1️⃣ Destination  
2️⃣ Why it's perfect for this traveler  
3️⃣ Suggested activities.";

            // Spinner-animation i terminalen (visar att AI:n "tänker")
            return await AnsiConsole.Status()
                .StartAsync("🧠 Thinking of your next adventure...", async ctx =>
                {
                    // Skapar ett JSON-objekt med data som OpenAI:s API kräver
                    var payload = new
                    {
                        model = "gpt-4o-mini", // Den modell som används (snabbare, billigare variant)
                        messages = new[]
                        {
                            new { role = "system", content = "You are a friendly AI travel assistant." },
                            new { role = "user", content = prompt }
                        },
                    };

                    // Serialisera (omvandla) objektet till en JSON-text
                    var json = JsonSerializer.Serialize(payload);

                    // Gör om JSON-texten till HTTP-innehåll som kan skickas i POST-anropet
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    try
                    {
                        // Skicka POST-anropet till OpenAI:s endpoint "chat/completions"
                        var response = await _httpClient.PostAsync("chat/completions", content);

                        // Säkerställ att svaret är OK (200)
                        response.EnsureSuccessStatusCode();

                        // Läs in hela svaret som en textsträng
                        var result = await response.Content.ReadAsStringAsync();

                        // Gör om texten till ett JSON-dokument så vi kan plocka ut AI:ns svar
                        using var doc = JsonDocument.Parse(result);

                        // Hämta ut texten från AI:ns svar
                        string message = doc.RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString() ?? "No suggestion available.";

                        // Trimma bort onödiga tomrader och returnera resultatet
                        return message.Trim();
                    }
                    catch (Exception ex)
                    {
                        // Om något går fel (t.ex. ingen internetanslutning, API-fel, timeout)
                        // så visas ett tydligt felmeddelande i terminalen.
                        UI.Error($"AI request failed: {ex.Message}");

                        // Returnera en standardtext istället för att krascha
                        return "Could not generate a travel suggestion.";
                    }
                });
        }

        // === 🤖 AI Travel Assistant ===
        // Använder OpenAI för att ge ett reseförslag baserat på användarens input
        public async Task ShowAISuggestionAsync()
        {
            UI.Transition("AI Travel Assistant 🤖✈️");

            // Be användaren om resepreferenser
            var budget = AnsiConsole.Ask<decimal>("What is your [green]budget (SEK)[/]?");
            var type = AnsiConsole.Ask<string>("What kind of [blue]trip[/] do you want? (e.g. city, beach, adventure, culture)");
            var days = AnsiConsole.Ask<int>("How many [yellow]days[/] do you plan to travel?");

            // Skapa AI-klassen och hämta förslag från OpenAI
            var ai = new AITravelAssistant();
            string suggestion = await ai.GetTravelSuggestionAsync(budget, type, days);

            // Visa resultatet i en snygg panel med färg och ram
            var panel = new Panel($"[white]{suggestion}[/]")
            {
                Header = new PanelHeader("🌍 Your AI Travel Suggestion"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Cyan1)
            };
            AnsiConsole.Write(panel);
        }
    }
}
