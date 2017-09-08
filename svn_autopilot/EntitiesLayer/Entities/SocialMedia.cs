using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class SocialMedia
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Provider { get; set; }
        public bool Status { get; set; }
        public int Followers { get; set; }
        public string SMId { get; set; }
        public string UserName { get; set; }
        public string ProfilepicUrl { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsInvalid { get; set; }
        public AccessDetails AccessDetails{ get; set; }
        public AccSettings AccSettings{ get; set; }
        
        //public UserBillingAddress BillingAddress { get; set; }

    }
}
