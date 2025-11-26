using System;
using System.Linq;

namespace Travel_Journal.Models
{
    // Modell för användarkonto
    // Innehåller användarnamn, lösenord, e-post och andra kontoinställningar
    // Inkluderar även metoder för validering av användarnamn och lösenord
    public class Account
    {
        public string UserName { get; set; } = string.Empty; // Användarnamn
        public string Password { get; set; } = string.Empty; // Lösenord
        public DateTime CreatedAt { get; set; } = default; // Tid kontot skapades
        public decimal Savings { get; set; } = 0m; // Sparkonto
        public string? Email { get; set; } // E-postadress för 2FA och återställning
        public bool EmailVerified { get; set; } // Om e-postadressen är verifierad
        public bool TwoFactorEnabled { get; set; } // Man kan välja att aktivera 2FA och få kod varje gång man ska logga in 
        public bool IsAdmin { get; set; } = false; // Detta blir till json fil och admin panel

        [System.Text.Json.Serialization.JsonIgnore] // Ignorera vid JSON-serialisering raden nedan
        public string? PendingTwoFactorCodeHash { get; set; } // Tillfällig lagring av 2FA-kodens hash

        [System.Text.Json.Serialization.JsonIgnore] // Ignorera vid JSON-serialisering raden nedan
        public DateTime? PendingTwoFactorExpiresUtc { get; set; } // Utgångstid för den tillfälliga 2FA-koden 10 min 
        public string? DreamDestination { get; set; } // Användarens drömresmål för att spara pengar 
        public decimal? DreamBudget { get; set; } // Budget för drömresmålet


        // === 🔑 Validerar lösenord enligt regler ===
        public bool CheckPassword(string passWord)
        {
            bool longEnough = passWord.Length >= 6;
            bool hasNumber = passWord.Any(char.IsDigit);
            bool hasUpper = passWord.Any(char.IsUpper);
            bool hasLower = passWord.Any(char.IsLower);
            bool hasSpecial = passWord.Any(c => !char.IsLetterOrDigit(c));

            return longEnough && hasNumber && hasUpper && hasLower && hasSpecial;
        }

        // === 👤 Validerar användarnamn ===
        public bool CheckUserName(string userName)
        {
            return !string.IsNullOrWhiteSpace(userName); // Användarnamn får inte vara tomt eller bara mellanslag
        }
    }
}
