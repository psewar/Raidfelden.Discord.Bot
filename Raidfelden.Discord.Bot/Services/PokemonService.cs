using Newtonsoft.Json;
using Raidfelden.Discord.Bot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimMetrics.Net.Metric;

namespace Raidfelden.Discord.Bot.Services
{
	public interface IPokemon
	{
		int Id { get; }
		string Name { get; }
	}

    public interface IPokemonService
    {
		IPokemon GetPokemonByName(string name);
		Task<ServiceResponse<IPokemon>> GetPokemonAsync(string name, int interactiveLimit, Func<string, Task<ServiceResponse>> interactiveCallbackAction);
        Task<ServiceResponse<KeyValuePair<IPokemon, IRaidboss>>> GetPokemonAndRaidbossAsync(string name, int interactiveLimit, Func<string, Task<ServiceResponse>> interactiveCallbackAction);
	    Task<Dictionary<IPokemon, double>> GetSimilarPokemonByNameAsync(string name, int limit = int.MaxValue);
    }

	public class PokemonService : IPokemonService
	{
		public PokemonService(IRaidbossService raidbossService)
		{
			Pokemon = new List<IPokemon>();
			var jsonPath = "pokemon.json";
			using (StreamReader reader = new StreamReader(jsonPath))
			{
				string json = reader.ReadToEnd();
				dynamic mons = JsonConvert.DeserializeObject(json);
				foreach (var mon in mons)
				{
					var pokemon = new Pokemon();
					var id = mon.Name;
					pokemon.Id = int.Parse(id);
					foreach (var property in mon)
					{
						foreach (var prop in property)
						{
							var key = (string)prop.Name;
							switch (key)
							{
								case "name":
									pokemon.Name = prop.Value.Value;
									break;
								case "types":
									foreach(var type in prop.Value)
									{
										var typeName = type.Value;
										pokemon.Types.Add(typeName);
									}
									break;
							}
						}
					}
					Pokemon.Add(pokemon);
				}
                RaidbossService = raidbossService;
            }
		}

        protected IRaidbossService RaidbossService { get; }
		protected List<IPokemon> Pokemon { get; set; }

		public IPokemon GetPokemonByName(string name)
		{
			return Pokemon.SingleOrDefault(e => e.Name.StartsWith(name));
		}

		public async Task<ServiceResponse<IPokemon>> GetPokemonAsync(string name, int interactiveLimit, Func<string, Task<ServiceResponse>> interactiveCallback)
		{
			return await InteractiveServiceHelper.GenericGetEntityWithCallback(
				Task.FromResult(Pokemon.Where(e => e.Name.ToLowerInvariant().Contains(name.ToLowerInvariant())).ToList()),
				list => list.Where(e => e.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList(),
				interactiveLimit,
				interactiveCallback,
				pokemon => pokemon.Name,
				pokemon => pokemon.Name,
				pokemon => pokemon.Name,
				() => $"Kein Pokemon gefunden das das Wortfragment \"{name}\" enthält. Hast du Dich eventuell vertippt?",
				list => $"{list.Count} Pokemon gefunden die das Wortfragment \"{name}\" enthalten. Bitte formuliere den Namen etwas exakter aus, maximal {interactiveLimit} dürfen übrig bleiben für den interaktiven Modus.",
				list => $"{list.Count} Pokemon gefunden die das Wortfragment \"{name}\" enthalten. Bitte wähle das passende Pokemon anhand der Nummer aus der Liste aus."
			);
		}

        public async Task<ServiceResponse<KeyValuePair<IPokemon, IRaidboss>>> GetPokemonAndRaidbossAsync(string name, int interactiveLimit, Func<string, Task<ServiceResponse>> interactiveCallback)
        {
            return await InteractiveServiceHelper.GenericGetEntityWithCallback(
				GetPossibleRaidbossPokemonAsync(name),
                list => list.Where(e => e.Key.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList(),
                interactiveLimit,
                interactiveCallback,
                pokemon => pokemon.Key.Name,
                pokemon => pokemon.Key.Name,
                pokemon => pokemon.Key.Name,
                () => $"Kein Raidboss-Pokemon gefunden, dass das Wortfragment \"{name}\" enthält. Hast du Dich eventuell vertippt?",
                list => $"{list.Count} Raidboss-Pokemon gefunden, dass das Wortfragment \"{name}\" enthalten. Bitte formuliere den Namen etwas exakter aus, maximal {interactiveLimit} dürfen übrig bleiben für den interaktiven Modus.",
                list => $"{list.Count} Raidboss-Pokemon gefunden, dass das Wortfragment \"{name}\" enthalten. Bitte wähle das passende Pokemon anhand der Nummer aus der Liste aus."
            );
        }

        public async Task<List<KeyValuePair<IPokemon, IRaidboss>>> GetPossibleRaidbossPokemonAsync(string name)
        {
            var pokemon = Pokemon.Where(e => e.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
            var pokemonAndRaidboss = pokemon.Select(e => new KeyValuePair<IPokemon, IRaidboss>(e, RaidbossService.GetRaidbossOrDefaultById(e.Id)));
            pokemonAndRaidboss = pokemonAndRaidboss.Where(e => e.Value != null);
            return await Task.FromResult(pokemonAndRaidboss.ToList());
        }

		public async Task<Dictionary<IPokemon, double>> GetSimilarPokemonByNameAsync(string name, int limit = int.MaxValue)
		{
			var algorithm = new Levenstein();
			var rankedList =
				Pokemon.Select(e => new {Pokemon = e, Rank = algorithm.GetSimilarity(e.Name, name)})
					   .OrderByDescending(e => e.Rank)
					   .Where(e => e.Rank > 0.1f)
					   .Take(limit);
			return await Task.FromResult(rankedList.ToDictionary(k => k.Pokemon, v => v.Rank));
		}
	}

	public class Pokemon : IPokemon
	{
		public Pokemon()
		{
			Types = new List<string>();
		}
		public int Id { get; set; }
		public string Name { get; set; }
		public List<string> Types { get; set; }
	}
}
