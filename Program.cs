

namespace Travel_Journal
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; // All text visas korrekt inklusive emojis
            await App.Run();
        }
    }
}
