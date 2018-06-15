using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raidfelden.Discord.Bot.Extensions
{
    public static class DiscordExtensions
    {
        public static ulong? GetGuildId(this SocketCommandContext context)
        {
            if (context.Guild ==null)
            {
                return null;
            }

            return context.Guild.Id;
        }
    }
}
