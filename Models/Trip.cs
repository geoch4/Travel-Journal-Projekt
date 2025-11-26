using System;
using System.Collections.Generic;

namespace Travel_Journal.Models
{
    // Modell för en resa i resejournalen
    // Innehåller information som land, stad, kostnad, datum, antal personer, betyg, kommentar och budget
    // Inkluderar även hjälpegenskaper för att avgöra om resan är kommande eller avslutad
    public class Trip
    {
       
        public string Country { get; set; }                // Land
        public string City { get; set; }                   // Stad
        public decimal Cost { get; set; }                  // Faktisk kostnad
        public DateTime StartDate { get; set; }            // Startdatum för resan
        public DateTime EndDate { get; set; }              // Slutdatum för resan
        public int NumberOfPassengers { get; set; }        // Antal personer
        public int Score { get; set; }                     // Betyg 1–5
        public string Review { get; set; }                 // Kommentar eller minne
        public decimal PlannedBudget { get; set; }         // Planerad budget


        // Detta för att avgöra om resan är kommande eller avslutad
        // Hjälpegenskaper
        public bool IsUpcoming => StartDate > DateTime.Now;
        public bool IsCompleted => EndDate < DateTime.Now;
    }
}
