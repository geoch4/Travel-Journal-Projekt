using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Travel_Journal.Interfaces;

namespace Travel_Journal.Services
{
    // email-sändare som använder SMTP för att skicka e-postmeddelanden
    public class SmtpEmailSender : IEmailSender
    {
        private readonly string _host; // SMTP-servervärd
        private readonly int _port; // SMTP-serverport
        private readonly string _user; // SMTP-användarnamn
        private readonly string _pass; // SMTP-lösenord
        private readonly bool _enableSsl; // Om SSL ska användas

        /// === Konstruktor: hämtar SMTP-inställningar från miljövariabler ===
        public SmtpEmailSender()
        {
            // Hämta SMTP-inställningar från miljövariabler
            _host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
            _port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var p) ? p : 587;
            _user = Environment.GetEnvironmentVariable("SMTP_USER") ?? throw new InvalidOperationException("SMTP_USER not set");
            _pass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? throw new InvalidOperationException("SMTP_PASS not set");
            _enableSsl = true;
        }

        // programmet starta en uppgift för att skicka e-post till användaren
        public async Task SendAsync(string toEmail, string subject, string body)
        {
            // Skapa och konfigurera SMTP-klienten
            using var smtp = new SmtpClient(_host, _port)
            {
                EnableSsl = _enableSsl,
                Credentials = new NetworkCredential(_user, _pass)
            };

            using var mail = new MailMessage(_user, toEmail, subject, body);
            await smtp.SendMailAsync(mail); // Skicka e-postmeddelandet och vänta på att det ska slutföras
        }
    }
}