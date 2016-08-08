using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace CcshlRateSheet
{
    public class Repository<T> : IDisposable where T : class
    {
        private bool disposed = false;
        private CcshlEntities context = null;

        protected DbSet<T> DbSet
        {
            get; set;
        }

        public Repository()
        {
            context = new CcshlEntities();
            DbSet = context.Set<T>();
        }

        public Repository(CcshlEntities context)
        {
            this.context = context;
        }
        public List<T> GetAll()
        {
            return DbSet.ToList();
        }

        public T Get(int id)
        {
            return DbSet.Find(id);
        }

        public void Add(T entity)
        {
            DbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            context.Entry<T>(entity).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            DbSet.Remove(DbSet.Find(id));
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                context.Dispose();
                disposed = true;
            }
        }
    }
}
