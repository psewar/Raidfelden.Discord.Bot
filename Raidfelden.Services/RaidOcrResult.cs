using System;
using Raidfelden.Entities;

namespace Raidfelden.Services
{

	public partial class OcrService
	{
		public class RaidOcrResult
		{
			public OcrResult<int> EggLevel { get; set; }
			public OcrResult<TimeSpan> EggTimer { get; set; }
			public OcrResult<IGym> Gym { get; set; }
			public OcrResult<RaidbossPokemon> Pokemon { get; set; }
			public OcrResult<int> PokemonCp { get; set; }
			public OcrResult<TimeSpan> RaidTimer { get; set; }

			public bool IsRaidImage => EggTimer.IsSuccess || RaidTimer.IsSuccess;
			public bool IsRaidBoss => RaidTimer.IsSuccess && Pokemon.IsSuccess;

			public bool IsSuccess
			{
				get
				{
					if (!IsRaidImage)
					{
						return false;
					}

					if (IsRaidBoss)
					{
						return Gym.IsSuccess && Pokemon.IsSuccess && RaidTimer.IsSuccess;
					}

					return Gym.IsSuccess && EggLevel.IsSuccess && EggTimer.IsSuccess;
				}
			}
		}
	}
}
