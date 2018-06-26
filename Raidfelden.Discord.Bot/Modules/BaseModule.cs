using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using Raidfelden.Discord.Bot.Services;
using Raidfelden.Discord.Bot.Attributes;
using System.Linq;
using Discord.Addons.Interactive;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using NodaTime;
using Raidfelden.Services;
using Raidfelden.Discord.Bot.Resources;
using Raidfelden.Discord.Bot.Extensions;
using Raidfelden.Configuration;

namespace Raidfelden.Discord.Bot.Modules
{
    [CheckUserBanned]
    public abstract class BaseModule<TModule, TConfiguration> : InteractiveBase<TModule> 
        where TModule : SocketCommandContext
        where TConfiguration : ChannelConfiguration
    {
        protected BaseModule(IConfigurationService configurationService, IEmojiService emojiService, ILocalizationService localizationService) : base()
        {
            ConfigurationService = configurationService;
            EmojiService = emojiService;
	        LocalizationService = localizationService;
	        InteractiveReactionLimit = ConfigurationService.GetAppConfiguration().InteractiveReactionLimit;
	        if (InteractiveReactionLimit == 0)
	        {
		        InteractiveReactionLimit = 4;
	        }
        }

        protected override void BeforeExecute(CommandInfo command)
        {
            var guildConfiguration = ConfigurationService.GetGuildConfiguration(Context.GetGuildId());
            ChannelConfigurations = ConfigurationService.GetChannelConfigurations(guildConfiguration, Context.Channel.Name).ToArray();
	        var cultureCodeChannel = ChannelConfigurations.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.CultureCode));
	        if (cultureCodeChannel != null)
	        {
		        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureCodeChannel.CultureCode);
	        }
	        ChannelTimeZone = ConfigurationService.GetChannelDateTimeZone(guildConfiguration, Context.Channel.Name);
            base.BeforeExecute(command);
        }

		protected IConfigurationService ConfigurationService { get; }
        protected IEmojiService EmojiService { get; }
	    protected ILocalizationService LocalizationService { get; }
	    protected ChannelConfiguration[] ChannelConfigurations { get; set; }
	    protected DateTimeZone ChannelTimeZone { get; private set; }
	    protected int InteractiveReactionLimit { get; private set; }

	    protected TConfiguration ChannelConfiguration
        {
            get
            {
                return ChannelConfigurations.OfType<TConfiguration>().FirstOrDefault();
            }
        }

        protected FenceConfiguration[] Fences
        {
            get
            {
                return ConfigurationService.GetFencesConfigurationForChannel(ChannelConfiguration).ToArray();
            }
        }

        protected bool CanProcessRequest
        {
            get
            {
                if (ChannelConfiguration == null && !ConfigurationService.ShouldProcessRequestAnyway(Context.Message.Author.Username))
                {
                    return false;
                }
                return true;
            }
        }

        protected async Task ReplyWithInteractive(Func<Task<ServiceResponse>> interactiveCallback, string titleSuccess)
        {
            var result = await interactiveCallback();
            if (result.IsSuccess)
            {
				var messageBuilder = new StringBuilder(LocalizationService.Get(typeof(i18n),"Base_Messages_Reply_Success", Context.Message.Author.Mention) + Environment.NewLine);
	            messageBuilder.Append(result.Message);
                await ReplySuccessAsync(titleSuccess, messageBuilder.ToString());
            }
            else
            {
                if (result.InterActiveCallbacks != null)
                {
                    await ReplyInteractiveAsync(result, titleSuccess);
                    return;
                }
                await ReplyFailureAsync(result.Message);
            }
        }

        protected virtual async Task ReplySuccessAsync(string title, string message)
        {
            var embed = BuildEmbed(title, message, Color.Green);
            await ReplyEmbed(embed);
        }

        protected virtual async Task ReplyFailureAsync(string message)
        {
	        var embed = BuildEmbed(LocalizationService.Get(typeof(i18n), "Base_Messages_Reply_Failure"), message, Color.Red);
            await ReplyEmbed(embed);
        }

        protected virtual async Task ReplyInteractiveAsync(ServiceResponse response, string titleSuccess)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(response.Message);
            var callbackCounter = 1;
            foreach (var resultInterActiveCallback in response.InterActiveCallbacks)
            {
                var emoji = EmojiService.Get(callbackCounter);
                messageBuilder.AppendLine(emoji.Name + " " + resultInterActiveCallback.Key);
                callbackCounter++;
            }
            var embed = BuildEmbed(LocalizationService.Get(typeof(i18n), "Base_Messages_Reply_Interactive"), messageBuilder.ToString(), Color.Orange);
            var reply = new ReactionCallbackData(string.Empty, embed, TimeSpan.FromSeconds(30));
            callbackCounter = 1;
            foreach (var resultInterActiveCallback in response.InterActiveCallbacks)
            {
                var emoji = EmojiService.Get(callbackCounter);
                reply.WithCallback(emoji, c => ReplyWithInteractive(resultInterActiveCallback.Value, titleSuccess));
                callbackCounter++;
            }
            await InlineReactionReplyAsync(reply);
        }

        protected virtual Embed BuildEmbed(string title, string message, Color color)
        {
            var embed = new EmbedBuilder()
                .WithAuthor((e) => { e.Name = Context.Message.Author.Username; e.IconUrl = Context.Message.Author.GetAvatarUrl() ?? "https://cdn.discordapp.com/embed/avatars/0.png"; e.Build(); })
                .WithTitle(title)
                .WithDescription(message)
                .WithColor(color)
                .WithCurrentTimestamp()
                .Build();
            return embed;
        }

        protected virtual async Task ReplyEmbed(Embed embed)
        {
            await ReplyAsync(string.Empty, embed: embed);
        }
    }
}
