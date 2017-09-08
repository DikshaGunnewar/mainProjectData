using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Repositories;
using EntitiesLayer.Entities;
using ServiceLayer.Interfaces;
using RepositoryLayer.Infrastructure;
using EntitiesLayer.ViewModel;
using System.Data.Entity;
using ServiceLayer.EnumStore;
using TweetSharp;
using GoogleMaps.LocationServices;

namespace ServiceLayer.Services
{
   public class UserService : IUserService
   {

       #region Initialization
       private readonly IRepository<BusinessCategory> _businessCategory;
       private readonly IRepository<SocialMedia> _socialMediaRepo;
       private readonly IRepository<SubscriptionsPlan> _subscriptionPlanRepo;
       private readonly IRepository<OrderDetails> _orderDetailsRepo;
       private readonly IRepository<UserAccountSubscription> _accountSubscriptionRepo;
       
       private readonly IRepository<Languages> _languageRepo;
       private readonly IRepository<Tags> _tagRepo;
       private readonly IRepository<Activities> _activityRepo;
       private readonly IRepository<Conversions> _conversionRepo;
       private readonly IRepository<SuperTargetUser> _superTargetUserRepo;
       private readonly IRepository<UserManagement> _userManagementRepo;
       private readonly IRepository<FollowersGraph> _followersGraphRepo;
       private readonly IRepository<UserBillingAddress> _userBillingAddressRepo;

       private readonly IUnitOfWork _unitOfWork;


       public UserService(IRepository<BusinessCategory> businessCategory,
           IUnitOfWork unitOfWork, 
           IRepository<SocialMedia> socialMedia,
           IRepository<Languages> languages,
           IRepository<Activities> activityRepo,
           IRepository<Conversions> conversionRepo,
           IRepository<UserManagement> userManagementRepo,
           IRepository<SuperTargetUser> superTargetUserRepo,
           IRepository<Tags> tagsRepo, IRepository<FollowersGraph> followersGraphRepo,
           IRepository<SubscriptionsPlan> subscriptionPlanRepo,
           IRepository<UserBillingAddress> userBillingAddressRepo,
           IRepository<OrderDetails> orderDetailsRepo,
       IRepository<UserAccountSubscription> accountSubscriptionRepo)
       {
           _businessCategory = businessCategory;
           _unitOfWork = unitOfWork;
           _tagRepo = tagsRepo;
           _superTargetUserRepo = superTargetUserRepo;
           _userManagementRepo = userManagementRepo;
           _conversionRepo = conversionRepo;
           _activityRepo = activityRepo;
           _orderDetailsRepo = orderDetailsRepo;
           _accountSubscriptionRepo = accountSubscriptionRepo;
           _userBillingAddressRepo = userBillingAddressRepo;
           _socialMediaRepo = socialMedia;
           _languageRepo = languages;
           _subscriptionPlanRepo = subscriptionPlanRepo;
           _followersGraphRepo = followersGraphRepo;
      }
       #endregion

       /// <summary>
       /// get business category
       /// </summary>
       /// <returns></returns>
       public IEnumerable<BusinessCategory> GetBusinessCategory()
       {
           var categories = _businessCategory.Get().ToList();
           return categories;
       }

       /// <summary>
       /// Get all socail account associated with a user.
       /// </summary>
       /// <param name="userId"></param>
       /// <returns></returns>
       public IEnumerable<SocialMediaVM> GetUsersAllAccounts(string userId) 
       {
           //var accounts = _socialMediaRepo.Get().Where(x => x.UserId == userId).ToList();
           var accounts = _userManagementRepo.Get().ToList()
               .Join(_socialMediaRepo.Get().Include(x=>x.AccSettings).Where(x=>x.IsDeleted ==false).ToList(), um => um.AccSettingId, sm => sm.AccSettings.Id, (um, sm) => new { um, sm })
               .Where(x=>x.um.userId== userId)
               .Select(m => new
               {
                   m.sm.Id,m.sm.SMId,m.sm.Status,m.sm.Followers,m.sm.Provider,m.sm.UserId,m.sm.UserName,m.sm.ProfilepicUrl,m.sm.IsInvalid
               }).ToList();
           List<SocialMediaVM> accountList = new List<SocialMediaVM>();
           foreach (var acc in accounts)
           {
               accountList.Add(new SocialMediaVM { Id = acc.Id, SMId = acc.SMId, Status = acc.Status, Followers = acc.Followers, Provider = acc.Provider, UserId = acc.UserId, UserName = acc.UserName, ProfilepicUrl = acc.ProfilepicUrl,IsInvalid = acc.IsInvalid });
           
           }
           
           return accountList;
       }

