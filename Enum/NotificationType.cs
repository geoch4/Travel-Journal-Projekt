using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Travel_Journal
{
    public enum NotificationType
    {
        BudgetExceeded,     // Gått över budgeten - när totalCost > plannedBudget
        BudgetMet,          // Hamnat exakt inom budgeten - när totalCost == plannedBudget
        BudgetUnder,        // Hamnat under budgeten - när totalCost < plannedBudget
    }
}
