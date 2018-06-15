using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Raidfelden.Configuration;
using Raidfelden.Configuration.Providers.Fences.Novabot;

namespace Raidfelden.Services
{
    public interface IConfigurationService
    {
        IEnumerable<FenceConfiguration> GetFencesConfigurationForChannel(ChannelConfiguration channelConfiguration);
        OcrConfiguration GetOcrConfiguration();
	    AppConfiguration GetAppConfiguration();
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly AppConfiguration _appConfiguration;
        private readonly FencesConfiguration _fencesConfiguration;

        public ConfigurationService()
        {
			var configuration = new ConfigurationBuilder()
					.AddNovabotGeoFencesFile("geofences.txt")
					.AddJsonFile("settings.json")
					.Build();

			var appConfig = new AppConfiguration();
			var section = configuration.GetSection("AppConfiguration");
			section.Bind(appConfig);
			_appConfiguration = appConfig;

			var fencesSection = configuration.GetSection("FencesConfiguration");
			var fences = new FencesConfiguration();
			fencesSection.Bind(fences);
			_fencesConfiguration = fences;
        }

	    public AppConfiguration GetAppConfiguration()
	    {
		    return _appConfiguration;
	    }
		
        private IEnumerable<ChannelConfiguration> GetChannelConfigurations(GuildConfiguration discord)
        {
            return discord.Raids.Cast<ChannelConfiguration>().Union(discord.Pokemon);
        }

        public IEnumerable<FenceConfiguration> GetFencesConfigurationForChannel(ChannelConfiguration channelConfiguration)
        {
            if (channelConfiguration == null)
            {
                return new List<FenceConfiguration>();
            }
            return _fencesConfiguration.GetFences(channelConfiguration.Fences);
        }

        public OcrConfiguration GetOcrConfiguration()
        {
            var result = _appConfiguration.OcrConfiguration 
					   ?? new OcrConfiguration();
	        return result;
        }
	}
}
