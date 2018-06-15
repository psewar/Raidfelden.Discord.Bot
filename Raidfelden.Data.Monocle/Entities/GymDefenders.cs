namespace Raidfelden.Data.Monocle.Entities
{
    public partial class GymDefenders
    {
        public long Id { get; set; }
        public int FortId { get; set; }
        public ulong ExternalId { get; set; }
        public short? PokemonId { get; set; }
        public string OwnerName { get; set; }
        public string Nickname { get; set; }
        public int? Cp { get; set; }
        public int? Stamina { get; set; }
        public int? StaminaMax { get; set; }
        public short? AtkIv { get; set; }
        public short? DefIv { get; set; }
        public short? StaIv { get; set; }
        public short? Move1 { get; set; }
        public short? Move2 { get; set; }
        public int? BattlesAttacked { get; set; }
        public int? BattlesDefended { get; set; }
        public short? NumUpgrades { get; set; }
        public int? Created { get; set; }
        public int? LastModified { get; set; }
        public byte? Team { get; set; }

        public Forts Fort { get; set; }
    }
}