       /// <summary>
       /// Getting all the social accounts in the application
       /// </summary>
       /// <returns></returns>
       public IEnumerable<SocialMediaVM> GetAllAccounts() {
           var accounts = _socialMediaRepo.Get().Include(x => x.AccSettings).Include(x => x.AccessDetails).Include(x => x.AccSettings.Tags).Include(x => x.AccSettings.SuperTargetUser).Where(x=>x.IsDeleted== false).ToList();
           var today = DateTime.UtcNow;
           List<SocialMediaVM> accountList = new List<SocialMediaVM>();
           foreach (var acc in accounts)
           {
               var subscription = _accountSubscriptionRepo.Get().Where(x => x.socialIds == acc.Id.ToString() && x.ExpiresOn > today).FirstOrDefault();
               
               var accountDetails = new SocialMediaVM()
               {
                   Id = acc.Id,
                   SMId = acc.SMId,
                   Status = acc.Status,
                   Followers = acc.Followers,
                   Provider = acc.Provider,
                   UserId = acc.UserId,
                   UserName = acc.UserName,
                   ProfilepicUrl = acc.ProfilepicUrl,
                   AccessDetails = acc.AccessDetails,
                   IsInvalid = acc.IsInvalid,
                   AccSettings = acc.AccSettings
               };
               if (subscription == null) //checking if account is under subscription or in trail
               {
                   var CheckForTrail = _accountSubscriptionRepo.Get().Where(x => x.userId == acc.UserId && x.ExpiresOn > today && x.IsTrail == true).FirstOrDefault();
                   if (CheckForTrail != null)
                   {
                       accountDetails.IsTrail = true;
                       accountDetails.IsSubscribed = false;
                   }
               }
               else {
                   accountDetails.IsTrail = false;
                   accountDetails.IsSubscribed = true;
                   accountDetails.planId = subscription.PlanId.ToString();
               }
               accountList.Add(accountDetails);
           }

           return accountList;
       }

       /// <summary>
       /// Get a particular account by Id
       /// </summary>
       /// <param name="socialId"></param>
       /// <returns></returns>
       public SocialMediaVM GetAccount(int socialId)
       {
           SocialMediaVM account = new SocialMediaVM();
           var today = DateTime.UtcNow;           
           var acc = _socialMediaRepo.Get().Where(x => x.Id == socialId && x.IsDeleted == false).Include(x => x.AccSettings).Include(x => x.AccessDetails).Include(x => x.AccSettings.Tags).Include(x => x.AccSettings.UserManagement).FirstOrDefault();
           var subscription = _accountSubscriptionRepo.Get().ToList()
                                       .Join(_subscriptionPlanRepo.Get().ToList(), asr => asr.PlanId, sp => sp.Id, (asr, sp) => new { asr, sp })
                                       .Where(x => x.asr.socialIds == acc.Id.ToString() && x.asr.ExpiresOn > today)
                                       .Select(m => new
                                       {
                                           m.asr.ExpiresOn,
                                           m.asr.Id,
                                           m.asr.IsTrail,
                                           m.asr.PlanId,
                                           m.sp.Title
                                       }).FirstOrDefault();  //account subscription details
           account.Id = acc.Id;
           account.SMId = acc.SMId;
           account.Status = acc.Status;
           account.Followers = acc.Followers;
           account.Provider = acc.Provider;
           account.UserId = acc.UserId;
           account.IsInvalid = acc.IsInvalid;
           account.UserName = acc.UserName;
           account.ProfilepicUrl = acc.ProfilepicUrl;
           account.AccessDetails = acc.AccessDetails;
           if (acc.AccSettings == null)
           {

               account.AccSettings = new AccSettings();
           }
           else
           {
               account.AccSettings = acc.AccSettings;
           }
           if (subscription == null) //checking is subscribed or is in trial
           {
               var CheckForTrail = _accountSubscriptionRepo.Get().Where(x => x.userId == acc.UserId && x.ExpiresOn > today && x.IsTrail == true).FirstOrDefault();
               if (CheckForTrail != null)
               {
                   account.ExpireOn = CheckForTrail.ExpiresOn;
                   account.IsTrail = true;
                   account.IsSubscribed = false;
               }
           }
           else
           {
               account.IsTrail = false;
               account.IsSubscribed = true;
               account.ExpireOn = subscription.ExpiresOn;
               account.planId = subscription.PlanId.ToString();
               account.planName = subscription.Title;
           }
           return account;
       }

