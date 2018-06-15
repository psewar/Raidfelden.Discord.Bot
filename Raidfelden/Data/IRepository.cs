using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Raidfelden.Data
{
	public interface IGenericRepository<TEntity, TIdentifier> where TEntity : class
	{
		TEntity Add(TEntity entity);
		Task<TEntity> AddAsync(TEntity entity);
		int Count();
		Task<int> CountAsync();
		void Delete(TIdentifier identifier);
		Task<int> DeleteAsync(TIdentifier identifier);
		void Delete(TEntity entity);
		Task<int> DeleteAsync(TEntity entity);
		void Dispose();
		TEntity Find(Expression<Func<TEntity, bool>> match);
		IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> match);
		Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> match);
		Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> match);
		IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate);
		Task<List<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate);
		TEntity Get(TIdentifier identifier);
		IQueryable<TEntity> GetAll();
		Task<List<TEntity>> GetAllAsync();
		IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, TIdentifier>>[] includeProperties);
		Task<TEntity> GetAsync(TIdentifier identifier);
		void Save();
		Task<int> SaveAsync();
		TEntity Update(TIdentifier identifier, TEntity entity);
		Task<TEntity> UpdateAsync(TIdentifier identifier, TEntity entity);
		TEntity CreateInstance();
	}
}
