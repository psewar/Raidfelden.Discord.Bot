using System;
using System.Collections.Generic;
using System.Text;

namespace Raidfelden.Interfaces.Entities
{
    public interface IGym
    {
	    int Id { get; }
	    string Name { get; }
	    string PictureUrl { get; }
	    double Latitude { get; }
	    double Longitude { get; }
    }
}
