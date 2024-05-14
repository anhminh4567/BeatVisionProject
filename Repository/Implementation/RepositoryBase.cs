﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Client;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Implementation
{
	public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        internal ApplicationDbContext _dbcontext;
        internal DbSet<T> _dbSet;
        public RepositoryBase(ApplicationDbContext context) 
        {
            _dbcontext = context;
            _dbSet = _dbcontext.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetByCondition(
            Expression<Func<T, bool>> filter = null, 
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, 
            string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public virtual async Task<T?> GetById(object id)
        {
            return await _dbSet.FindAsync(id);   
        }
		public virtual async Task<T?> GetByIdInclude(object id, string includeProperties = "")
		{
            var get = await GetById(id);
            if (get is null)
                return null;
			IQueryable<T> query = _dbSet;
			foreach (var includeProperty in includeProperties.Split
				(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				query = query.Include(includeProperty);
			}
            return await query.FirstOrDefaultAsync(t => t.Equals(get));
		}
		public virtual async Task<IEnumerable<T>> GetRange(int start, int amount)
		{
			return await _dbSet.Skip(start).Take(amount).ToListAsync();
		}
		public virtual async Task<T> Create(T entity)
        {
            return (await _dbSet.AddAsync(entity)).Entity;
        }

        public virtual Task<T> Delete(T entity) 
        { 
            return Task.FromResult(_dbSet.Remove(entity).Entity);
        }
        public virtual Task<T> Update(T entity)
        {
            return Task.FromResult(_dbSet.Update(entity).Entity);
        }
        public virtual void Save()
        {
            _dbcontext.SaveChanges();
        }

		
	}
}