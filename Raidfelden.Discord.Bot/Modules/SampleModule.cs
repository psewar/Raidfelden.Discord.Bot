using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Raidfelden.Discord.Bot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raidfelden.Discord.Bot.Modules
{
    /// <summary>
    /// https://raw.githubusercontent.com/JanVargovsky/Discord.Addons.Interactive/cfcd84eef92b47d8a73f12c6169b8da812f4cda4/SampleApp/Modules/SampleModule.cs
    /// </summary>
    public class SampleModule : InteractiveBase
    {
        //private readonly IEmojiService _emojiService;

        //public SampleModule(IEmojiService emojiService)
        //{
        //    _emojiService = emojiService;
        //}

        // DeleteAfterAsync will send a message and asynchronously delete it after the timeout has popped
        // This method will not block.
        [Command("delete")]
        public async Task<RuntimeResult> Test_DeleteAfterAsync()
        {
            await ReplyAndDeleteAsync("this message will delete in 10 seconds", timeout: TimeSpan.FromSeconds(10));
            return Ok();
        }

        // NextMessageAsync will wait for the next message to come in over the gateway, given certain criteria
        // By default, this will be limited to messages from the source user in the source channel
        // This method will block the gateway, so it should be ran in async mode.
        [Command("next", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("What is 2+2?");
            var response = await NextMessageAsync();
            if (response != null)
                await ReplyAsync($"You replied: {response.Content}");
            else
                await ReplyAsync("You did not reply before the timeout");
        }

        // PagedReplyAsync will send a paginated message to the channel
        // You can customize the paginator by creating a PaginatedMessage object
        // You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        // This method will not block.
        [Command("paginator")]
        public async Task Test_Paginator()
        {
            var pages = new[] { "Page 1", "Page 2", "Page 3", "aaaaaa", "Page 5" };
            await PagedReplyAsync(pages);
        }

        // InlineReactionReplyAsync will send a message and adds reactions on it.
        // Once an user adds a reaction, the callback is fired.
        // If callback was successfull next callback is not handled (message is unsubscribed).
        // Unsuccessful callback is a reaction that did not have a callback.
        [Command("reaction")]
        public async Task Test_ReactionReply()
        {
            await InlineReactionReplyAsync(new ReactionCallbackData("text")
                .WithCallback(new Emoji("👍"), c => c.Channel.SendMessageAsync("You've replied with 👍"))
                .WithCallback(new Emoji("👎"), c => c.Channel.SendMessageAsync("You've replied with 👎"))
                );
        }
        [Command("embedreaction")]
        public async Task Test_EmedReactionReply()
        {
            var one = new Emoji("1⃣");
            var two = new Emoji("2⃣");

            var embed = new EmbedBuilder()
                .WithTitle("Choose one")
                .AddInlineField(one.Name, "Beer")
                .AddInlineField(two.Name, "Drink")
                .Build();

            await InlineReactionReplyAsync(new ReactionCallbackData("text", embed)
                .WithCallback(one, c => c.Channel.SendMessageAsync("Here you go :beer:"))
                .WithCallback(two, c => c.Channel.SendMessageAsync("Here you go :tropical_drink:"))
                );
        }

        [Command("options")]
        public async Task Test_OptionsReply(int count)
        {
            string[] numbers = new[] { "0⃣", "1⃣", "2⃣", "3⃣", "4⃣", "5⃣", "6⃣", "7⃣", "8⃣", "9⃣" };
            var numberEmojis = new List<Emoji>();
            for (int i = 0; i < numbers.Length; i++)
            {
                numberEmojis.Add(new Emoji(numbers[i]));
            }

            var one = new Emoji("1?");

            var reply = new ReactionCallbackData("Option Count: " + count);
            for (int i = 1; i <= count; i++)
            {
                var counter = i;
                reply.WithCallback(numberEmojis[counter], c => c.Channel.SendMessageAsync("You have choosen option: " + counter));
            }

            await InlineReactionReplyAsync(reply);
        }
    }
}