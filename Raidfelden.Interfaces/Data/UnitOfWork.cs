using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raidfelden.Interfaces.Data
{
	public interface IUnitOfWork
	{
		void Register(IRepository repository);
		void Commit();
	}

	public class UnitOfWork : IUnitOfWork
	{
		private readonly Dictionary<string, IRepository> _repositories;
		public UnitOfWork()
		{
			_repositories = new Dictionary<string, IRepository>();
		}

		public void Register(IRepository repository)
		{
			_repositories.Add(repository.GetType().Name, repository);
		}

		public void Commit()
		{
			_repositories.ToList().ForEach(x => x.Value.Submit());
		}
	}
}
