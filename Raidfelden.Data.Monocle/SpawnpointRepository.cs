using System.Collections.Generic;
using System.Threading.Tasks;
using Raidfelden.Data.Monocle.Entities;

namespace Raidfelden.Data.Monocle
{
    public class SpawnpointRepository : GenericCastRepository<Spawnpoints, int, ISpawnpoint>, ISpawnpointRepository
    {
		public SpawnpointRepository(Hydro74000Context context) : base(context)
		{
		}

        public async Task<IEnumerable<ISpawnpoint>> GetNearestSpawnpointsAsync(double latitude, double longitude)
        {
            var context = Context as Hydro74000Context;
            return await context.GetNearestSpawnpointsAsync(latitude, longitude);
        }
    }
}
