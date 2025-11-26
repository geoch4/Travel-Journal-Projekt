using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Travel_Journal.UIServices;

namespace Travel_Journal
{
    public static class NotificationUI
    {

        //Visar en budgetrelaterad notis till användaren-presentationsdelen

        //type=Typen av notis
        //plannedBudget=Den planerade budgeten
        //totalCost=Den faktiska kostnaden
        public static void ShowBudgetNotification(NotificationType type, decimal plannedBudget, decimal totalCost)
        {
            string message;// Variabel för att lagra meddelandet som ska visas

            switch (type) // Switch - sats för att hantera olika notifieringstyper
            {
                case NotificationType.BudgetExceeded:
                    decimal exceededBy = totalCost - plannedBudget;
                    message = $"⚠️ Over Budget: You exceeded your budget by **{exceededBy:C}**!";
                    // Antag att UI.Warn visar gul text
                    UI.Warn(message);
                    break;

                case NotificationType.BudgetUnder:
                    decimal underBy = plannedBudget - totalCost;
                    message = $"✅ Under Budget: You saved **{underBy:C}** from your budget!";
                    // Antag att UI.Success visar blå text
                    UI.Info(message);
                    break;

                case NotificationType.BudgetMet:
                    message = $"🎉 On Budget: The total cost matched your budget exactly!";
                    // Antag att UI.Success visar grön text
                    UI.Success(message);
                    break;


            }
        }
    }
}