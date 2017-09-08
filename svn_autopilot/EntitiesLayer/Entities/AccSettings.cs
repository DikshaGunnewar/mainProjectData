using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class AccSettings
    {
        public int Id { get; set; }
        public string Language { get; set; }
        public ICollection<Tags> Tags { get; set; }
        public ICollection<SuperTargetUser> SuperTargetUser { get; set; }
        public ICollection<UserManagement> UserManagement { get; set; }
        public ICollection<TargetPlaylist> TargetPlaylist { get; set; }
        

        public AccSettings()
        {
            Tags = new List<Tags>();
            SuperTargetUser = new List<SuperTargetUser>();
            UserManagement = new List<UserManagement>();
            TargetPlaylist = new List<TargetPlaylist>();
        }
    }
}
