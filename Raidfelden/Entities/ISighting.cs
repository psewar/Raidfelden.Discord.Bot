using System;
using System.Collections.Generic;
using System.Text;

namespace Raidfelden.Entities
{
    public interface ISighting
    {          
        long Id { get; set; }
        short? PokemonId { get; set; }
        long? SpawnId { get; set; }
        int? ExpireTimestamp { get; set; }
        ulong? EncounterId { get; set; }
        double? Lat { get; set; }
        double? Lon { get; set; }
        byte? AtkIv { get; set; }
        byte? DefIv { get; set; }
        byte? StaIv { get; set; }
        short? Move1 { get; set; }
        short? Move2 { get; set; }
        short? Gender { get; set; }
        short? Form { get; set; }
        short? Cp { get; set; }
        short? Level { get; set; }
        int? Updated { get; set; }
        short? WeatherBoostedCondition { get; set; }
        ulong? WeatherCellId { get; set; }
        double? Weight { get; set; }
    }
}
