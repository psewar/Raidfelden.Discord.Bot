using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;

namespace Raidfelden.Configuration
{
    public class FencesConfiguration
    {
        public FenceConfiguration[] Fences { get; set; }
        public IEnumerable<FenceConfiguration> GetFences(string[] names)
        {
            if (Fences == null || names == null)
            {
                return new List<FenceConfiguration>();
            }
            return Fences.Where(e => e.Names.Any(n => names.Contains(n)));
        }
    }

    public class FenceConfiguration
    {
        public string[] Names { get; set; }
        public CoordinateConfiguration[] Coordinates { get; set; }

        private Envelope _area;
        public Envelope Area
        {
            get
            {
                if (_area != null)
                {
                    return _area;
                }

                if (Coordinates.Length == 0)
                {
                    _area = new Envelope();
                }
                else
                {
                    _area = new Envelope(ToCoordinate(Coordinates[0]));
                    foreach (var point in Coordinates)
                    {
                        _area.ExpandToInclude(ToCoordinate(point));
                    }
                }
                return _area;
            }
        }

        private Coordinate ToCoordinate(CoordinateConfiguration coordinateConfiguration)
        {
            return new Coordinate(coordinateConfiguration.Latitude, coordinateConfiguration.Longitude);
        }
    }

    public class CoordinateConfiguration
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
