using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Spectre.Console;
using Travel_Journal.UIServices;

namespace Travel_Journal.Data
{
    // En generisk klass som kan läsa/spara valfri lista av objekt till JSON.
    public class DataStore<T>
    {
        // Filsökväg för den specifika datatypen, t.ex. "data/andre_trips.json"
        private readonly string _filePath;

        public DataStore(string fileName)
        {
            // Säkerställ att datamappen finns
            Directory.CreateDirectory(Paths.DataDir);

            // Bygg hela filsökvägen, t.ex. "data/andre_trips.json"
            _filePath = Path.Combine(Paths.DataDir, fileName);
        }

        // Spara listan till JSON-fil
        public void Save(List<T> items)
        {
            try
            {
                var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                UI.Error($"Failed to save data: {ex.Message}");
                Logg.Log($"Error saving data to {_filePath}: {ex}");
            }
        }

        // Läser in listan från JSON-fil med felhantering
        public List<T> Load()
        {
            if (!File.Exists(_filePath))
                return new List<T>();

            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
            catch (Exception ex)
            {
                UI.Error($"Failed to load data: {ex.Message}");
                Logg.Log($"Error loading data from {_filePath}: {ex}");
                return new List<T>();
            }
        }
    }
}
