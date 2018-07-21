namespace Raidfelden.Entities
{
    public interface IUser
    {
		int Id { get; set; }
		string Name { get; set; }
		double Latitude { get; set; }
		double Longitude { get; set; }
		bool Active { get; set; }
		ulong? DiscordId { get; set; }
		string DiscordMention { get; set; }
	    bool IsTradeAllowed { get; set; }
		string FriendshipCode { get; set; }
	}
}
