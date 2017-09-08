using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class PinterestScheduledPin
    {
        public int Id { get; set; }
        public int SocialId { get; set; }
        public string note { get; set; }
        public string link { get; set; }
        public string image_url { get; set; }
        public string jobId { get; set; }
        public string board { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime ScheduleDate { get; set; }

    }
}
