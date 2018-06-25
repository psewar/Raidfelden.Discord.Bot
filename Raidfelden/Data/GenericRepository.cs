using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Raidfelden.Data
{
	public abstract class GenericRepository<TEntity, TIdentifier> : IGenericRepository<TEntity, TIdentifier> where TEntity : class, new()
	{
		protected DbContext Context { get; }

		protected GenericRepository(DbContext context)
		{
			Context = context;
		}

		public IQueryable<TEntity> GetAll()
		{
			return Context.Set<TEntity>();
		}

		public virtual async Task<List<TEntity>> GetAllAsync()
		{
			return await Context.Set<TEntity>().ToListAsync();
		}

		public virtual TEntity Get(TIdentifier identifier)
		{
			return Context.Set<TEntity>().Find(identifier);
		}

		public virtual async Task<TEntity> GetAsync(TIdentifier identifier)
		{
			return await Context.Set<TEntity>().FindAsync(identifier);
		}

		public virtual TEntity Add(TEntity entity)
		{
			Context.Set<TEntity>().Add(entity);
			Context.SaveChanges();
			return entity;
		}

		public virtual async Task<TEntity> AddAsync(TEntity entityt)
		{
			Context.Set<TEntity>().Add(entityt);
			await Context.SaveChangesAsync();
			return entityt;
		}

		public virtual TEntity Find(Expression<Func<TEntity, bool>> match)
		{
			return Context.Set<TEntity>().SingleOrDefault(match);
		}

		public virtual async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> match)
		{
			return await Context.Set<TEntity>().SingleOrDefaultAsync(match);
		}

		public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> match)
		{
			return Context.Set<TEntity>().Where(match);
		}

		public async Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> match)
		{
			return await Context.Set<TEntity>().Where(match).ToListAsync();
		}

		public virtual void Delete(TIdentifier identifier)
		{
			var entity = Get(identifier);
			Delete(entity);
		}

		public virtual async Task<int> DeleteAsync(TIdentifier identifier)
		{
			var entity = await GetAsync(identifier);
			return await DeleteAsync(entity);
		}

		public virtual void Delete(TEntity entity)
		{
			Context.Set<TEntity>().Remove(entity);
			Context.SaveChanges();
		}

		public virtual async Task<int> DeleteAsync(TEntity entity)
		{
			Context.Set<TEntity>().Remove(entity);
			return await Context.SaveChangesAsync();
		}

		public virtual TEntity Update(TIdentifier identifier, TEntity entity)
		{
			if (entity == null)
			{
				return null;
			}
			var exist = Context.Set<TEntity>().Find(identifier);
			if (exist != null)
			{
				Context.Entry(exist).CurrentValues.SetValues(entity);
				Context.SaveChanges();
			}
			return exist;
		}

		public virtual async Task<TEntity> UpdateAsync(TIdentifier identifier, TEntity entity)
		{
			if (entity == null)
			{
				return null;
			}
			var exist = await Context.Set<TEntity>().FindAsync(identifier);
			if (exist != null)
			{
				Context.Entry(exist).CurrentValues.SetValues(entity);
				await Context.SaveChangesAsync();
			}
			return exist;
		}

		public int Count()
		{
			return Context.Set<TEntity>().Count();
		}

		public async Task<int> CountAsync()
		{
			return await Context.Set<TEntity>().CountAsync();
		}

		public virtual void Save()
		{
			Context.SaveChanges();
		}

		public virtual async Task<int> SaveAsync()
		{
			return await Context.SaveChangesAsync();
		}

		public virtual IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate)
		{
			var query = Context.Set<TEntity>().Where(predicate);
			return query;
		}

		public virtual async Task<List<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate)
		{
			return await Context.Set<TEntity>().Where(predicate).ToListAsync();
		}

		public IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, TIdentifier>>[] includeProperties)
		{

			var queryable = GetAll();
			foreach (Expression<Func<TEntity, TIdentifier>> includeProperty in includeProperties)
			{
				queryable = queryable.Include(includeProperty);
			}

			return queryable;
		}

		public TEntity CreateInstance()
		{
			return new TEntity();
		}

		private bool _disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				Context.Dispose();
			}
			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
