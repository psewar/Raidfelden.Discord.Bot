using System;
using System.Collections.Generic;
using System.Text;

namespace Raidfelden.Entities
{
    public interface IUser
    {
		int Id { get; set; }
		string Name { get; set; }
		double Latitude { get; set; }
		double Longitude { get; set; }
		sbyte Active { get; set; }
		ulong? DiscordId { get; set; }
		string DiscordMention { get; set; }
		sbyte IsTradeAllowed { get; set; }
	}
}
