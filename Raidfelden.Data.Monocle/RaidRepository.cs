using Raidfelden.Data.Monocle.Entities;
using Raidfelden.Interfaces.Entities;

namespace Raidfelden.Data.Monocle
{
	public class RaidRepository : GenericCastRepository<Forts, int, IGym>, IGymRepository
	{
		public RaidRepository(Hydro74000Context context) : base(context)
		{
		}
	}
}
