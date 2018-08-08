using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using Geocoding;
using Geocoding.Google;
using Microsoft.EntityFrameworkCore;
using Raidfelden.Configuration;
using Raidfelden.Data;
using Raidfelden.Entities;
using SimMetrics.Net.Metric;

namespace Raidfelden.Services
{
    public interface IGymService
	{
		Task<IGym> GetGymByIdAsync(int id);
			
		Task<ServiceResponse<IGym>> GetGymAsync(Type textResource, string name, int interactiveLimit, Func<int, Task<ServiceResponse>> interactiveCallbackAction, FenceConfiguration[] fences = null);

		Task<Dictionary<IGym, double>> GetSimilarGymsByNameAsync(string name, FenceConfiguration[] fences = null, int limit = int.MaxValue);

		Task<string> GetGymNameWithAdditionAsync(IGym gym, List<IGym> gymList);

		Task UpdateGymAsync(IGym gym);
	}

	public class GymService : IGymService
	{
		protected IGymRepository GymRepository { get; }
		protected IFortSightingRepository FortSightingRepository { get; }
		protected ILocalizationService LocalizationService { get; }
		protected IConfigurationService ConfigurationService { get; }
		

		public GymService(IGymRepository gymRepository, IFortSightingRepository fortSightingRepository, ILocalizationService localizationService, IConfigurationService configurationService)
		{
			GymRepository = gymRepository;
			FortSightingRepository = fortSightingRepository;
			LocalizationService = localizationService;
			ConfigurationService = configurationService;			
			GymsByFences = new LazyConcurrentDictionary<FenceConfiguration, int[]>();
		}

		protected LazyConcurrentDictionary<FenceConfiguration, int[]> GymsByFences { get; set; }

		public async Task<IGym> GetGymByIdAsync(int id)
		{
			return await GymRepository.GetAsync(id);
		}

		public async Task<ServiceResponse<IGym>> GetGymAsync(Type textResource, string name, int interactiveLimit, Func<int, Task<ServiceResponse>> interactiveCallbackAction, FenceConfiguration[] fences = null)
		{
			return await InteractiveServiceHelper.GenericGetEntityWithCallback(
				GetGymsByNameAsync(name, fences),
				list => list.Where(e => e.Name.ToLowerInvariant().Trim() == name.ToLowerInvariant().Trim()).ToList(),
				interactiveLimit,
				interactiveCallbackAction,
				gym => gym.Id,
				GetGymNameWithAdditionAsync,
				gym => gym.Name,
				() => LocalizationService.Get(textResource, "Gyms_Errors_NothingFound", name),
				list => LocalizationService.Get(textResource, "Gyms_Errors_ToManyFound", list.Count, name, interactiveLimit),
				list => LocalizationService.Get(textResource, "Gyms_Errors_InteractiveMode", list.Count, name)
			);
		}

		public async Task<string> GetGymNameWithAdditionAsync(IGym gym, List<IGym> gymList)
		{
			// Check if there is only one gym wth this name
			if (gymList.Count(e => e.Name.Trim() == gym.Name.Trim()) == 1)
			{
				return gym.Name;
			}

			string nameAddition = await GetLocationNameFromLocationAsync(gym);
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(gym.Name);
			stringBuilder.Append(" (");
			stringBuilder.Append(nameAddition);
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		private static readonly LazyConcurrentDictionary<KeyValuePair<int, string>, string> LocationLookupCache = new LazyConcurrentDictionary<KeyValuePair<int, string>, string>();

		private async Task<string> GetLocationNameFromLocationAsync(IGym gym)
		{
			var languageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
			Console.WriteLine("GetLocationNameFromLocationAsync enter with: " + languageCode);
			if (LocationLookupCache.TryGetValue(new KeyValuePair<int, string>(gym.Id, languageCode), out string value))
			{
				return value;
			}

			var fallback = string.Concat(gym.Latitude, ", ", gym.Longitude);
			Console.WriteLine("GLNFL: Fallback defined as " + fallback);
			var config = ConfigurationService.GetAppConfiguration();
			if (config.GoogleMapsApiKeys == null)
			{
				Console.WriteLine("GLNFL: No GoogleMapsApiKeys Config Section found");
				return fallback;
			}
			var apiKey = config.GoogleMapsApiKeys.FirstOrDefault();
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				Console.WriteLine("GLNFL: No Api-Key found");
				return fallback;
			}
			IGeocoder geocoder = new GoogleGeocoder { ApiKey = apiKey, Language = languageCode };
			var addresses = await geocoder.ReverseGeocodeAsync(gym.Latitude, gym.Longitude);
			if (addresses == null)
			{
				Console.WriteLine("GLNFL: Google Api returend null");
				return fallback;
			}
			var address = addresses.FirstOrDefault();
			if (address == null)
			{
				Console.WriteLine("GLNFL: Google Api returend no addresses");
				return fallback;
			}

			LocationLookupCache.AddOrUpdate(new KeyValuePair<int, string>(gym.Id, languageCode), address.FormattedAddress, i => address.FormattedAddress);
			return address.FormattedAddress;
		}

