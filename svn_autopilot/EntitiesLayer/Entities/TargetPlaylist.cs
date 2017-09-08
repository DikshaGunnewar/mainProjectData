using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class TargetPlaylist
    {
        public int Id { get; set; }
        [ForeignKey("AccSettings")]
        public int AccSettingId { get; set; }
        public string OwnerId { get; set; }
        public string PlaylistId { get; set; }
        public int TracksCount { get; set; }
        public string Name { get; set; }
        public int count { get; set; }
        public AccSettings AccSettings { get; set; }    
    }
}
