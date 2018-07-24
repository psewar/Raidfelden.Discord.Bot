using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public class InlineReactionCallback : IReactionCallback
    {
        public RunMode RunMode => RunMode.Sync;

        public ICriterion<SocketReaction> Criterion { get; }

        public TimeSpan? Timeout { get; }

        public SocketCommandContext Context { get; }

        public IUserMessage Message { get; private set; }

        readonly InteractiveService interactive;
        readonly ReactionCallbackData data;

        public InlineReactionCallback(
            InteractiveService interactive,
            SocketCommandContext context,
            ReactionCallbackData data,
            ICriterion<SocketReaction> criterion = null)
        {
            this.interactive = interactive;
            Context = context;
            this.data = data;
            Criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            Timeout = data.Timeout ?? TimeSpan.FromSeconds(30);
        }

        public async Task DisplayAsync()
        {
            var message = await Context.Channel.SendMessageAsync(data.Text, embed: data.Embed).ConfigureAwait(false);
            Message = message;
            interactive.AddReactionCallback(message, this);

            /* We can't do this -- we run into discord's rate limiting way too fast.
            _ = Task.Run(async () =>
            {
                foreach (var item in data.Callbacks)
                    await message.AddReactionAsync(item.Reaction);
            });
            */

            if (Timeout.HasValue)
            {
                _ = Task.Delay(Timeout.Value)
                    .ContinueWith(_ => interactive.RemoveReactionCallback(message));
            }
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var reactionCallbackItem = data.Callbacks.FirstOrDefault(t => t.Reaction.Equals(reaction.Emote));
            if (reactionCallbackItem == null) {
                Console.WriteLine("Unrecognized reaction to interactive: " + reaction.Emote);        
                return false;
        }

            await reactionCallbackItem.Callback(Context);
            return true;
        }
    }
}
