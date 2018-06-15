namespace Raidfelden.Entities
{
    public interface IRaid
    {
		int Id { get; set; }
		long? ExternalId { get; set; }
		int? FortId { get; set; }
		byte? Level { get; set; }
		short? PokemonId { get; set; }
		short? Move1 { get; set; }
		short? Move2 { get; set; }
		int? TimeSpawn { get; set; }
		int? TimeBattle { get; set; }
		int? TimeEnd { get; set; }
		int? Cp { get; set; }
	}
}
