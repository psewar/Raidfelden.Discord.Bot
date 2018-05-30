using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raidfelden.Discord.Bot.Modules
{
    public class Ping : ModuleBase<SocketCommandContext>
    {
		[Command("ping")]
		public async Task PingAsync()
		{
			await ReplyAsync("pong");
		}
    }
}
