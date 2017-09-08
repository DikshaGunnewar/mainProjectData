using EntitiesLayer;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer
{
    public class RegisterService : IRegisterService
    {
        private readonly IUserRepository userRepository;
        private readonly IUnitOfWork unitOfWork;

        public RegisterService(IUserRepository SocialRepository, IUnitOfWork unitOfWork)
        {
            this.userRepository = SocialRepository;
            this.unitOfWork = unitOfWork;
        }

        public IEnumerable<UserDetail> GetAllCustomers()
        {
            return userRepository.GetAll();
        }

        public void AddUser(UserDetail registeruser)
        {
            userRepository.Add(registeruser);
            unitOfWork.Commit();
        }
    }

    public interface IRegisterService
    {
        IEnumerable<UserDetail> GetAllCustomers();
        void AddUser(UserDetail userdetail);
    }
}
