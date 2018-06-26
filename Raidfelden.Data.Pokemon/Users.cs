using System;
using System.Collections.Generic;
using Raidfelden.Entities;

namespace Raidfelden.Data.Pokemon
{
    public partial class User : IUser
    {
        public User()
        {
            Trades = new HashSet<Trade>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public sbyte Active { get; set; }
        public ulong? DiscordId { get; set; }
        public string DiscordMention { get; set; }
        public sbyte IsTradeAllowed { get; set; }

        public ICollection<Trade> Trades { get; set; }
    }
}
