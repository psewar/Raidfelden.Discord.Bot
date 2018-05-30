using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Utilities;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore;

namespace Raidfelden.Discord.Bot.Services
{
    public interface IGymService
    {
        Task<ServiceResponse<Forts>> GetGymAsync(Hydro74000Context context, string name, int interactiveLimit, Func<int, Task<ServiceResponse>> interactiveCallbackAction, IEnumerable<FenceConfiguration> fences = null);
    }

    public class GymService : IGymService
    {
        private readonly FencesConfiguration _fencesConfiguration;

        public GymService(FencesConfiguration fencesConfiguration)
        {
            _fencesConfiguration = fencesConfiguration;
            GymsByFences = new LazyConcurrentDictionary<FenceConfiguration, int[]>();
        }

        protected LazyConcurrentDictionary<FenceConfiguration, int[]> GymsByFences { get; set; }

        public async Task<ServiceResponse<Forts>> GetGymAsync(Hydro74000Context context, string name, int interactiveLimit, Func<int, Task<ServiceResponse>> interactiveCallbackAction, IEnumerable<FenceConfiguration> fences = null)
        {
            return await InteractiveServiceHelper.GenericGetEntityWithCallback(
                GetGymsByNameAsync(context, name, fences),
                list => list.Where(e => e.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList(),
                interactiveLimit,
                interactiveCallbackAction,
                gym => gym.Id,
                gym => gym.Name,
                gym => gym.Name,
                () => $"Keine Arena gefunden die das Wortfragment \"{name}\" enthält. Hast du Dich eventuell vertippt?",
                list => $"{list.Count} Arenen gefunden die das Wortfragment \"{name}\" enthalten. Bitte formuliere den Namen etwas exakter aus, maximal {interactiveLimit} dürfen übrig bleiben für den interaktiven Modus.",
                list => $"{list.Count} Arenen gefunden die das Wortfragment \"{name}\" enthalten. Bitte wähle die passende Arena anhand der Nummer aus der Liste aus."
            );
        }

        private async Task<List<Forts>> GetGymsByNameAsync(Hydro74000Context context, string name, IEnumerable<FenceConfiguration> fences = null)
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
    }
}
