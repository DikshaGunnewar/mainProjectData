using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.ViewModel
{
    public class ContactPerson
    {
        public long ID { get; set; }
        public long ContactID { get; set; }
        public string FirstName { get; set; }
        public Nullable<bool> IsPrimary { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public long? UserID { get; set; }
        public bool IsLogin { get; set; }
        
    }
}
