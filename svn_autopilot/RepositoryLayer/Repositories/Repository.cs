
using RepositoryLayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Repositories
{
    public class Repository<T> : IRepository<T> where T : class,new()
    {
        #region Initialize
        private AutopilotContext dbContext;

        protected IDbFactory DbFactory
        {
            get;
            private set;
        }

        protected AutopilotContext DbContext
        {
            get { return dbContext ?? (dbContext = DbFactory.Init()); }
        }
        public Repository(IDbFactory dbFactory)
        {
            DbFactory = dbFactory;
        }
        #endregion
     
        /// <summary>
        /// Generic method to get a list
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> Get()
        {
            return DbContext.Set<T>();
        }

        /// <summary>
        /// Generic method to get by Id
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> Get(string Id)
        {
            return DbContext.Set<T>();
        }

        /// <summary>
        /// generic method to Add Data
        /// </summary>
        /// <param name="entity"></param>
        public void  Add(T entity)
        {
           
                DbContext.Set<T>().Add(entity);
               
        }

        public T AddAndReturn(T entity)
        {

           return DbContext.Set<T>().Add(entity);

        }
        /// <summary>
        /// generic method to Update Data
        /// </summary>
        /// <param name="entity"></param>
        public void Update(T entity)
        {

            DbContext.Entry(entity).State = EntityState.Modified;
         }
        /// <summary>
        /// generic method to  Data
        /// </summary>
        /// <param name="entity"></param>
        public void Remove(T entity)
        {
            DbContext.Set<T>().Remove(entity);

        }

    }
}
