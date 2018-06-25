using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raidfelden.Data
{
    public interface ISpawnpointRepository : IGenericRepository<ISpawnpoint, int>
    {
        Task<IEnumerable<ISpawnpoint>> GetNearestSpawnpointsAsync(double latitude, double longitude);
    }
}
