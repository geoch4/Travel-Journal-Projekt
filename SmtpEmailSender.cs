using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Travel_Journal.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _pass;
        private readonly bool _enableSsl;

        public SmtpEmailSender()
        {
            _host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
            _port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var p) ? p : 587;
            _user = Environment.GetEnvironmentVariable("SMTP_USER") ?? throw new InvalidOperationException("SMTP_USER not set");
            _pass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? throw new InvalidOperationException("SMTP_PASS not set");
            _enableSsl = true;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            using var smtp = new SmtpClient(_host, _port)
            {
                EnableSsl = _enableSsl,
                Credentials = new NetworkCredential(_user, _pass)
            };

            using var mail = new MailMessage(_user, toEmail, subject, body);
            await smtp.SendMailAsync(mail);
        }
    }
}