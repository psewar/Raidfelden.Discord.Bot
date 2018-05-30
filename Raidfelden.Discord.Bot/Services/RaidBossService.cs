using Newtonsoft.Json;
using Raidfelden.Discord.Bot.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Raidfelden.Discord.Bot.Services
{
    public interface IRaidbossService
    {
        IRaidboss GetRaidbossById(int id);
        IRaidboss GetRaidbossOrDefaultById(int id);
    }

	public class RaidbossService : IRaidbossService
	{
        public RaidbossService()
        {
            Raidbosses = new List<IRaidboss>();
            var jsonPath = "bosses.json";
            using (StreamReader reader = new StreamReader(jsonPath))
            {
                string json = reader.ReadToEnd();
                var bosses = JsonConvert.DeserializeObject<List<Raidboss>>(json);
                Raidbosses.AddRange(bosses);
            }
        }

        protected List<IRaidboss> Raidbosses { get; set; }

        public IRaidboss GetRaidbossById(int id)
        {
            return Raidbosses.First(e => e.Id == id);
        }

        public IRaidboss GetRaidbossOrDefaultById(int id)
        {
            return Raidbosses.FirstOrDefault(e => e.Id == id);
        }
    }
}
