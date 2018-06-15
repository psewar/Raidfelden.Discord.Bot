﻿using Raidfelden.Data.Monocle.Entities;
using Raidfelden.Interfaces.Entities;

namespace Raidfelden.Data.Monocle
{
	public class GymRepository : GenericCastRepository<Forts, int, IGym>, IGymRepository
	{
		public GymRepository(Hydro74000Context context) : base(context)
		{
		}
	}
}
