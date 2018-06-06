using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Utilities;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore;
using SimMetrics.Net.Metric;

namespace Raidfelden.Discord.Bot.Services
{
    public interface IGymService
    {
        Task<ServiceResponse<Forts>> GetGymAsync(Hydro74000Context context, string name, int interactiveLimit, Func<int, Task<ServiceResponse>> interactiveCallbackAction, FenceConfiguration[] fences = null);

	    Task<Dictionary<Forts, double>> GetSimilarGymsByNameAsync(Hydro74000Context context, string name, FenceConfiguration[] fences = null, int limit = int.MaxValue);
    }

    public class GymService : IGymService
    {
		protected ILocalizationService LocalizationService { get; }

		public GymService(ILocalizationService localizationService)
		{
			LocalizationService = localizationService;
			GymsByFences = new LazyConcurrentDictionary<FenceConfiguration, int[]>();
        }

        protected LazyConcurrentDictionary<FenceConfiguration, int[]> GymsByFences { get; set; }

        public async Task<ServiceResponse<Forts>> GetGymAsync(Hydro74000Context context, string name, int interactiveLimit, Func<int, Task<ServiceResponse>> interactiveCallbackAction, FenceConfiguration[] fences = null)
        {
            return await InteractiveServiceHelper.GenericGetEntityWithCallback(
                GetGymsByNameAsync(context, name, fences),
                list => list.Where(e => e.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList(),
                interactiveLimit,
                interactiveCallbackAction,
                gym => gym.Id,
                gym => gym.Name,
                gym => gym.Name,
                () => LocalizationService.Get("Gyms_Errors_NothingFound", name),
                list => LocalizationService.Get("Gyms_Errors_ToManyFound", list.Count, name, interactiveLimit),
                list => LocalizationService.Get("Gyms_Errors_InteractiveMode", list.Count, name)
            );
        }

        private async Task<List<Forts>> GetGymsByNameAsync(Hydro74000Context context, string name, FenceConfiguration[] fences = null)
        {
            Dictionary<string, int> aliasCache = new Dictionary<string, int>();
            aliasCache.Add("spital", 37);
            aliasCache.Add("reha", 36);

            if (aliasCache.ContainsKey(name.ToLower()))
            {
                var gymId = aliasCache[name.ToLower()];
                return await Task.FromResult(context.Forts.Where(e => e.Id == gymId).ToList());
            }
            else
            {
                if (fences != null && fences.Any())
                {
                    var fortIds = new List<int>();
                    foreach (var fence in fences)
                    {
                        var ids = GymsByFences.GetOrAdd(fence, key => GetFortIdsForFence(context, key));
                        fortIds.AddRange(ids);
                    }
                    fortIds = fortIds.Distinct().ToList();

                    return await context.Forts.Where(e => fortIds.Contains(e.Id) && e.Name.Contains(name)).ToListAsync();
                }
                else
                {
                    return await context.Forts.Where(e => e.Name.Contains(name)).ToListAsync();
                }
            }
        }

        private int[] GetFortIdsForFence(Hydro74000Context context, FenceConfiguration fence)
        {
            var result = new List<int>();
            var allForts = context.Forts.ToList();
            foreach (var fort in allForts)
            {
                var coordinate = new Coordinate(fort.Lat.Value, fort.Lon.Value);
                if (fence.Area.Contains(coordinate))
                {
                    result.Add(fort.Id);
                }
            }
            return result.ToArray();
        }

	    private IQueryable<Forts> GetGyms(Hydro74000Context context, FenceConfiguration[] fences = null)
	    {
		    if (fences != null && fences.Any())
			{
				var fortIds = new List<int>();
				foreach (var fence in fences)
				{
					var ids = GymsByFences.GetOrAdd(fence, key => GetFortIdsForFence(context, key));
					fortIds.AddRange(ids);
				}
				fortIds = fortIds.Distinct().ToList();

				return context.Forts.Where(e => fortIds.Contains(e.Id));
			}
		    return context.Forts;
	    }

		public async Task<Dictionary<Forts, double>> GetSimilarGymsByNameAsync(Hydro74000Context context, string name, FenceConfiguration[] fences = null, int limit = int.MaxValue)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return new Dictionary<Forts, double>();
			}
			var algorithm = new Levenstein();
			var gyms = GetGyms(context, fences);
			var rankedList =
				gyms.Select(e => new { Gym = e, Rank = algorithm.GetSimilarity(e.Name, name) })
					   .OrderByDescending(e => e.Rank)
					   .Where(e => e.Rank > 0.5f)
					   .Take(limit);
			return await Task.FromResult(rankedList.ToDictionary(k => k.Gym, v => v.Rank));
		}
	}
}
