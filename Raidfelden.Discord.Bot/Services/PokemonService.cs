﻿using Newtonsoft.Json;
using Raidfelden.Discord.Bot.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
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
		Task<Dictionary<IPokemon, double>> GetSimilarRaidbossByNameAsync(string name, int limit = int.MaxValue);
	}

	public class PokemonService : IPokemonService
	{
		protected ILocalizationService LocalizationService { get; }

		public PokemonService(IRaidbossService raidbossService, ILocalizationService localizationService)
		{
			LocalizationService = localizationService;
			RaidbossService = raidbossService;
			PokemonPerCulture = new Dictionary<CultureInfo, List<IPokemon>>();
			var pokemon = LoadFromJson(@"de.pokemon.json");
			PokemonPerCulture.Add(CultureInfo.GetCultureInfo("de-DE"), pokemon);
			pokemon = LoadFromJson(@"en.pokemon.json");
			PokemonPerCulture.Add(CultureInfo.GetCultureInfo("en-US"), pokemon);
		}

		private List<IPokemon> LoadFromJson(string jsonPath)
		{
			var result = new List<IPokemon>();
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
									foreach (var type in prop.Value)
									{
										var typeName = type.Value;
										pokemon.Types.Add(typeName);
									}
									break;
							}
						}
					}
					result.Add(pokemon);
				}
			}
			return result;
		}

		protected Dictionary<CultureInfo, List<IPokemon>> PokemonPerCulture { get; }
		protected IRaidbossService RaidbossService { get; }
		protected List<IPokemon> Pokemon => PokemonPerCulture[Thread.CurrentThread.CurrentUICulture];

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
				() => LocalizationService.Get("Pokemon_Errors_NothingFound", name, string.Empty),
				list => LocalizationService.Get("Pokemon_Errors_ToManyFound", list.Count, name, interactiveLimit, string.Empty),
				list => LocalizationService.Get("Pokemon_Errors_InteractiveMode", list.Count, name, string.Empty)
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
				() => LocalizationService.Get("Pokemon_Errors_NothingFound", name, "raidboss-"),
				list => LocalizationService.Get("Pokemon_Errors_ToManyFound", list.Count, name, interactiveLimit, "raidboss-"),
				list => LocalizationService.Get("Pokemon_Errors_InteractiveMode", list.Count, name, "raidboss-")
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

		public async Task<Dictionary<IPokemon, double>> GetSimilarRaidbossByNameAsync(string name, int limit = int.MaxValue)
		{
			var algorithm = new Levenstein();
			var rankedList =
				Pokemon.Select(e => new { Pokemon = e, Rank = algorithm.GetSimilarity(e.Name, name) })
					   .OrderByDescending(e => e.Rank)
					   .Where(e => e.Rank > 0.1f);
			var raidbosses = RaidbossService.Raidbosses.Select(e => e.Id);
			var rankedListFiltered = rankedList.Where(e => raidbosses.Contains(e.Pokemon.Id));
			return await Task.FromResult(rankedListFiltered.ToDictionary(k => k.Pokemon, v => v.Rank));
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
