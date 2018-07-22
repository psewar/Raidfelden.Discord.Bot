using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Raidfelden.Entities;

namespace Raidfelden.Data
{
    public interface IRaidRepository : IGenericRepository<IRaid, int>
    {
	    Task<List<IRaid>> FindAllWithGymsAsync(Expression<Func<IRaid, bool>> match);
    }
}
