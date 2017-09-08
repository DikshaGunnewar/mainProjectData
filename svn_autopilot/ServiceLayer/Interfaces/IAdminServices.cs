using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Interfaces
{
    public interface IAdminServices
    {
        List<ConversionStats> SocialAccountsStats();
        List<ConversionStats> CalculateLastWeekAddedAccount(List<ApplicationUserVM> userList);
        bool AddSubscriptionsPlan(SubscriptionsPlan plan);
        bool RemoveSubscriptionPlan(int planId);
        bool UpdateSubscriptionsPlan(SubscriptionsPlan plan);
        List<ApplicationUserVM> UserHistory(List<ApplicationUserVM> users);
    }
}
