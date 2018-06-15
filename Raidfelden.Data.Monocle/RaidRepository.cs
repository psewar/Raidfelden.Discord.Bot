using Raidfelden.Data.Monocle.Entities;
using Raidfelden.Entities;

namespace Raidfelden.Data.Monocle
{
	public class RaidRepository : GenericCastRepository<Raids, int, IRaid>, IRaidRepository
	{
		public RaidRepository(Hydro74000Context context) : base(context)
		{
		}
	}
}
