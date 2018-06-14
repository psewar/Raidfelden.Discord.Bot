using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Services;

namespace Raidfelden.Discord.Bot.Tests
{
    public class OcrTestsHelper
    {
		public static OcrService GetOcrService(IConfigurationService configurationService, Hydro74000Context context)
		{
			var localizationService = new LocalizationService();
			var gymService = new GymService(localizationService, configurationService);
			var raidbossService = new RaidbossService();
			var fileWatcherService = new FileWatcherService();
			var pokemonService = new PokemonService(raidbossService, localizationService, fileWatcherService);
			var raidService = new RaidService(context, gymService, pokemonService, raidbossService, localizationService);
			return new OcrService(context, configurationService, gymService, pokemonService, raidService, localizationService);
		}

		public static string GetOcrResultString(OcrService ocrService, string filePath, FenceConfiguration[] fences = null)
		{
			var utcNow = SystemClock.Instance.GetCurrentInstant().InUtc();
			var channelTimeZone = DateTimeZoneProviders.Tzdb["Europe/Zurich"];
			var result = ocrService.AddRaidAsync(utcNow, channelTimeZone, filePath, 4, fences, true).Result;
			return result.Message;
		}

		public static ServiceResponse GetOcrResult(OcrService ocrService, string filePath, FenceConfiguration[] fences = null)
		{
			var utcNow = SystemClock.Instance.GetCurrentInstant().InUtc();
			var channelTimeZone = DateTimeZoneProviders.Tzdb["Europe/Zurich"];
			return ocrService.AddRaidAsync(utcNow, channelTimeZone, filePath, 4, fences, true).Result;
		}
	}
}
