using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Autopilot.Hubs
{
    public class AutopilotHub:Hub
    {
        public void Sendtoall(string message)
        {
            Clients.All.addNewMessageToPage(message);
        }

    }
}