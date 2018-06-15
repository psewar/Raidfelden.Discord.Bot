namespace Raidfelden.Data
{
    public interface ISpawnpoint
    {
        int Id { get; set; }
        long? SpawnId { get; set; }
        short? DespawnTime { get; set; }
        double? Lat { get; set; }
        double? Lon { get; set; }
        int? Updated { get; set; }
        byte? Duration { get; set; }
        byte? Failures { get; set; }
    }
}
