using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IRepositoryBase<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetByCondition(Expression<Func<T, bool>> expression = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "");
        Task<T?> GetById(object id);
        Task<T?> GetByIdInclude(object id, string includeProperties = "");
		Task<IEnumerable<T>> GetRange(int start, int take);
		Task<T> Create(T entity);
		Task<T> Update(T entity);
		Task<T> Delete(T entity);
        void Save();
    }
}
