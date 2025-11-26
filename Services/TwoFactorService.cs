using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Travel_Journal.Interfaces;
using Travel_Journal.Models;

namespace Travel_Journal.Services
{
    // Email för tvåfaktorsautentisering
    public class TwoFactorService
    {
        private readonly IEmailSender _emailSender;
        public TwoFactorService(IEmailSender emailSender) => _emailSender = emailSender;

        // / Genererar en numerisk kod med angivet antal siffror (digits) 6 som standard
        public string GenerateNumericCode(int digits = 6)
        {
            // Kryptografiskt säkert
            var bytes = RandomNumberGenerator.GetBytes(4);
            uint v = BitConverter.ToUInt32(bytes, 0) % (uint)Math.Pow(10, digits);
            return v.ToString(new string('0', digits));
        }
        // Den tar en text och gör om den till en SHA-256-hash
        // / Detta används för att säkert lagra verifieringskoder
        // en hash är en unik kod som inte går att göra tillbaka till text
        public static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }

        // Skickar en verifieringskod via e-post till användaren
        public async Task<bool> SendEmailCodeAsync(Account acc, string purpose = "Your verificationcode")
        {
            if (string.IsNullOrWhiteSpace(acc.Email))
            {
                AnsiConsole.MarkupLine("[red]No email on account.[/]");
                return false;
            }

            var code = GenerateNumericCode(6);
            acc.PendingTwoFactorCodeHash = Sha256(code);
            acc.PendingTwoFactorExpiresUtc = DateTime.UtcNow.AddMinutes(10);

            string subject = purpose;
            string body = $"Hi {acc.UserName}!\n\nYour code is: {code}\nValid for 10 minutes.\n\n/Team Travel Journal";

            await _emailSender.SendAsync(acc.Email, subject, body);
            return true;
        }

        // Verifierar att den inmatade koden matchar den sparade hashen och inte har gått ut
        public bool VerifyCode(Account acc, string inputCode)
        {
            if (acc.PendingTwoFactorExpiresUtc is null || acc.PendingTwoFactorExpiresUtc < DateTime.UtcNow)
                return false;

            var hash = Sha256(inputCode ?? string.Empty);
            return string.Equals(hash, acc.PendingTwoFactorCodeHash, StringComparison.OrdinalIgnoreCase);
        }

        // Rensar den sparade koden och utgångstiden från kontot
        public void ClearPending(Account acc)
        {
            acc.PendingTwoFactorCodeHash = null;
            acc.PendingTwoFactorExpiresUtc = null;
        }
    }
}