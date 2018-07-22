using Microsoft.Extensions.DependencyInjection;

namespace Raidfelden.Services
{
	public static class Setup
	{
		public static IServiceCollection ConfigureServices(this IServiceCollection services)
		{
			services
				.AddScoped<IConfigurationService, ConfigurationService>()
				.AddScoped<IFileWatcherService, FileWatcherService>()
				.AddScoped<IGymService, GymService>()
				.AddScoped<IPokestopService, PokestopService>()
				.AddScoped<ILocalizationService, LocalizationService>()
				.AddScoped<IOcrService, OcrService>()
				.AddScoped<IPokemonService, PokemonService>()
				.AddScoped<IRaidbossService, RaidbossService>()
				.AddScoped<IRaidService, RaidService>()
				.AddScoped<IUserService, UserService>()
				.AddScoped<ITradeService, TradeService>()
				.AddScoped<IQuestService, QuestService>();
			return services;
		}
	}
}
