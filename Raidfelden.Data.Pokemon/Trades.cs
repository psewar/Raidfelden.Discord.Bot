using System;
using System.Collections.Generic;

namespace Raidfelden.Data.Pokemon
{
    public partial class Trade
    {
        public ulong Id { get; set; }
        public int UserId { get; set; }
        public int PokemonId { get; set; }
        public int TradeType { get; set; }

        public User User { get; set; }
    }
}
