using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class OrderDetails
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime Date { get; set; }
        public string SubscriptionPlanId { get; set; }
        public string Amount { get; set; }
        public string TransactionId{ get; set; }
        public string PaymentMethod { get; set; }
        public string InvoiceId { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string SocialId { get; set; }

    }
}
