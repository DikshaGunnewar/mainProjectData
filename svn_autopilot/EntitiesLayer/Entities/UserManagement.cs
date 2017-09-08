using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class UserManagement
    {
        public int Id { get; set; }
        [ForeignKey("AccSettings")]
        public int AccSettingId { get; set; }
        public string userId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public AccSettings AccSettings { get; set; }
    }
}
