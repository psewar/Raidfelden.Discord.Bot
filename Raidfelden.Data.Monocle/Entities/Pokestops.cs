using Raidfelden.Entities;

namespace Raidfelden.Data.Monocle.Entities
{
    public partial class Pokestops : IPokestop
	{
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int? Updated { get; set; }

		double ILocation.Latitude
		{
			get { return Lat.Value; }
			set { Lat = value; }
		}

		double ILocation.Longitude
		{
			get { return Lon.Value; }
			set { Lon = value; }
		}
	}
}
