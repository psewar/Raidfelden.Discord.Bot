using Raidfelden.Data.Pokemon.Entities;
using Raidfelden.Entities;

namespace Raidfelden.Data.Pokemon
{
	public class TradeRepository : GenericCastRepository<Trade, int, ITrade>, ITradeRepository
	{
		public TradeRepository(RaidfeldenContext context) : base(context)
		{
		}
	}
}
