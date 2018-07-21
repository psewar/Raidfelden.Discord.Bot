using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Raidfelden.Configuration;
using Raidfelden.Discord.Bot.Resources;
using Raidfelden.Discord.Bot.Services;
using Raidfelden.Services;
using Raidfelden.Services.Extensions;

namespace Raidfelden.Discord.Bot.Modules
{
	[Group("users")]
    public class UserModule : BaseModule<SocketCommandContext, RaidChannel>
	{
		protected IUserService UserService { get; }

		public UserModule(IUserService userService, IConfigurationService configurationService, IEmojiService emojiService, ILocalizationService localizationService) : base(configurationService, emojiService, localizationService)
		{
			UserService = userService;
		}

		[Command("register")]
		public async Task RegisterAsync(string trainerName, double latitude, double longitude, string friendshipCode)
		{
			try
			{
				var response = await UserService.RegisterAsync(trainerName, latitude, longitude, friendshipCode, Context.Message.Author.Id, Context.Message.Author.Mention);
				await ReplyWithInteractive(() => Task.FromResult(response as ServiceResponse), "Registered");
			}
			catch (Exception ex)
			{
				var innerstEx = ex.GetInnermostException();
				Console.WriteLine(innerstEx.Message);
				Console.WriteLine(innerstEx.StackTrace);
				await ReplyFailureAsync(LocalizationService.Get(typeof(i18n), "Raids_Errors_Unexpected", innerstEx.Message));
			}
		}
	}
}
