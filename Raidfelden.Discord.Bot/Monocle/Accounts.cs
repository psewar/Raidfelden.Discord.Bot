using System;
using System.Collections.Generic;

namespace Raidfelden.Discord.Bot.Monocle
{
    public partial class Accounts
    {
        public int Id { get; set; }
        public string Instance { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Provider { get; set; }
        public short Level { get; set; }
        public string Model { get; set; }
        public string DeviceVersion { get; set; }
        public string DeviceId { get; set; }
        public int? Hibernated { get; set; }
        public string Reason { get; set; }
        public int? Captchaed { get; set; }
        public int? Created { get; set; }
        public int? Updated { get; set; }
        public sbyte? Remove { get; set; }
        public short? ReserveType { get; set; }
        public int? Binded { get; set; }
        public int? LastHibernated { get; set; }
    }
}
