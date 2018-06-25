using Raidfelden.Data.Monocle.Entities;
using Raidfelden.Entities;

namespace Raidfelden.Data.Monocle
{
    public class SightingRepository : GenericCastRepository<Sightings, long, ISighting>, ISightingRepository
    {
        public SightingRepository(Hydro74000Context context) : base(context)
		{
        }
    }
}
