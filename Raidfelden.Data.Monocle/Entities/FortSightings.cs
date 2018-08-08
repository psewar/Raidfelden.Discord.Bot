using Raidfelden.Entities;

namespace Raidfelden.Data.Monocle.Entities
{
    public partial class FortSightings : IFortSighting
	{
        public int Id { get; set; }
        public int? FortId { get; set; }
        public int? LastModified { get; set; }
        public byte Team { get; set; }
        public short? GuardPokemonId { get; set; }
        public short? SlotsAvailable { get; set; }
        public sbyte? IsInBattle { get; set; }
        public int? Updated { get; set; }

        public Forts Fort { get; set; }
    }
}
