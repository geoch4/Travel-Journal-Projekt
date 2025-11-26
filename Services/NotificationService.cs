using Travel_Journal; // Add this if NotificationType is defined in Models namespace

namespace Travel_Journal.Services
{

    public class NotificationService
    {
        /// Bedömer budgetutfallet för en resa-LOGIKdelen

        //plannedBudget = Den planerade budgeten
        //totalCost=Den faktiska kostnaden

        public NotificationType AssessBudgetOutcome(decimal plannedBudget, decimal totalCost)//metoden förhåller sig till enumet
        {
            if (totalCost > plannedBudget) //om kostnaden över planerat
            {
                return NotificationType.BudgetExceeded;
            }
            else if (totalCost < plannedBudget) //om kostnaden är lägre än budgeten
            {
                return NotificationType.BudgetUnder;
            }
            else //om kostnaden är lika med planerat
            {
                return NotificationType.BudgetMet;
            }
        }
    }
}

