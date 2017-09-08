using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.ViewModel
{
    public class Payment
    {
        public long Id { get; set; }
        public long UserID { get; set; }
        public string UserName{ get; set; }
        public long ContactID { get; set; } 
        public long OrganizationID { get; set; }
        public long PlanId { get; set; }
        public string PaypalPaymentId { get; set; }
    }

    public class SubscriptionPlan
    {
        public long ID { get; set; }
        public string PlanName { get; set; }
        public decimal PlanAmount { get; set; }
        public string TimePeriod { get; set; }
        public string Services { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<bool> IsSelected { get; set; }
        public Nullable<bool> IsTrial { get; set; }
        public Nullable<bool> newUser { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public Nullable<long> DeletedBy { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
    }

    public class Plan
    {
        public string id { get; set; }
        public int amount { get; set; }
        public int created { get; set; }
        public string currency { get; set; }
        public string interval { get; set; }
        public int interval_count { get; set; }
        public string name { get; set; }
        public object statement_descriptor { get; set; }
        public object trial_period_days { get; set; }
        public string url { get; set; }
    }

}

