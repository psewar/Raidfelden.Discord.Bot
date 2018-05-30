using System;
using System.Collections.Generic;
using System.Text;

namespace Raidfelden.Discord.Bot.Configuration
{
    public class AppConfiguration
    {
        public AppConfiguration()
        {
            CultureCode = "en-US";
        }

        public string CultureCode { get; set; }
        public string BotToken { get; set; }
        public string DefaultCommandPrefix { get; set; }
        public GuildConfiguration[] Guilds { get; set; }
    }

    public class GuildConfiguration
    {
        public GuildConfiguration()
        {
            Raids = new RaidChannel[0];
            Pokemon = new PokemonChannel[0];
        }

        public ulong GuildId { get; set; }
        public string CommandPrefix { get; set; }
        public RaidChannel[] Raids { get; set; }
        public PokemonChannel[] Pokemon { get; set; }
    }

    public abstract class ChannelConfiguration
    {
        public string Name { get; set; }        
        public string[] Fences { get; set; }
        //public bool HasRegionConstraint { get { return Fences != null && Fences.Length > 0; } }
        public bool IsWildcardChannelWithFenceInName { get; set; }
        public bool IsOcrAllowed { get; set; }
    }

    public class RaidChannel : ChannelConfiguration { }

    public class PokemonChannel : ChannelConfiguration { }
}
