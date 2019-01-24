using Discord.Commands;

namespace Raidfelden.Discord.Bot.Extensions
{
    public static class DiscordExtensions
    {
        public static ulong? GetGuildId(this SocketCommandContext context)
        {
	        return context.Guild?.Id;
        }
    }
}