       /// <summary>
       /// Get application user's profile
       /// </summary>
       /// <param name="userId"></param>
       /// <returns></returns>
       public UserProfileVM GetUserprofile(string userId){
           try
           {
               UserProfileVM profile = new UserProfileVM();
               var accounts = GetUsersAllAccounts(userId);
               var billingAddress = _userBillingAddressRepo.Get().Where(x => x.UserId == userId).FirstOrDefault();
               profile.UserId = userId;
               profile.UserAccounts = accounts.ToList();
               if (billingAddress != null)
               {
                   profile.BillingAddress = billingAddress;               
               }
               else {
                   profile.BillingAddress = new UserBillingAddress();
               }
               return profile;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       /// <summary>
       /// update user's billing address / profile
       /// </summary>
       /// <param name="billAddress"></param>
       /// <returns></returns>
       public bool UpdateProfile(UserBillingAddress billAddress)
       {
           try
           {
               if (billAddress.Id == 0)
               {
                   _userBillingAddressRepo.Add(billAddress);
               }
               else {
                   _userBillingAddressRepo.Update(billAddress);
               }
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {

               return false;
           }
       }

       /// <summary>
       /// Save payment/transaction details
       /// </summary>
       /// <param name="transactioDetails"></param>
       /// <returns></returns>
       public bool SaveTransactionDetails(PaymentViewModel transactioDetails) {
           try
           {
              
               OrderDetails order = new OrderDetails(){
                   Amount = transactioDetails.Amount,
                   Date= DateTime.UtcNow,
                   PaymentMethod="Paypal",
                   SubscriptionPlanId = transactioDetails.planId,
                   TransactionId= transactioDetails.TransactionId,
                   UserId = transactioDetails.userId,
                   Status = transactioDetails.Status,
                   Currency=transactioDetails.Currency,
                   InvoiceId=transactioDetails.InvoiceId,
                   SocialId = transactioDetails.socialIds
               };   //preparing order details object
               var addedOrder = _orderDetailsRepo.AddAndReturn(order); //add order details
              
               var pId = Int16.Parse(transactioDetails.planId);
               var planDetails = _subscriptionPlanRepo.Get().Where(x => x.Id == pId).FirstOrDefault();
               DateTime planExpiresDate = transactioDetails.Date;
               if (planDetails.BillingFrequency == "Monthly") { planExpiresDate = transactioDetails.Date.AddDays(30); }
               else if (planDetails.BillingFrequency == "Yearly") { planExpiresDate = transactioDetails.Date.AddDays(365); }
               if (transactioDetails.Status.ToLower() == "approved") // transcation is approved/success then apply subscription for user account
               {
                   foreach (var item in transactioDetails.socialIds.Split(','))
                   {

                       ApplyUserSubscription(new UserAccountSubscription { socialIds = item, userId = transactioDetails.userId, ExpiresOn = planExpiresDate, IsTrail = false, PlanId = Int16.Parse(transactioDetails.planId), OrderId = addedOrder.Id.ToString() });
                   }
               }
              
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;

           }
       }

       /// <summary>
       /// Apply/Add subscription for particular social account
       /// </summary>
       /// <param name="accSubscriptionDetails"></param>
       public void ApplyUserSubscription(UserAccountSubscription accSubscriptionDetails) {
           try
           {
               _accountSubscriptionRepo.Add(accSubscriptionDetails);
               _unitOfWork.Commit();
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       /// <summary>
       /// check if any subscription is available for a user
       /// </summary>
       /// <param name="socialId"></param>
       /// <returns></returns>
       public UserAccountSubscription CheckUserSubscription(string socialId)
       {
           try
           {
               var today = DateTime.UtcNow;
               var check =  _accountSubscriptionRepo.Get().Where(x => x.socialIds == socialId && x.ExpiresOn > today).FirstOrDefault();
               return check;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       /// <summary>
       /// Get language List
       /// </summary>
       /// <returns></returns>
       public List<Languages> GetLanguages() 
       {
           try
           {
               var languages = _languageRepo.Get().ToList();
               return languages;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       /// <summary>
       /// update Language perferences
       /// </summary>
       /// <param name="languageIds"></param>
       /// <param name="socialId"></param>
       /// <returns></returns>
       public bool UpdateLanguage(string languageIds, int socialId) 
       {
           try
           {    
               var result = _socialMediaRepo.Get().Include(x=>x.AccSettings).Where(x=>x.Id == socialId).FirstOrDefault() ;
               result.AccSettings.Language = languageIds;
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;
           }
       }

       /// <summary>
       /// Return language code associated with langId
       /// </summary>
       /// <param name="langId"></param>
       /// <returns></returns>
       public string ReturnLanguageCode(string langId) {
           try
           {
               var Id = Int32.Parse(langId);
               var langCode=_languageRepo.Get().Where(x => x.Id == Id).FirstOrDefault().LangCode;
               return langCode;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       /// <summary>
       /// Get Token for User social account
       /// </summary>
       /// <param name="socialId"></param>
       /// <returns></returns>
       public AccessDetails GetTokenForUser(int socialId)
       {
           try
           {
               var accessToken = _socialMediaRepo.Get().Include(x => x.AccessDetails).Where(x => x.Id == socialId).Select(x => x.AccessDetails).FirstOrDefault();
               return accessToken;
           }
           catch (Exception)
           {

               throw;
           }
       }

       /// <summary>
       /// Add/block tags
       /// </summary>
       /// <param name="tag"></param>
       /// <param name="socailId"></param>
       /// <param name="IsBlocked"></param>
       /// <returns></returns>
       public bool AddBlockTag(string tag, int socailId, bool IsBlocked)
       {
           try
           {
               var acc = _socialMediaRepo.Get().Include(x => x.AccSettings).Include(x=>x.AccSettings.Tags).Where(x => x.Id == socailId).FirstOrDefault();
               if (acc.AccSettings == null)
               {
                   acc.AccSettings = new AccSettings();
               }
               var check = acc.AccSettings.Tags.Where(x => x.TagName.ToLower() == tag.ToLower()).FirstOrDefault();
               if (check == null)
               {
                   acc.AccSettings.Tags.Add(new Tags { IsBlocked = IsBlocked, TagName = tag });
                   _unitOfWork.Commit();
                   return true;
               }
               else {
                   return false;
               }
               
           }
           catch (Exception)
           {
               return false;
           }

       }

       /// <summary>
       /// Remove Tags
       /// </summary>
       /// <param name="tagId"></param>
       /// <returns></returns>
       public bool RemoveTags(int tagId)
       {
           try
           {
               var tagItem = _tagRepo.Get().Where(x => x.Id == tagId).FirstOrDefault();
               _tagRepo.Remove(tagItem);
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {

               return false;
           }
       }

       /// <summary>
       /// Get all tags (blocked as well as unblocked)
       /// </summary>
       /// <param name="socialId"></param>
       /// <returns></returns>
       public List<TagsVM> GetAllTags(int socialId)
       {
           try
           {
               var acc = _socialMediaRepo.Get().Include(x => x.AccSettings).Include(x => x.AccSettings.Tags).Where(x => x.Id == socialId).FirstOrDefault();
               List<TagsVM> tagList = new List<TagsVM>();
               if (acc.AccSettings != null)
               {
                   foreach (var item in acc.AccSettings.Tags)
                   {
                       tagList.Add(new TagsVM { AccSettingId = item.AccSettingId, Id = item.Id, IsBlocked = item.IsBlocked, TagName = item.TagName, Location = item.Location });
                   }
               }
               
               return tagList;
           }
           catch (Exception)
           {

               throw;
           }
       }

       /// <summary>
       /// Save User activity (Common methods to all accounts)
       /// </summary>
       /// <param name="activity"></param>
       /// <returns></returns>
       public bool SaveActivity(Activities activity)
       {
           try
           {
               _activityRepo.Add(activity);
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;
              
           }
       }

       /// <summary>
       /// Save social account conversion
       /// </summary>
       /// <param name="conversion"></param>
       /// <returns></returns>
       public bool SaveConversion(Conversions conversion)
       {
           try
           {
               _conversionRepo.Add(conversion);
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;

           }
       }

       /// <summary>
       /// Method to get coordinated(lang,lat) by location
       /// </summary>
       /// <param name="location"></param>
       /// <returns></returns>
       public Coordinates AddressToCoordinates(string location)
       {
           try
           {
               var locationService = new GoogleLocationService();
               var point = locationService.GetLatLongFromAddress(location);
               Coordinates coordinate = new Coordinates();
               coordinate.latitude = point.Latitude;
               coordinate.longitude = point.Longitude;
               return coordinate;
           }
           catch (Exception)
           {
               
               throw;
           }
          
       
       }

       /// <summary>
       /// Add User to a social account as editor (User management)
       /// </summary>
       /// <param name="socialId"></param>
       /// <param name="email"></param>
       /// <param name="userId"></param>
       /// <returns></returns>
       public bool AddUserToAccount(int socialId, string email, string userId)
       {
           try
           {
               var acc = _socialMediaRepo.Get().Include(x=>x.AccSettings).Include(x=>x.AccSettings.UserManagement).Where(x => x.Id == socialId).FirstOrDefault();
               if (acc.AccSettings.UserManagement.Where(x => x.Email == email).FirstOrDefault() == null)
               {
                   acc.AccSettings.UserManagement.Add(new UserManagement { Email = email, Role = "Editor", userId = userId });
                   _unitOfWork.Commit();
                   return true;
               }
               else {
                   return false;
               }

           }
           catch (Exception)
           {
               return false;
           }
       }

       /// <summary>
       /// Remove user from a social account (User management)
       /// </summary>
       /// <param name="userManagementId"></param>
       /// <returns></returns>
       public bool RemoveUser(int userManagementId) {
           try
           {
               _userManagementRepo.Remove(_userManagementRepo.Get().Where(x => x.Id == userManagementId).FirstOrDefault());
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;
           }
       }

       /// <summary>
       /// Check if a user is already convert
       /// </summary>
       /// <param name="SMId"></param>
       /// <param name="socialId"></param>
       /// <returns></returns>
       public bool CheckIfUserConvert(string SMId, int socialId) {
           try
           {
              var result = _conversionRepo.Get().Where(x => x.SMId == SMId && x.socialId == socialId).FirstOrDefault();
              if (result == null)
              {
                  return false;
              }
              else {
                  return true;
              }
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       /// <summary>
       /// Calculate conversion stats & return stats for conversion graph
       /// </summary>
       /// <param name="socialId"></param>
       /// <returns></returns>
       public List<ConversionStats> CalculateConversion(int socialId)
       {
           try
           {
               var fromDate = DateTime.UtcNow.AddDays(-6);
               var today = DateTime.UtcNow;
               var result = _conversionRepo.Get().Where(x => x.socialId == socialId && x.ConvertDate > fromDate  && x.ConvertDate < today).ToList();
               List<ConversionStats> stats = new List<ConversionStats>();
               if(result != null){
                   for (int i = -6; i < 0; i++)
                   {
                       var convertDate = DateTime.UtcNow.AddDays(i);
                       int convertCount = 0;
                       foreach (var item in result)
                       {
                           if (item.ConvertDate.ToShortDateString() == convertDate.ToShortDateString()) {
                               convertCount++;
                           }
                       }
                       stats.Add(new ConversionStats { label = convertDate.ToShortDateString(), count = convertCount });
                   }
                }
               return stats;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       /// <summary>
       /// Count conversion for social accounts stats like todays , week & total conversion
       /// </summary>
       /// <param name="socialId"></param>
       /// <returns></returns>
       public ConversionCount CountConversion(int socialId) {
           try
           {
               ConversionCount count = new ConversionCount();
               var accConversion = _conversionRepo.Get().Where(x => x.socialId == socialId).ToList();
               count.totalConversion = accConversion.Count();
               var today = DateTime.UtcNow.AddHours(-23);
               var weekInterval = DateTime.UtcNow.AddDays(-7);
               count.todayConversion = accConversion.Where(x => x.ConvertDate > today).Count();
               count.weekConversion = accConversion.Where(x => x.ConvertDate > weekInterval).Count();
               return count;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       /// <summary>
       /// Delete a social account (soft-delete)
       /// </summary>
       /// <param name="socialId"></param>
       /// <returns></returns>
       public bool DeleteAccount(int socialId) {
           try
           {
               var acc = _socialMediaRepo.Get().Where(x => x.Id == socialId).FirstOrDefault();
               acc.IsDeleted = true;
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {

               return false;
           }
       }

       /// <summary>
       /// Get list of user convert
       /// </summary>
       /// <param name="socailId"></param>
       /// <returns></returns>
       public List<ConversionsVM> GetConversion(int socailId)
       {
           try
           {
               List<ConversionsVM> list = new List<ConversionsVM>();        
                var conversion =_conversionRepo.Get().Where(x=>x.socialId==socailId).ToList();
                foreach (var item in conversion)
                {
                    list.Add(new ConversionsVM { ConvertDate = item.ConvertDate.ToString(), Id = item.Id, socialId = item.socialId, Tag = item.Tag, SMId = item.SMId, Username = item.Username, ProfilePic = GetAccount(item.socialId).ProfilepicUrl });
                } 
               
               return list;
           }
            catch (Exception)
            {
		
	            throw;
            }
       }
        
       /// <summary>
       /// Get tag performance stats
       /// </summary>
       /// <param name="socialId"></param>
       /// <returns></returns>
       public List<ConversionStats> TagPerformance(int socialId)
       {
           try
           {

               var result = _conversionRepo.Get().Where(x => x.socialId == socialId).ToList(); //convert user for a social account
               List<ConversionStats> performance = new List<ConversionStats>();
              
               if (result != null)
               {
                   foreach (var item in result)
                   {
                       //int count = 1;
                       var conversion = performance.Where(x => x.label == item.Tag).FirstOrDefault(); 
                       if (conversion == null)
                       {
                           performance.Add(new ConversionStats { label = item.Tag, count = 1 });
                       }
                       else {
                           conversion.count = conversion.count + 1;
                       }
                   }

                   //check for hashtags performance
                   var account = _socialMediaRepo.Get().Include(x => x.AccSettings.Tags).Include(x=>x.AccSettings.SuperTargetUser).Where(x => x.Id == socialId).FirstOrDefault();
                   foreach (var res in result)
                   {
                       foreach (var item in account.AccSettings.Tags)
                       {
                           if (res.Tag.Substring(1) != item.TagName) {
                               performance.Add(new ConversionStats { label = '#'+item.TagName, count = 0 });
                            
                           }
                       }
                   }
                   //check for super target user performance
                   foreach (var res in result)
                   {
                       foreach (var item in account.AccSettings.SuperTargetUser)
                       {
                           if (res.Tag.Substring(1) != item.UserName)
                           {
                               performance.Add(new ConversionStats { label = '@' + item.UserName, count = 0 });

                           }
                       }
                   }

               }
               return performance;
           }
           catch (Exception)
           {
               throw;
           }
       }

       /// <summary>
       /// Get stats fro followers graph
       /// </summary>
       /// <param name="socialId"></param>
       /// <returns></returns>
       public List<ConversionStats> GetFollowersGrowth(int socialId) {
           try
           {
               var fromDate = DateTime.UtcNow.AddDays(-6);
               var today = DateTime.UtcNow;
               var result =_followersGraphRepo.Get().Where(x => x.SocialId == socialId && x.Date > fromDate && x.Date < today).ToList();
               //var result = _conversionRepo.Get().Where(x => x.socialId == socialId && x.ConvertDate > fromDate && x.ConvertDate < today).ToList();
               List<ConversionStats> stats = new List<ConversionStats>();
               if (result.Count() >0)
               {
                   for (int i = -6; i < 0; i++)
                   {
                       var updatedDate = DateTime.UtcNow.AddDays(i);
                       //int convertCount = 0;
                       foreach (var item in result)
                       {
                           if (item.Date.ToShortDateString() == updatedDate.ToShortDateString())
                           {
                               stats.Add(new ConversionStats { label = updatedDate.ToShortDateString(), count = item.Followers });
                           }
                       }
                       if (stats.Where(x => x.label == updatedDate.ToShortDateString()).FirstOrDefault() == null) { 
                               stats.Add(new ConversionStats { label = updatedDate.ToShortDateString(), count = 0 });
                           
                       }
                   }
               }
               else {
                   for (int i = -6; i < 0; i++)
                   {
                       var updatedDate = DateTime.UtcNow.AddDays(i);
                        stats.Add(new ConversionStats { label = updatedDate.ToShortDateString(), count = 0 });

                   }
               }
               return stats;
           }
           catch (Exception)
           {

               throw;
           }
       }

       /// <summary>
       /// Save followers count for today
       /// </summary>
       /// <param name="socialId"></param>
       /// <param name="followerCount"></param>
       /// <returns></returns>
       public bool SaveFollowersCount(int socialId, int followerCount)
       {
           try
           {
               _followersGraphRepo.Add(new FollowersGraph { Date = DateTime.UtcNow, Followers = followerCount, SocialId = socialId });
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;
           }
       }

       /// <summary>
       /// Get all subscription plans
       /// </summary>
       /// <returns></returns>
       public List<SubscriptionsPlan> GetAllSubscriptionPlan()
       {
           try
           {
               var plans = _subscriptionPlanRepo.Get().Where(x => x.IsDeleted == false).ToList();
               return plans;
           }
           catch (Exception)
           {

               throw;
           }
       }

       /// <summary>
       /// Get a particular subscription plan
       /// </summary>
       /// <param name="PlanId"></param>
       /// <returns></returns>
       public SubscriptionsPlan GetASubscriptionPlan(string PlanId)
       {
           try
           {
               var pId = Int16.Parse(PlanId);
               var plans = _subscriptionPlanRepo.Get().Where(x => x.IsDeleted == false && x.Id == pId).FirstOrDefault();
               return plans;
           }
           catch (Exception)
           {

               throw;
           }
       }

       /// <summary>
       /// Get all the order for a user account
       /// </summary>
       /// <param name="userId"></param>
       /// <returns></returns>
       public List<UserOrderVM> GetUserOrders(string userId)
       {
           try
           {
               List<UserOrderVM> orderList = new List<UserOrderVM>();
               var userOrders = _orderDetailsRepo.Get().ToList()
                   .Join(_subscriptionPlanRepo.Get().ToList(), od => od.SubscriptionPlanId, sp => sp.Id.ToString(), (od, sp) => new { od, sp })
                   .Where(x => x.od.UserId == userId).OrderByDescending(x => x.od.Id).Select(m => new { 
                   m.od.Id,
                   m.od.PaymentMethod,
                   m.od.InvoiceId,
                   m.od.Status,
                   m.od.SubscriptionPlanId,
                   m.od.TransactionId,
                   m.od.UserId,m.od.Amount,
                   m.od.Currency,
                   m.od.Date,m.sp.Title,
                   m.sp.BillingFrequency,
                   m.od.SocialId
                   }).ToList(); 
               foreach (var item in userOrders)
               {
                   List<string> AccountDetails = new List<string>();
                   foreach (var Id in item.SocialId.Split(','))
                   {
                       var id = Int16.Parse(Id);
                       var acc =_socialMediaRepo.Get().Where(x => x.Id == id).FirstOrDefault();
                       AccountDetails.Add(acc.UserName + " (" + acc.Provider + ")");
                   } 
                   
                   orderList.Add(new UserOrderVM
                   {
                       Id = item.Id,
                       Date = item.Date,
                       Amount = item.Amount,
                       Currency = item.Currency,
                       InvoiceId = item.InvoiceId,
                       Status = item.Status,
                       PaymentMethod = item.PaymentMethod,
                       SubscriptionPlanId = item.SubscriptionPlanId,
                       TransactionId = item.TransactionId,
                       UserId = item.UserId,
                       BillingFrequency = item.BillingFrequency,
                       AccountDetails = AccountDetails,PlanName = item.Title
                   });
               }
               return orderList;
           }
           catch (Exception)
           {
               
               throw;
           }
       }

       /// <summary>
       /// Medthod stop service of a account by user.
       /// </summary>
       /// <param name="socialId"></param>
       public bool ChangeServiceStatus(int socialId) {
           try
           {
               var acc = _socialMediaRepo.Get().Where(x => x.Id == socialId).FirstOrDefault();
               if (acc.Status == true)
               {
                   acc.Status = false;
               }
               else {
                   acc.Status = true;
               }
               _unitOfWork.Commit();
               return true;
           }
           catch (Exception)
           {
               return false;
           }
       }

   }
}
