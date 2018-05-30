using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raidfelden.Discord.Bot.Attributes
{
    public class RequireRoleAttribute : RequireContextAttribute
    {
        private ulong[] _roleIds;
        private string[] _roleNames;

        /// <summary> Requires that the command caller has ANY of the supplied role ids. </summary>
        public RequireRoleAttribute(params ulong[] roleIds) : base(ContextType.Guild)
            => _roleIds = roleIds;
        /// <summary> Requires that the command caller has ANY of the supplied role names. </summary>
        public RequireRoleAttribute(params string[] roleNames) : base(ContextType.Guild)
            => _roleNames = roleNames;

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var allowedRoleIds = new List<ulong>();

            if (_roleIds != null)
                allowedRoleIds.AddRange(_roleIds);
            if (_roleNames != null)
                allowedRoleIds.AddRange(context.Guild.Roles.Where(x => _roleNames.Contains(x.Name)).Select(x => x.Id));

            return (context.User as IGuildUser).RoleIds.Intersect(allowedRoleIds).Any()
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError("You do not have a role required to execute this command."));
        }
    }
}
