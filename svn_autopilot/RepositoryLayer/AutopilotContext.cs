using EntitiesLayer.Entities;
//using EntitiesLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer
{
    public class AutopilotContext:DbContext
    {
        public AutopilotContext():base("Autopilot")
        {}

        #region Entities
        public IDbSet<BusinessCategory> BusinessCategory { get; set; }
        public IDbSet<SocialMedia> SocialMedia { get; set; }
        public IDbSet<AccessDetails> AccessDetails { get; set; }
        public IDbSet<AccSettings> AccSettings { get; set; }
        public IDbSet<Tags> Tags { get; set; }
        public IDbSet<SuperTargetUser> SuperTargetUser { get; set; }
        public IDbSet<Languages> Language { get; set; }
        public IDbSet<Activities> Activities { get; set; }
        public IDbSet<Conversions> Conversions { get; set; }
        public IDbSet<UserManagement> UserManagement { get; set; }
        public IDbSet<TargetPlaylist> TargetPlaylist { get; set; }
        public IDbSet<FollowersGraph> FollowersGraph { get; set; }
        public IDbSet<PinterestScheduledPin> PinterestScheduledPin { get; set; }
        public IDbSet<PinterestFollowingBoardMapping> PinterestFollowingBoardMapping { get; set; }
        public IDbSet<SubscriptionsPlan> SubscriptionsPlan { get; set; }
        public IDbSet<SuggestedTracks> SuggestedTracks { get; set; }
        public IDbSet<UserBillingAddress> UserBillingAddress { get; set; }
        public IDbSet<UserAccountSubscription> UserAccountSubscription { get; set; }
        public IDbSet<OrderDetails> OrderDetails { get; set; }
        public IDbSet<InstagramLocations> InstagramLocations { get; set; }

        #endregion
        public virtual void Commit()
        {
            base.SaveChanges();
        }
    }
}
