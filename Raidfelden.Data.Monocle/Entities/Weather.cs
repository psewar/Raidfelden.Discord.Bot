using System.Collections.Generic;

namespace Raidfelden.Data.Monocle.Entities
{
    public partial class Weather
    {
        public Weather()
        {
            Forts = new HashSet<Forts>();
            MysterySightings = new HashSet<MysterySightings>();
            Sightings = new HashSet<Sightings>();
        }

        public int Id { get; set; }
        public ulong? S2CellId { get; set; }
        public short? Condition { get; set; }
        public short? AlertSeverity { get; set; }
        public sbyte? Warn { get; set; }
        public short? Day { get; set; }
        public int? Updated { get; set; }

        public ICollection<Forts> Forts { get; set; }
        public ICollection<MysterySightings> MysterySightings { get; set; }
        public ICollection<Sightings> Sightings { get; set; }
    }
}
