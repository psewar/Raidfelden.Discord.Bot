using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Raidfelden.Data
{
	public class GenericCastRepository<TEntity, TIdentifier, TInterface> : GenericRepository<TEntity, TIdentifier>, IGenericRepository<TInterface, TIdentifier> where TEntity : class, TInterface, new() where TInterface : class
	{
		public GenericCastRepository(DbContext context) : base(context)
		{
		}

		TInterface IGenericRepository<TInterface, TIdentifier>.Add(TInterface entity)
		{
			return Add((TEntity)entity);
		}

		async Task<TInterface> IGenericRepository<TInterface, TIdentifier>.AddAsync(TInterface entity)
		{
			return await AddAsync((TEntity)entity);
		}

		void IGenericRepository<TInterface, TIdentifier>.Delete(TInterface entity)
		{
			Delete((TEntity)entity);
		}

		async Task<int> IGenericRepository<TInterface, TIdentifier>.DeleteAsync(TInterface entity)
		{
			return await DeleteAsync((TEntity)entity);
		}

		TInterface IGenericRepository<TInterface, TIdentifier>.Find(Expression<Func<TInterface, bool>> match)
		{
			return GetAll().SingleOrDefault(match);
		}

		IQueryable<TInterface> IGenericRepository<TInterface, TIdentifier>.FindAll(Expression<Func<TInterface, bool>> match)
		{
			return GetAll().Where(match);
		}

		async Task<List<TInterface>> IGenericRepository<TInterface, TIdentifier>.FindAllAsync(Expression<Func<TInterface, bool>> match)
		{
			return await GetAll().Where(match).ToListAsync();
		}

		async Task<TInterface> IGenericRepository<TInterface, TIdentifier>.FindAsync(Expression<Func<TInterface, bool>> match)
		{
			return await GetAll().SingleOrDefaultAsync(match);
		}

		IQueryable<TInterface> IGenericRepository<TInterface, TIdentifier>.FindBy(Expression<Func<TInterface, bool>> predicate)
		{
			return GetAll().Where(predicate);
		}

		async Task<List<TInterface>> IGenericRepository<TInterface, TIdentifier>.FindByAsync(Expression<Func<TInterface, bool>> predicate)
		{
			return await GetAll().Where(predicate).ToListAsync();
		}

		TInterface IGenericRepository<TInterface, TIdentifier>.Get(TIdentifier identifier)
		{
			return Get(identifier);
		}

		IQueryable<TInterface> IGenericRepository<TInterface, TIdentifier>.GetAll()
		{
			return GetAll();
		}

		async Task<List<TInterface>> IGenericRepository<TInterface, TIdentifier>.GetAllAsync()
		{
			return await ConvertTaskListAsync(GetAllAsync());
		}

		IQueryable<TInterface> IGenericRepository<TInterface, TIdentifier>.GetAllIncluding(params Expression<Func<TInterface, TIdentifier>>[] includeProperties)
		{
			var queryable = ((IGenericRepository<TInterface, TIdentifier>)this).GetAll();
			foreach (Expression<Func<TInterface, TIdentifier>> includeProperty in includeProperties)
			{
				queryable = queryable.Include(includeProperty);
			}

			return queryable;
		}

		async Task<TInterface> IGenericRepository<TInterface, TIdentifier>.GetAsync(TIdentifier identifier)
		{
			return await GetAsync(identifier);
		}

		TInterface IGenericRepository<TInterface, TIdentifier>.Update(TIdentifier identifier, TInterface entity)
		{
			return Update(identifier, (TEntity)entity);
		}

		async Task<TInterface> IGenericRepository<TInterface, TIdentifier>.UpdateAsync(TIdentifier identifier, TInterface entity)
		{
			return await UpdateAsync(identifier, (TEntity)entity);
		}

		protected async Task<List<TInterface>> ConvertTaskListAsync(Task<List<TEntity>> entitiesTask)
		{
			var entities = await entitiesTask;
			return entities.Cast<TInterface>().ToList();
		}

		TInterface IGenericRepository<TInterface, TIdentifier>.CreateInstance()
		{
			return CreateInstance();
		}
	}
}