using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Raidfelden.Data.Monocle.Entities;
using Raidfelden.Entities;

namespace Raidfelden.Data.Monocle
{
	public class RaidRepository : GenericCastRepository<Raids, int, IRaid>, IRaidRepository
	{
		public RaidRepository(Hydro74000Context context) : base(context)
		{
		}

		public async Task<List<IRaid>> FindAllWithGymsAsync(Expression<Func<IRaid, bool>> match)
		{
			return await Context.Set<Raids>().Include(e => e.Fort).Where(match).ToListAsync();
		}
	}
}
