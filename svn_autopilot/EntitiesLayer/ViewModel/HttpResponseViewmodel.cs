using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesLayer.ViewModel
{
   public class HttpResponseViewmodel
    {
        public long Id { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Content { get; set; }
        public string SubscriptionId { get; set; }
        public string link { get; set; }
    }
}
