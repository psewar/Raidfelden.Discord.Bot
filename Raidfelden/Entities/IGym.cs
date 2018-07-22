namespace Raidfelden.Entities
{
    public interface IGym : ILocation
    {
	    int Id { get; }
	    string Name { get; set; }
	    string PictureUrl { get; set; }
		string ExternalId { get; set; }
		ulong WeatherCellId { get; set; }
	}
}
