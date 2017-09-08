using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class SuperTargetUser
    {
        public int Id { get; set; }
        [ForeignKey("AccSettings")]
        public int AccSettingId { get; set; }
        public string UserName { get; set; }
        public string SMId { get; set; }
        public int Followers { get; set; }
        public bool IsBlocked { get; set; }
        public AccSettings AccSettings { get; set; }
    }
}
