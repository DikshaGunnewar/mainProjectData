using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class UserAccountSubscription
    {
        public int Id { get; set; }
        public string userId { get; set; }
        public string socialIds { get; set; }
       [Column(TypeName = "DateTime2")]
        public DateTime ExpiresOn { get; set; }
        public int PlanId { get; set; }
        public bool IsTrail { get; set; }
        public bool IsExpire { get; set; }
        public string OrderId { get; set; }

    }
}
