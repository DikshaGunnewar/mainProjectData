using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;

namespace ServiceLayer.Interfaces
{
    public interface IUserService
    {
        IEnumerable<BusinessCategory> GetBusinessCategory();
        IEnumerable<SocialMediaVM> GetUsersAllAccounts(string userId);
        IEnumerable<SocialMediaVM> GetAllAccounts();
        SocialMediaVM GetAccount(int socialId);
        List<Languages> GetLanguages(); 
        bool UpdateLanguage(string languageIds,int socialId);
        string ReturnLanguageCode(string langId);
        AccessDetails GetTokenForUser(int socialId);
        bool AddBlockTag(string tag, int socailId, bool IsBlocked);
        bool RemoveTags(int tagId);
        List<TagsVM> GetAllTags(int socailId);
        bool SaveActivity(Activities activity);
        bool SaveConversion(Conversions conversion);
        Coordinates AddressToCoordinates(string location);
        bool AddUserToAccount(int socialId, string email, string userId);
        bool RemoveUser(int userManagementId);
        bool CheckIfUserConvert(string SMId, int socialId);
        List<ConversionStats> CalculateConversion(int socialId);
        ConversionCount CountConversion(int socialId);
        bool DeleteAccount(int socialId);
        List<ConversionsVM> GetConversion(int socailId);
        List<ConversionStats> TagPerformance(int socialId);
        List<ConversionStats> GetFollowersGrowth(int socialId);
        bool SaveFollowersCount(int socialId, int followerCount);
        List<SubscriptionsPlan> GetAllSubscriptionPlan();
        SubscriptionsPlan GetASubscriptionPlan(string PlanId);
        UserProfileVM GetUserprofile(string userId);
        bool UpdateProfile(UserBillingAddress billAddress);
        bool SaveTransactionDetails(PaymentViewModel transactioDetails);
        void ApplyUserSubscription(UserAccountSubscription accSubscriptionDetails);
        List<UserOrderVM> GetUserOrders(string userId);
        bool ChangeServiceStatus(int socialId);
    }
}
