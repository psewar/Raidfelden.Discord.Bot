using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Raidfelden.Configuration;
using Raidfelden.Configuration.Providers.Fences.Novabot;
using NodaTime;

namespace Raidfelden.Services
{
    public interface IConfigurationService
    {
        AppConfiguration GetAppConfiguration();
        OcrConfiguration GetOcrConfiguration();
        string GetConnectionString(string name);
        GuildConfiguration GetGuildConfiguration(ulong? guildId);
        string GetCommandPrefix(GuildConfiguration guildConfiguration);       
        IEnumerable<ChannelConfiguration> GetChannelConfigurations(GuildConfiguration guildConfiguration, string channelName);
        IEnumerable<FenceConfiguration> GetFencesConfigurationForChannel(ChannelConfiguration channelConfiguration);        
        DateTimeZone GetChannelDateTimeZone(GuildConfiguration guildConfiguration, string channelName);
        bool ShouldProcessRequestAnyway(string userName);
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRoot _root;
        private readonly AppConfiguration _appConfiguration;
        private readonly FencesConfiguration _fencesConfiguration;

        public ConfigurationService()
        {
            _root = new ConfigurationBuilder()
					.AddNovabotGeoFencesFile("geofences.txt")
					.AddJsonFile("settings.json")
					.Build();

			var appConfig = new AppConfiguration();
			var section = _root.GetSection("AppConfiguration");
			section.Bind(appConfig);
			_appConfiguration = appConfig;

			var fencesSection = _root.GetSection("FencesConfiguration");
			var fences = new FencesConfiguration();
			fencesSection.Bind(fences);
			_fencesConfiguration = fences;
        }

	    public AppConfiguration GetAppConfiguration()
	    {
		    return _appConfiguration;
	    }

        public string GetConnectionString(string name)
        {
            return _root.GetConnectionString(name);
        }

        public GuildConfiguration GetGuildConfiguration(ulong? guildId)
        {
            if (!guildId.HasValue)
            {
                return null;
            }
            return _appConfiguration.Guilds.SingleOrDefault(e => e.GuildId == guildId.Value);
        }

        public string GetCommandPrefix(GuildConfiguration guildConfiguration)
        {
            var prefix = _appConfiguration.DefaultCommandPrefix;
            if (guildConfiguration != null)
            {
                if (!string.IsNullOrWhiteSpace(guildConfiguration.CommandPrefix))
                {
                    prefix = guildConfiguration.CommandPrefix;
                }
            }
            return prefix;
        }

        public IEnumerable<ChannelConfiguration> GetChannelConfigurations(GuildConfiguration guildConfiguration, string channelName)
        {
            if (guildConfiguration == null)
            {
                yield break;
            }
            channelName = channelName.ToLowerInvariant();
            var configurations = GetChannelConfigurations(guildConfiguration);
            foreach (var configuration in configurations)
            {
                if (configuration.IsWildcardChannelWithFenceInName)
                {
                    if (channelName.StartsWith(configuration.Name.ToLowerInvariant()))
                    {
                        // Add part of the channel name to the fences list
                        //var fences = configuration.Fences != null ? configuration.Fences.ToList() : new List<string>();
                        //fences.Add(channelName.Substring(configuration.Name.Length));
                        //configuration.Fences = fences.Distinct().ToArray();
                        configuration.Fences = channelName.Substring(configuration.Name.Length).Split('-');
                        yield return configuration;
                    }
                }
                else
                {
                    if (channelName == configuration.Name.ToLowerInvariant())
                    {
                        yield return configuration;
                    }
                }
            }
            //var channelSetting = GetChannelConfigurations(guildSetting).FirstOrDefault(e => e.HasRegionConstraint 
            //                                                                        ? channelName.StartsWith(e.Name.ToLowerInvariant()) 
            //                                                                        : channelName == e.Name.ToLowerInvariant());
            //return channelSetting;
        }

        private IEnumerable<ChannelConfiguration> GetChannelConfigurations(GuildConfiguration discord)
        {
            return discord.Raids.Cast<ChannelConfiguration>().Union(discord.Pokemon);
        }

        public bool ShouldProcessRequestAnyway(string userName)
        {
            return userName == "psewar";
        }

        public DateTimeZone GetChannelDateTimeZone(GuildConfiguration guildConfiguration, string channelName)
        {
            var defaultZone = _appConfiguration.Timezone;
            var channelConfigurations = GetChannelConfigurations(guildConfiguration, channelName);
            var channelZone = channelConfigurations.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Timezone));
            if (channelZone != null)
            {
                return DateTimeZoneProviders.Tzdb[channelZone.Timezone];
            }
            if (defaultZone != null)
            {
                return DateTimeZoneProviders.Tzdb[defaultZone];
            }
            return DateTimeZone.Utc;
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
