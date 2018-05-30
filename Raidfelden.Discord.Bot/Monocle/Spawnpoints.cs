using System;
using System.Collections.Generic;

namespace Raidfelden.Discord.Bot.Monocle
{
    public partial class Spawnpoints
    {
        public int Id { get; set; }
        public long? SpawnId { get; set; }
        public short? DespawnTime { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public int? Updated { get; set; }
        public byte? Duration { get; set; }
        public byte? Failures { get; set; }
    }
}
