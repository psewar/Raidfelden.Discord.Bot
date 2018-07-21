using System;
using Raidfelden.Entities;

namespace Raidfelden.Data.Pokemon.Entities
{
    public partial class Trade : ITrade
    {
        public ulong Id { get; set; }
        public int UserId { get; set; }
        public int PokemonId { get; set; }
        public TradeType TradeType { get; set; }
		public string Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public bool IsDeleted { get; set; }

		public User User { get; set; }
    }
}
