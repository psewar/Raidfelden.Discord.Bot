using System.Collections.Generic;
using Raidfelden.Entities;

namespace Raidfelden.Data.Monocle.Entities
{
    public partial class Forts : IGym
    {
        public Forts()
        {
            FortSightings = new HashSet<FortSightings>();
            GymDefenders = new HashSet<GymDefenders>();
            Raids = new HashSet<Raids>();
        }

        public int Id { get; set; }
        public string ExternalId { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public short? Sponsor { get; set; }
        public ulong? WeatherCellId { get; set; }
        public int? Parkid { get; set; }
        public string Park { get; set; }

        public Parks ParkNavigation { get; set; }
        public Weather WeatherCell { get; set; }
        public ICollection<FortSightings> FortSightings { get; set; }
        public ICollection<GymDefenders> GymDefenders { get; set; }
        public ICollection<Raids> Raids { get; set; }

	    string IGym.PictureUrl
	    {
		    get { return Url; }
		    set { Url = value; }
	    }

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

	    ulong IGym.WeatherCellId
	    {
			get { return WeatherCellId.Value; }
		    set { WeatherCellId = value; }
	    }
	}
}
