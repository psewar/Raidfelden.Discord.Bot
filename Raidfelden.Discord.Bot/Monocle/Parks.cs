using System;
using System.Collections.Generic;

namespace Raidfelden.Discord.Bot.Monocle
{
    public partial class Parks
    {
        public Parks()
        {
            Forts = new HashSet<Forts>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Coords { get; set; }
        public int? Updated { get; set; }
        public int Internalid { get; set; }
        public string Instanceid { get; set; }

        public ICollection<Forts> Forts { get; set; }
    }
}
