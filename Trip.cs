using System;
using System.Collections.Generic;

namespace Travel_Journal
{
    public class Trip
    {
        public Guid Id { get; set; } = Guid.NewGuid();     // Unikt ID för varje resa
        public string Country { get; set; }                // Land
        public string City { get; set; }                   // Stad
        public decimal Cost { get; set; }                  // Faktisk kostnad
        public string Currency { get; set; }               // Valuta (t.ex. SEK, USD)
        public DateTime StartDate { get; set; }            // Startdatum för resan
        public DateTime EndDate { get; set; }              // Slutdatum för resan
        public int NumberOfPassengers { get; set; }        // Antal personer
        public int Score { get; set; }                     // Betyg 1–5
        public string Review { get; set; }                 // Kommentar eller minne
        public decimal PlannedBudget { get; set; }         // Planerad budget
        public string TimeZone { get; set; }               // Tidszon (t.ex. "Europe/Stockholm")

        public decimal RemainingBudget => PlannedBudget - Cost; // Beräknad egenskap för återstående budget

        // Hjälpegenskaper
        public bool IsUpcoming => StartDate > DateTime.Now;
        public bool IsCompleted => EndDate < DateTime.Now;
    }
}
