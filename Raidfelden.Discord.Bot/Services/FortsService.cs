using GeoAPI.Geometries;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Raidfelden.Discord.Bot.Services
{
    public interface IFortsService
    {
        IEnumerable<Forts> GetFortsByName(Hydro74000Context context, string name);
        IEnumerable<Forts> GetFortsByName(Hydro74000Context context, string name, IEnumerable<FenceConfiguration> fences);
    }

    public class FortsService : IFortsService
    {
        private readonly FencesConfiguration _fencesConfiguration;

        public FortsService(FencesConfiguration fencesConfiguration)
        {
            _fencesConfiguration = fencesConfiguration;
            FortsByFences = new LazyConcurrentDictionary<FenceConfiguration, int[]>();
        }

        protected LazyConcurrentDictionary<FenceConfiguration, int[]> FortsByFences { get; set; }

        public IEnumerable<Forts> GetFortsByName(Hydro74000Context context, string name)
        {
            return GetFortsByName(context, name, null);
        }

        public IEnumerable<Forts> GetFortsByName(Hydro74000Context context, string name, IEnumerable<FenceConfiguration> fences)
        {
            if (fences != null && fences.Any())
            {
                var fortIds = new List<int>();
                foreach(var fence in fences)
                {
                    var ids = FortsByFences.GetOrAdd(fence, key => GetFortIdsForFence(context, key));
                    fortIds.AddRange(ids);
                }
                fortIds = fortIds.Distinct().ToList();

                return context.Forts.Where(e => fortIds.Contains(e.Id) && e.Name.Contains(name));
            }
            else
            {
                return context.Forts.Where(e => e.Name.Contains(name));
            }
        }

        private int[] GetFortIdsForFence(Hydro74000Context context, FenceConfiguration fence)
        {
            var result = new List<int>();
            var allForts = context.Forts.ToList();
            foreach(var fort in allForts)
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
