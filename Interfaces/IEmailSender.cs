using System.Threading.Tasks;

namespace Travel_Journal.Interfaces
{
    public interface IEmailSender
    {
        // Task används för att programmet ska kunna göra andra saker medan det väntar på att e-post ska skickas, 
        // vilket förhindrar att programmet fryser eller blir långsamt.
        // Metoden tar emot tre parametrar: mottagarens e-postadress (toEmail),
        // ämnet för e-postmeddelandet (subject) och själva meddelandets innehåll (body).
        Task SendAsync(string toEmail, string subject, string body);
    }
}