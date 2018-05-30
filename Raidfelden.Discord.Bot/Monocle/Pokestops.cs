using System;
using System.Collections.Generic;

namespace Raidfelden.Discord.Bot.Monocle
{
    public partial class Pokestops
    {
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int? Updated { get; set; }
    }
}
