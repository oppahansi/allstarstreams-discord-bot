using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected DbContext DbContext { get; set; }

        public RepositoryBase(DbContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<IEnumerable<T>> FindAllAsync()
        {
            return await DbContext.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> expression)
        {
            return await DbContext.Set<T>().Where(expression).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllByConditionsAsync(List<Func<T, bool>> filters, byte filterType)
        {
            return await DbContext.Set<T>().Where(x => filters.Any(f => f(x))).ToListAsync();
        }

        public async Task CreateAsync(T entity)
        {
            await DbContext.Set<T>().AddAsync(entity);
        }

        public void Create(T entity)
        {
            DbContext.Set<T>().Add(entity);
        }

        public async Task CreateManyAsync(IEnumerable<T> entity)
        {
            await DbContext.Set<T>().AddRangeAsync(entity);
        }

        public void Update(T entity)
        {
            DbContext.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            DbContext.Set<T>().Remove(entity);
        }

        public void DeleteMany(IEnumerable<T> entities)
        {
            DbContext.Set<T>().RemoveRange(entities);
        }

        public async Task SaveAsync()
        {
            await DbContext.SaveChangesAsync();
        }

        public void Save()
        {
            DbContext.SaveChanges();
        }
    }
}