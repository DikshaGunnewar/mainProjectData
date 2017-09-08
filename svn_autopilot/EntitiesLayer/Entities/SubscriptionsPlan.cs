using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class SubscriptionsPlan
    {
        public int Id{ get; set; }
        public string Title { get; set; }
        public double Price { get; set; }
        public string BillingFrequency { get; set; }
        public string NoOfAccounts { get; set; }
        public bool LimitedTagService{ get; set; }
        public bool LowSpeedOfInteraction { get; set; }
        public bool AllowSuperTargeting { get; set; }
        public bool AllowNegativeTags { get; set; }
        public string TagLimit { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsTrail { get; set; }
        public string TrailDays { get; set; }
    }
}
