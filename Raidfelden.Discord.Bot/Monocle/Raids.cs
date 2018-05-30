using System;
using System.Collections.Generic;

namespace Raidfelden.Discord.Bot.Monocle
{
    public partial class Raids
    {
        public int Id { get; set; }
        public long? ExternalId { get; set; }
        public int? FortId { get; set; }
        public byte? Level { get; set; }
        public short? PokemonId { get; set; }
        public short? Move1 { get; set; }
        public short? Move2 { get; set; }
        public int? TimeSpawn { get; set; }
        public int? TimeBattle { get; set; }
        public int? TimeEnd { get; set; }
        public int? Cp { get; set; }

        public Forts Fort { get; set; }
    }
}
