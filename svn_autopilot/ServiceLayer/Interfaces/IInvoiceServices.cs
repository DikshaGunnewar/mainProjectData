using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesLayer.ViewModel;

namespace ServiceLayer.Interfaces
{
   public interface IInvoiceServices
    {
        ContactPerson GetContactPersonByInvoiceID(long InvoiceIdint, int GateWayType);
    }
}
