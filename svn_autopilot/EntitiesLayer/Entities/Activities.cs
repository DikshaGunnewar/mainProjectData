using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class Activities
    {
        public int Id{ get; set; }
        public string Tag { get; set; }
        public int socialId { get; set; }
        public string SMId { get; set; }
        public string PostId { get; set; }
        public string OriginalPostId { get; set; }
        public string Username { get; set; }
        public string Activity { get; set; }
        public string UserType { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime ActivityDate { get; set; }
    }
}
