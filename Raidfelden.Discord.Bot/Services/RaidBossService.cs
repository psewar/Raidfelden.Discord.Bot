﻿using Newtonsoft.Json;
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
		List<IRaidboss> Raidbosses { get; }
	}

	public class RaidbossService : IRaidbossService
	{
		private static List<IRaidboss> _raidbosses;
		private static readonly object Locker = new object();

        public RaidbossService()
        {
	        if (_raidbosses != null) return;
	        lock (Locker)
	        {
		        _raidbosses = new List<IRaidboss>();
		        var jsonPath = "bosses.json";
		        using (var reader = new StreamReader(jsonPath))
		        {
			        var json = reader.ReadToEnd();
			        var bosses = JsonConvert.DeserializeObject<List<Raidboss>>(json);
			        _raidbosses.AddRange(bosses);
		        }
	        }
        }

        public List<IRaidboss> Raidbosses => _raidbosses;

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
