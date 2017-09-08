using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.Entities
{
    public class AccessDetails
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
        [Column(TypeName="DateTime2")]
        public DateTime UpdateDateTime { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime Expires_in { get; set; }
        public string Refresh_token { get; set; }
    }
}
