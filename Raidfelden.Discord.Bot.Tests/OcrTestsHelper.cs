using NodaTime;
using Raidfelden.Services;
using Raidfelden.Configuration;
using Raidfelden.Discord.Bot.Resources;
using Raidfelden.Data;
using Raidfelden.Data.Monocle;

namespace Raidfelden.Discord.Bot.Tests
{
    public class OcrTestsHelper
    {
		public static OcrService GetOcrService(IConfigurationService configurationService, Hydro74000Context context)
		{
            IGymRepository gymRepository = new GymRepository(context);
            IRaidRepository raidRepository = new RaidRepository(context);
			var localizationService = new LocalizationService();
			var gymService = new GymService(gymRepository, localizationService, configurationService);
			var raidbossService = new RaidbossService();
			var fileWatcherService = new FileWatcherService();
			var pokemonService = new PokemonService(raidbossService, localizationService, fileWatcherService);
			var raidService = new RaidService(raidRepository, gymService, pokemonService, raidbossService, localizationService);
			return new OcrService(configurationService, gymService, pokemonService, raidService, localizationService);
		}

		public static string GetOcrResultString(IOcrService ocrService, string filePath, FenceConfiguration[] fences = null)
		{
			var utcNow = SystemClock.Instance.GetCurrentInstant().InUtc();
			var channelTimeZone = DateTimeZoneProviders.Tzdb["Europe/Zurich"];
			var result = ocrService.AddRaidAsync(typeof(i18n), utcNow, channelTimeZone, filePath, 4, fences, true).Result;
			return result.Message;
		}

		public static ServiceResponse GetOcrResult(OcrService ocrService, string filePath, FenceConfiguration[] fences = null)
		{
			var utcNow = SystemClock.Instance.GetCurrentInstant().InUtc();
			var channelTimeZone = DateTimeZoneProviders.Tzdb["Europe/Zurich"];
			return ocrService.AddRaidAsync(typeof(i18n), utcNow, channelTimeZone, filePath, 4, fences, true).Result;
		}
	}
}
