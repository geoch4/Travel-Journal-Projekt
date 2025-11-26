using Spectre.Console;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Travel_Journal.Data;
using Travel_Journal.UIServices;

namespace Travel_Journal.Services
{
    /// <summary>
    /// AITravelAssistant använder OpenAI:s API för att generera reseförslag 
    /// baserat på användarens budget, reslängd och typ av resa.
    /// </summary>
    public class AITravelAssistant
    {
        // http klient är verktyget för att skicka anrop till webbtjänsten
        // vi använder private readonly för att den bara ska användas inom denna klass
        private readonly HttpClient _httpClient;

        // API-nyckeln hämtas från miljövariabeln "API_KEY"
        private readonly string _apiKey;

        // === Konstruktor ===
        public AITravelAssistant()
        {
            // Hämta API-nyckeln från datorns miljövariabler
            // (Om den inte finns kastas ett felmeddelande)
            var key = Environment.GetEnvironmentVariable("API_KEY");
            if (key == null)
            {
                Logg.Log("API_KEY environment variable missing.");
                throw new InvalidOperationException("❌API_KEY not found.Set it with: setx API_KEY 'sk-....' ");
            }

            _apiKey = key;

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
            // Jag antar att detta är C# baserat på syntaxen ($@" ... "), men principen är densamma för alla språk.

            var prompt = $@"
You are an experienced, enthusiastic travel planner AI.

Your task:
Suggest exactly ONE travel destination that fits the traveler's budget, trip type and duration.
Assume the budget ({budget} SEK) is the TOTAL budget for the whole trip (flights/transport, accommodation and activities).
Only suggest destinations that are realistic within this budget.

Traveler info:
Budget: {budget} SEK
Trip type: {tripType}
Duration: {durationDays} days

Write the answer in clear English using Markdown. Structure the response exactly like this:

# 🌍 A [Adjective] Trip to [Destination]

1) A short, engaging intro (2–3 sentences).

---

## 📍 Destination: [City, Country]
Very short explanation why this fits the budget and trip type.

---

## 📅 Itinerary: [Duration] Days

(Repeat this block for each day)
### Day X: [Title of the day]
* **Morning:** ...
* **Afternoon:** ...
* **Evening:** ...

---

## ✅ Why this fits you:
* 💎 **Budget:** [Point about cost]
* 🚶 **Accessibility:** [Point about navigation/transport]
* 🎨 **Experience:** [Point about culture/activities]

Important constraints:
- Use emojis to make headers and lists visual and friendly.
- Use horizontal rules (---) to separate main sections.
- Keep the tone friendly and inspiring.
- Do not ask follow-up questions.
";
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
                        Logg.Log($"AITravelAssistant error: {ex}");

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
            var budgetstring = UI.AskWithBack("What is your [bold]budget[/] in SEK?");

            if (budgetstring == null) return; // eller gå till föregående meny

            // Konvertera budgetsträngen till decimal
            decimal budget = decimal.Parse(budgetstring);

            // Be användaren om typ av resa
            var type = AnsiConsole.Ask<string>("What kind of [blue]trip[/] do you want? (e.g. city, beach, adventure, culture)");

            // Be användaren om reslängd i dagar
            var days = UI.AskInt("How many [yellow]days[/] do you plan to travel?");

            // Hämta reseförslaget från AI:n
            string suggestion = await GetTravelSuggestionAsync(budget, type, days);

            // Visa resultatet i en snygg panel med färg och ram
            var panel = new Panel($"[white]{suggestion}[/]")
            {
                Header = new PanelHeader("🌍 Your AI Travel Suggestion"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Aqua)
            };
            AnsiConsole.Write(panel);
        }
    }
}
