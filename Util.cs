using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Travel_Journal
{
    public static class Util
    {

        // Enkel, lättskriven återställningskod, t.ex. 5821-9044
        public static string GenerateRecoveryCode()
        {
            var rnd = new Random();
            int Next4() => rnd.Next(0, 10000);
            return $"{Next4():0000}-{Next4():0000}";
        }
    }
}
