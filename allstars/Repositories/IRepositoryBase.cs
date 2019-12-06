using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public interface IRepositoryBase<T>
    {
        Task<IEnumerable<T>> FindAllAsync();

        Task<IEnumerable<T>> FindAllByConditionsAsync(List<Func<T, bool>> filters, byte filterType);

        Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> expression);

        Task CreateAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task SaveAsync();
    }
}