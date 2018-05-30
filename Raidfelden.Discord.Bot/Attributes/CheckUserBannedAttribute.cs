using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Raidfelden.Discord.Bot.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CheckUserBannedAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as SocketGuildUser;
            if (user != null)
            {
                var userIsBanned = user.Roles.Any(e => e.Name.ToLower() == "infobotbanned");
                if (userIsBanned)
                {
                    return Task.FromResult(PreconditionResult.FromError($"User {user.Username} is banned"));
                }
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
