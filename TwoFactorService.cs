using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Travel_Journal.Email;

namespace Travel_Journal.Security
{
    public class TwoFactorService
    {
        private readonly IEmailSender _emailSender;
        public TwoFactorService(IEmailSender emailSender) => _emailSender = emailSender;

        public string GenerateNumericCode(int digits = 6)
        {
            // Kryptografiskt säkert
            var bytes = RandomNumberGenerator.GetBytes(4);
            uint v = BitConverter.ToUInt32(bytes, 0) % (uint)Math.Pow(10, digits);
            return v.ToString(new string('0', digits));
        }

        public static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }

        public async Task<bool> SendEmailCodeAsync(Account acc, string purpose = "Din verifieringskod")
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
            string body = $"Hej {acc.UserName}!\n\nDin kod är: {code}\nGiltig i 10 minuter.\n\n/Travel Journal";

            await _emailSender.SendAsync(acc.Email, subject, body);
            return true;
        }

        public bool VerifyCode(Account acc, string inputCode)
        {
            if (acc.PendingTwoFactorExpiresUtc is null || acc.PendingTwoFactorExpiresUtc < DateTime.UtcNow)
                return false;

            var hash = Sha256(inputCode ?? string.Empty);
            return string.Equals(hash, acc.PendingTwoFactorCodeHash, StringComparison.OrdinalIgnoreCase);
        }

        public void ClearPending(Account acc)
        {
            acc.PendingTwoFactorCodeHash = null;
            acc.PendingTwoFactorExpiresUtc = null;
        }
    }
}