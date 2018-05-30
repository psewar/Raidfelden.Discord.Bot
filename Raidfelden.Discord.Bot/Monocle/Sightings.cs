using System;
using System.Collections.Generic;

namespace Raidfelden.Discord.Bot.Monocle
{
    public partial class Sightings
    {
        public long Id { get; set; }
        public short? PokemonId { get; set; }
        public long? SpawnId { get; set; }
        public int? ExpireTimestamp { get; set; }
        public ulong? EncounterId { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public byte? AtkIv { get; set; }
        public byte? DefIv { get; set; }
        public byte? StaIv { get; set; }
        public short? Move1 { get; set; }
        public short? Move2 { get; set; }
        public short? Gender { get; set; }
        public short? Form { get; set; }
        public short? Cp { get; set; }
        public short? Level { get; set; }
        public int? Updated { get; set; }
        public short? WeatherBoostedCondition { get; set; }
        public ulong? WeatherCellId { get; set; }
        public double? Weight { get; set; }

        public Weather WeatherCell { get; set; }
    }
}
