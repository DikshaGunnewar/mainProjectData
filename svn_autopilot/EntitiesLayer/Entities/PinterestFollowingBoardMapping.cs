using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class PinterestFollowingBoardMapping
    {
        public int Id { get; set; }
        public int SocialId { get; set; }
        public string MyBoardId { get; set; }
        public string FollowingBoardId { get; set; }
        public bool IsDeleted { get; set; }        

    }
}
