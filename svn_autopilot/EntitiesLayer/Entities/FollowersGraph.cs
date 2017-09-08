using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class FollowersGraph
    {
        public int Id { get; set; }
        public int SocialId { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime Date { get; set; }
        public int Followers{ get; set; }
    
    }
}
