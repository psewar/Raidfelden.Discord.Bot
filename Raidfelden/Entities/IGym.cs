namespace Raidfelden.Entities
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
