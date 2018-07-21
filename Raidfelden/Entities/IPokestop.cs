namespace Raidfelden.Entities
{
	public interface IPokestop : ILocation
	{
		int Id { get; set; }
		string ExternalId { get; set; }
		double? Lat { get; set; }
		double? Lon { get; set; }
		string Name { get; set; }
		string Url { get; set; }
		int? Updated { get; set; }
	}
}
