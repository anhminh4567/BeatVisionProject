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
        int COUNT { get;  }
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetByCondition(Expression<Func<T, bool>> expression = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "", int? skip = null, int? take = null);
        Task<T?> GetById(object id);
        Task<T?> GetByIdInclude(object id, string includeProperties = "");
		Task<IEnumerable<T>> GetRange(int start, int take);
        Task<IList<TAnything>> GetByCondition_selectReturn<TAnything>(
			Expression<Func<T, TAnything>> selector,
			Expression<Func<T, bool>> expression = null,
			Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
			string includeProperties = "");
		Task<T> Create(T entity);
        Task<bool> CreateRange(IEnumerable<T> entityRange);
		Task<T> Update(T entity);
		Task<T> Delete(T entity);
		Task<bool> DeleteRange(IEnumerable<T> entityRange);

		void Save();
    }
}
