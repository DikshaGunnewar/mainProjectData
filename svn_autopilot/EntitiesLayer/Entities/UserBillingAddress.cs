using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class UserBillingAddress
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string TaxId { get; set; }
    }
}
