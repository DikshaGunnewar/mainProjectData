using EntitiesLayer;
using ServiceLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Autopilot.Controllers
{
    public class UsersController : ApiController
    {
        private readonly IRegisterService userservice;

   
        public UsersController(IRegisterService userRepo)
        {
            this.userservice = userRepo;
        }

        [HttpGet]
        public IEnumerable<UserDetail> Get()
        {
            try
            {
                return userservice.GetAllCustomers();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public void Post(UserDetail registeruser)
        {
            userservice.AddUser(registeruser);
        }
    }
}
