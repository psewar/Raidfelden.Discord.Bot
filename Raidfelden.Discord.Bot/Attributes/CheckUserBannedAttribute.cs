using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Raidfelden.Discord.Bot.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class CheckUserBannedAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
	        if (!(context.User is SocketGuildUser user))
	        {
		        return Task.FromResult(PreconditionResult.FromSuccess());
	        }

	        var userIsBanned = user.Roles.Any(e => e.Name.ToLower() == "infobotbanned");
	        return Task.FromResult(userIsBanned 
		        ? PreconditionResult.FromError($"User {user.Username} is banned") 
		        : PreconditionResult.FromSuccess());
        }
    }
}
