using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Repositories;
using ServiceLayer.EnumStore;
using ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class AdminServices : IAdminServices
    {
        private readonly IRepository<SocialMedia> _socialMediaRepo;
        private readonly IRepository<SubscriptionsPlan> _subscriptionPlanRepo;
        private readonly IUserService _userServices;
        private readonly IRepository<UserAccountSubscription> _userAccountSubscription;
        private readonly IRepository<UserBillingAddress> _userBillingAddressRepo;

        private readonly IUnitOfWork _unitOfWork;

        public AdminServices(IRepository<SocialMedia> socialMediaRepo, IUserService userServices, IRepository<SubscriptionsPlan> subscriptionPlanRepo,
            IUnitOfWork unitOfWork, IRepository<UserAccountSubscription> userAccountSubscription, IRepository<UserBillingAddress> userBillingAddressRepo)
        {
            _socialMediaRepo = socialMediaRepo;
            _subscriptionPlanRepo = subscriptionPlanRepo;
            _userServices = userServices;
            _userBillingAddressRepo = userBillingAddressRepo;
            _userAccountSubscription = userAccountSubscription;
            _unitOfWork = unitOfWork;
        }


        public List<ConversionStats> SocialAccountsStats()
        {
            try
            {
                List<ConversionStats> performance = new List<ConversionStats>();

                foreach (SocialMediaProviders item in Enum.GetValues(typeof(SocialMediaProviders)))
                {
                    var result = _userServices.GetAllAccounts().Where(x=>x.Provider == item.ToString()).ToList();
                    performance.Add(new ConversionStats { label = item.ToString(), count = result.Count() });
                }

                return performance;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<ConversionStats> CalculateLastWeekAddedAccount(List<ApplicationUserVM> userList)
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-6);
                var today = DateTime.UtcNow;

                //var result = _conversionRepo.Get().Where(x => x.socialId == socialId && x.ConvertDate > fromDate && x.ConvertDate < today).ToList();
                var result = userList.Where(x => x.RegisterationDate > fromDate && x.RegisterationDate < today).ToList();
                
                List<ConversionStats> stats = new List<ConversionStats>();
                if (result != null)
                {
                    for (int i = -6; i < 0; i++)
                    {
                        var convertDate = DateTime.UtcNow.AddDays(i);
                        int convertCount = 0;
                        foreach (var item in result)
                        {
                            if (item.RegisterationDate.ToShortDateString() == convertDate.ToShortDateString())
                            {
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

        public bool AddSubscriptionsPlan(SubscriptionsPlan plan)
        {
            try
            {
                _subscriptionPlanRepo.Add(plan);
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateSubscriptionsPlan(SubscriptionsPlan plan)
        {
            try
            {
                _subscriptionPlanRepo.Update(plan);
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RemoveSubscriptionPlan(int planId) {
            try
            {
                var plan = _subscriptionPlanRepo.Get().Where(x=>x.Id==planId).FirstOrDefault();
                plan.IsDeleted = true;
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<ApplicationUserVM> UserHistory(List<ApplicationUserVM> users)
        {
            try
            {
                foreach (var item in users)
                {
                    var accounts = _socialMediaRepo.Get().Where(x => x.UserId == item.UserId).ToList();
                        
                    foreach (var acc in accounts)
	                        {
                                var subscriptionDetails = _userAccountSubscription.Get().ToList()
                                    .Join(_subscriptionPlanRepo.Get().ToList(), uas => uas.PlanId, sp => sp.Id, (uas, sp) => new { uas, sp })
                                     .Where(x => x.uas.socialIds == acc.Id.ToString() && x.uas.IsExpire == false)
                                    .Select(m => new { m.sp.Title ,m.sp.NoOfAccounts,m.uas.ExpiresOn}).FirstOrDefault();
                                if (subscriptionDetails != null)
                                {
                                    item.Accounts.Add(new socialAccountsSubscriptionVM
                                    {
                                        ExpiresOn = subscriptionDetails.ExpiresOn.ToLongDateString(),
                                        SubscriptionType = subscriptionDetails.Title,
                                        Provider = acc.Provider,
                                        UserName = acc.UserName
                                    });
                                }
                    }
                    
                   var billingAddress = _userBillingAddressRepo.Get().Where(x=>x.UserId == item.UserId).FirstOrDefault();
                   if (billingAddress != null) {
                       item.Address = billingAddress.Address + " " + billingAddress.PostalCode + " " + billingAddress.Country;                   
                   }
                }

                return users;
            }
            catch (Exception)
            {
                return users;
            }
        }
    }
}
