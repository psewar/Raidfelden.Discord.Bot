using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Raidfelden.Configuration;
using Raidfelden.Entities;

namespace Raidfelden.Services
{
	public interface IQuest
	{
		int Id { get; set; }
		string Name { get; }
	}
	public interface IQuestEntry
	{
		int Id { get; set; }
		int PokestopId { get; set; }
		int QuestId { get; set; }
	}

	public interface IQuestService
	{
		Task<ServiceResponse> AddAsync(string pokestopName, string questNameOrAlias, int interactiveLimit, FenceConfiguration[] fences = null);
		Task<ServiceResponse> DeleteAsync(string pokestopName);
	}

	public class QuestService : IQuestService
    {
	    protected IPokestopService PokestopService { get; }

	    public QuestService(IPokestopService pokestopService)
	    {
		    PokestopService = pokestopService;
			LoadQuests();
	    }

	    private static Dictionary<CultureInfo, Dictionary<string, string>> _questsPerLanguage;
		public void LoadQuests()
	    {
			_questsPerLanguage = new Dictionary<CultureInfo, Dictionary<string, string>>();
		    var english = CultureInfo.GetCultureInfo("en-US");
		    var japanese = CultureInfo.GetCultureInfo("ja-JP");
			var french = CultureInfo.GetCultureInfo("fr-FR");
		    var spanish = CultureInfo.GetCultureInfo("es-ES");
		    var german = CultureInfo.GetCultureInfo("de-DE");
		    var italian = CultureInfo.GetCultureInfo("it-IT");
		    var korean = CultureInfo.GetCultureInfo("ko");
		    var chineseTradition = CultureInfo.GetCultureInfo("zh-Hant");
		    var portuguese = CultureInfo.GetCultureInfo("pt-PT");

			_questsPerLanguage.Add(english, new Dictionary<string, string>());
			_questsPerLanguage.Add(japanese, new Dictionary<string, string>());
			_questsPerLanguage.Add(french, new Dictionary<string, string>());
			_questsPerLanguage.Add(spanish, new Dictionary<string, string>());
			_questsPerLanguage.Add(german, new Dictionary<string, string>());
			_questsPerLanguage.Add(italian, new Dictionary<string, string>());
			_questsPerLanguage.Add(korean, new Dictionary<string, string>());
			_questsPerLanguage.Add(chineseTradition, new Dictionary<string, string>());
			_questsPerLanguage.Add(portuguese, new Dictionary<string, string>());

			var path = Path.Combine("Resources", "PogoAssets/decrypted_assets/feature-quests.txt");
		    using (StreamReader reader = new StreamReader(path))
		    {
			    // ReSharper disable once RedundantAssignment As this first line should be skipped anyway
			    var line = reader.ReadLine();
				while ((line = reader.ReadLine()) != null)
				{
					if (string.IsNullOrWhiteSpace(line))
					{
						continue;
					}
					var parts = line.Split('\t').Select(e => e.Trim('"')).ToArray();
					var key = parts[0];
					if (key.StartsWith("quest_title_") ||
					    key.StartsWith("quest_reward_") ||
					    key.StartsWith("quest_special_dialogue_"))
					{
						continue;
					}

					try
					{
						_questsPerLanguage[english].Add(parts[1], key);
						//_questsPerLanguage[japanese].Add(parts[2], key);
						_questsPerLanguage[french].Add(parts[3], key);
						_questsPerLanguage[spanish].Add(parts[4], key);
						_questsPerLanguage[german].Add(parts[5], key);
						_questsPerLanguage[italian].Add(parts[6], key);
						_questsPerLanguage[korean].Add(parts[7], key);
						_questsPerLanguage[chineseTradition].Add(parts[8], key);
						_questsPerLanguage[portuguese].Add(parts[9], key);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						throw;
					}

				}
			}
	    }

	    private Dictionary<string, string> GetQuestList()
	    {
			var quests = _questsPerLanguage[Thread.CurrentThread.CurrentUICulture];
		    if (quests == null)
		    {
			    quests = _questsPerLanguage[CultureInfo.GetCultureInfo("en-US")];
		    }
		    return quests;
	    }

	    public async Task<ServiceResponse> AddAsync(string pokestopName, string questNameOrAlias, int interactiveLimit, FenceConfiguration[] fences = null)
	    {
		    var quests = GetQuestList();
		    var quest = quests.FirstOrDefault(e => e.Key.ToLowerInvariant().Contains(questNameOrAlias.ToLowerInvariant()));
			return new ServiceResponse(true, quest.Value);
		    //throw new NotImplementedException();
		    var pokestopResult = await PokestopService.GetBySimilarNameAsync(pokestopName, fences, interactiveLimit * 2);
			if (InteractiveServiceHelper.UseInteractiveMode(pokestopResult.ToArray()))
			{ }
			throw new NotImplementedException();
		}

	    private async Task<ServiceResponse> InteractiveResolvePokestop()
	    {
		    throw new NotImplementedException();
	    }

	    private async Task<ServiceResponse> AddQuestAsync(IPokestop pokestop, string questNameOrAlias, int interactiveLimit)
	    {
			throw new NotImplementedException();
		}

	    private async Task<ServiceResponse> AddQuestToDatabaseAsync(IPokestop pokestop, IQuest quest)
	    {
			throw new NotImplementedException();
		}

	    public Task<ServiceResponse> DeleteAsync(string pokestopName)
	    {
		    throw new NotImplementedException();
	    }
    }
}
