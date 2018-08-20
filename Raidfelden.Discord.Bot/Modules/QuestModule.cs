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
	[Group("quests")]
    public class QuestModule : BaseModule<SocketCommandContext, QuestChannel>
	{
		protected IQuestService QuestService => ServiceFactory.Build<IQuestService>();

		public QuestModule(IServiceFactory serviceFactory) : base(serviceFactory) { }

		[Command("add")]
		public async Task AddQuestAsync(string pokestopName, string questNameOrAlias)
		{
			try
			{
				var response = await QuestService.AddAsync(pokestopName, questNameOrAlias, InteractiveReactionLimit, Fences);
				await ReplyWithInteractive(() => Task.FromResult(response), "Quest added");
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
