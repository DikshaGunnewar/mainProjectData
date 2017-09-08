using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer
{
    public class UserDetail
    {
        [Key]
        public int userId { get; set; }
        public string FirstName { get; set; }
        public string PhoneNo { get; set; }
    }
}
