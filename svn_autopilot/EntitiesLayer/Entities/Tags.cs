using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class Tags
    {
        public int Id { get; set; }
        [ForeignKey("AccSettings")]
        public int AccSettingId { get; set; }
        public string TagName { get; set; }
        public string Location { get; set; }
        public bool IsBlocked { get; set; }
        public AccSettings AccSettings { get; set; }
    }
}
