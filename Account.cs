using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace Travel_Journal
{
    public class Account // Klass med attribut för användare och metoder för att registrera, logga in, kolla lösen och användarnamn
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RecoveryCode { get; set; } = string.Empty; // STEG 1
        public DateTime CreatedAt { get; set; } = default; // STEG 1


        public bool Register(string userName, string passWord) 
        {
            var passwordOk = CheckPassword(passWord);
            var userNameOk = CheckUserName(userName);


            if (!passwordOk || !userNameOk)
                return false;


            UserName = userName;
            Password = passWord;
            return true;
        }


        public bool Login(string userName, string passWord)
        {
            return userName == UserName && passWord == Password;
        }


        public bool CheckPassword(string passWord)
        {
            bool longEnough = passWord.Length >= 6;
            bool hasNumber = passWord.Any(char.IsDigit);
            bool hasUpper = passWord.Any(char.IsUpper);
            bool hasLower = passWord.Any(char.IsLower);
            bool hasSpecial = passWord.Any(c => !char.IsLetterOrDigit(c));


            return longEnough && hasNumber && hasUpper && hasLower && hasSpecial;
        }


        public bool CheckUserName(string userName)
        {
            return userName.Length >= 1;
        }
    }
}
