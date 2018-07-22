using System;

namespace Raidfelden.Entities
{
    public interface ITrade
    {
		ulong Id { get; set; }
		int UserId { get; set; }
		int PokemonId { get; set; }
		TradeType TradeType { get; set; }
	    string Description { get; set; }
	    DateTime CreatedAt { get; set; }
	    bool IsDeleted { get; set; }
    }

	public enum TradeType
	{
		None = 0,
		Buy = 1,
		Sell = 2
	}
}
