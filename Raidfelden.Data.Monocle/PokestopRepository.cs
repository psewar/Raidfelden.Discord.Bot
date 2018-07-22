using Raidfelden.Data.Monocle.Entities;
using Raidfelden.Entities;

namespace Raidfelden.Data.Monocle
{
    public class PokestopRepository : GenericCastRepository<Pokestops, int, IPokestop>, IPokestopRepository
	{
		public PokestopRepository(Hydro74000Context context) : base(context)
		{
		}
	}
}
