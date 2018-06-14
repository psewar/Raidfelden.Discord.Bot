using Discord.Commands;
using Raidfelden.Discord.Bot.Configuration;
using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace Raidfelden.Discord.Bot.Services
{
    public interface IConfigurationService
    {
        string GetCommandPrefix(ICommandContext context);
        GuildConfiguration GetGuildConfiguration(ICommandContext context);
        IEnumerable<ChannelConfiguration> GetChannelConfigurations(ICommandContext context);
        bool ShouldProcessRequestAnyway(ICommandContext context);
        IEnumerable<FenceConfiguration> GetFencesConfigurationForChannel(ChannelConfiguration channelConfiguration);
        OcrConfiguration GetOcrConfiguration();
	    DateTimeZone GetChannelDateTimeZone(ICommandContext context);
	    AppConfiguration GetAppConfiguration();
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly AppConfiguration _configuration;
        private readonly FencesConfiguration _fencesConfiguration;

        public ConfigurationService(AppConfiguration configuration, FencesConfiguration fencesConfiguration)
        {
            _configuration = configuration;
            _fencesConfiguration = fencesConfiguration;
        }

	    public AppConfiguration GetAppConfiguration()
	    {
		    return _configuration;
	    }
		
		public string GetCommandPrefix(ICommandContext context)
        {
            var prefix = _configuration.DefaultCommandPrefix;
            var guild = GetGuildConfiguration(context);
            if (guild != null)
            {
                if(!string.IsNullOrWhiteSpace(guild.CommandPrefix))
                {
                    prefix = guild.CommandPrefix;
                }
            }
            return prefix;
        }

        public GuildConfiguration GetGuildConfiguration(ICommandContext context)
        {
            if (context.Guild == null)
            {
                return null;
            }
            var guildId = context.Guild.Id;
            return _configuration.Guilds.SingleOrDefault(e => e.GuildId == guildId);
        }

        public IEnumerable<ChannelConfiguration> GetChannelConfigurations(ICommandContext context)
        {
            var guildSetting = GetGuildConfiguration(context);
            if (guildSetting == null)
            {
                yield break;
            }
            var channelName = context.Channel.Name.ToLowerInvariant();
            var configurations = GetChannelConfigurations(guildSetting);
            foreach(var configuration in configurations)
            {
                if (configuration.IsWildcardChannelWithFenceInName)
                {
                    if (channelName.StartsWith(configuration.Name.ToLowerInvariant()))
                    {
                        // Add part of the channel name to the fences list
                        //var fences = configuration.Fences != null ? configuration.Fences.ToList() : new List<string>();
                        //fences.Add(channelName.Substring(configuration.Name.Length));
                        //configuration.Fences = fences.Distinct().ToArray();
                        configuration.Fences = new[] { channelName.Substring(configuration.Name.Length) };
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

        public bool ShouldProcessRequestAnyway(ICommandContext context)
        {
            return context.Message.Author.Username == "psewar";
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
            var result = _configuration.OcrConfiguration 
					   ?? new OcrConfiguration();
	        return result;
        }

	    public DateTimeZone GetChannelDateTimeZone(ICommandContext context)
	    {
		    var defaultZone = _configuration.Timezone;
		    var channelConfigurations = GetChannelConfigurations(context);
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
	}
}