		private async Task<List<IGym>> GetGymsByNameAsync(string name, FenceConfiguration[] fences = null)
		{
			Dictionary<string, int> aliasCache = new Dictionary<string, int>();
			aliasCache.Add("spital", 37);
			aliasCache.Add("reha", 36);

			if (aliasCache.ContainsKey(name.ToLower()))
			{
				var gymId = aliasCache[name.ToLower()];
				return await Task.FromResult(GymRepository.FindAll(e => e.Id == gymId).ToList());
			}
			else
			{
				if (fences != null && fences.Any())
				{
					var fortIds = new List<int>();
					foreach (var fence in fences)
					{
						var ids = GymsByFences.GetOrAdd(fence, GetFortIdsForFence);
						fortIds.AddRange(ids);
					}
					fortIds = fortIds.Distinct().ToList();

					return await GymRepository.FindAll(e => fortIds.Contains(e.Id) && e.Name.Contains(name)).ToListAsync();
				}
				else
				{
					return await GymRepository.FindAll(e => e.Name.Contains(name)).ToListAsync();
				}
			}
		}

		private int[] GetFortIdsForFence(FenceConfiguration fence)
		{
			var result = new List<int>();
			var allGyms = GymRepository.GetAll().ToList();
			foreach (var gym in allGyms)
			{
				var coordinate = new Coordinate(gym.Latitude, gym.Longitude);
				if (fence.Area.Contains(coordinate))
				{
					result.Add(gym.Id);
				}
			}
			return result.ToArray();
		}

		private IQueryable<IGym> GetGyms(FenceConfiguration[] fences = null)
		{
			if (fences != null && fences.Any())
			{
				var fortIds = new List<int>();
				foreach (var fence in fences)
				{
					var ids = GymsByFences.GetOrAdd(fence, GetFortIdsForFence);
					fortIds.AddRange(ids);
				}
				fortIds = fortIds.Distinct().ToList();

				return GymRepository.FindAll(e => fortIds.Contains(e.Id));
			}
			return GymRepository.GetAll();
		}

		public async Task<Dictionary<IGym, double>> GetSimilarGymsByNameAsync(string name, FenceConfiguration[] fences = null, int limit = int.MaxValue)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return new Dictionary<IGym, double>();
			}
			var algorithm = new JaroWinkler();
			var gyms = GetGyms(fences);
			var rankedList =
				gyms.Select(e => new { Gym = e, Rank = algorithm.GetSimilarity(TrimString(e.Name), TrimString(name)) })
					   .OrderByDescending(e => e.Rank)
					   .Where(e => e.Rank > 0.5f)
					   .Take(limit);
			return await Task.FromResult(rankedList.ToDictionary(k => k.Gym, v => v.Rank));
		}

		private static string TrimString(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return value;
			}
			return value.Trim();
		}

		public async Task UpdateGymAsync(IGym gym)
		{
			var fortSightings = await FortSightingRepository.FindAll(e => e.FortId == gym.Id).Take(1).ToListAsync();
			//Yuck -- we only store unix time as an int in this table!
			int now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			if (fortSightings == null || fortSightings.Count == 0)
			{
				var fortSighting = FortSightingRepository.CreateInstance();
				fortSighting.FortId = gym.Id;
				fortSighting.Team = 0;
				fortSighting.LastModified = now;
				fortSighting.Updated = now;
				FortSightingRepository.Add(fortSighting);
			}
			else
			{
				var fortSighting = fortSightings[0];
				fortSighting.LastModified = now;
				fortSighting.Updated = now;
			}
			await FortSightingRepository.SaveAsync();
		}
	}
}
