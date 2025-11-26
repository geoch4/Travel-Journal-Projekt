using System;
using System.IO;

namespace Travel_Journal.Data
{
    // Statisk klass för att hantera filvägar i applikationen
    public static class Paths
    {
        // Bas-katalog (där .exe körs)
        public static readonly string BaseDir = AppContext.BaseDirectory;

        // Mapp där alla JSON-filer sparas
        public static readonly string DataDir = Path.Combine(BaseDir, "data");

        // Huvudfil för alla användarkonton
        public static readonly string UsersFile = Path.Combine(DataDir, "users.json");
    }
}
