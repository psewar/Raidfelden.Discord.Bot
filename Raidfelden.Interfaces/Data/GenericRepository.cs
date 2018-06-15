using System;
using System.Collections.Generic;
using System.Text;

namespace Raidfelden.Interfaces.Data
{
	public abstract class GenericRepository<T> : IRepository<T> where T : class
	{
		protected GenericRepository(IUnitOfWork unitOfWork)
		{
			unitOfWork.Register(this);
		}

		public abstract void Submit();
	}
}
