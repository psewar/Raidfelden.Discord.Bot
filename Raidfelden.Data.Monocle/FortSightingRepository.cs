using System;
using System.Collections.Generic;
using System.Text;
using Raidfelden.Data.Monocle.Entities;
using Raidfelden.Entities;

namespace Raidfelden.Data.Monocle
{
	public class FortSightingRepository : GenericCastRepository<FortSightings, int, IFortSighting>, IFortSightingRepository
	{
		public FortSightingRepository(Hydro74000Context context) : base(context)
		{
		}
	}
}
