using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using Raidfelden.Configuration;
using Raidfelden.Data;
using Raidfelden.Entities;

namespace Raidfelden.Services
{
    public interface IRaidService
    {
        Task<ServiceResponse> AddAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, string gymName, string pokemonNameOrLevel, string timeLeft, int interactiveLimit, FenceConfiguration[] fences);

	    Task<ServiceResponse> AddResolveGymAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, int gymId, byte level, IPokemon pokemon, IRaidboss raidboss, TimeSpan timeSpan, int interactiveLimit, FenceConfiguration[] fences);

		Task<ServiceResponse> HatchAsync(Type textResource, string gymName, string pokemonName, int interactiveLimit, FenceConfiguration[] fences);

	    Task<ServiceResponse> GetRaidList(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone,
		    string pokemonNameOrLevel, FenceConfiguration[] fences, string orderType, int interactiveLimit,
		    Func<RaidListInfo, string> formatResult);

    }

    public class RaidService : IRaidService
    {
		protected ILocalizationService LocalizationService { get; }
		protected IRaidRepository RaidRepository { get; }
        protected IGymService GymService { get; }
        protected IPokemonService PokemonService { get; }
        protected IRaidbossService RaidbossService { get; }

        public RaidService(IRaidRepository raidRepository, IGymService gymService, IPokemonService pokemonService, IRaidbossService raidbossService, ILocalizationService localizationService)
        {
	        RaidRepository = raidRepository;
	        LocalizationService = localizationService;
            GymService = gymService;
            PokemonService = pokemonService;
            RaidbossService = raidbossService;
        }

        #region Add

        public async Task<ServiceResponse> AddAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, string gymName, string pokemonNameOrLevel, string timeLeft, int interactiveLimit, FenceConfiguration[] fences)
        {
			var startEndTime = GetTimeSpan(timeLeft);
            if (!startEndTime.HasValue) { return new ServiceResponse(false, LocalizationService.Get(textResource, "Raids_Errors_TimeFormat")); }

            return await AddResolvePokemonOrLevelAsync(textResource, requestStartInUtc, userZone, gymName, pokemonNameOrLevel, startEndTime.Value, interactiveLimit, fences);
        }

        private async Task<ServiceResponse> AddResolvePokemonOrLevelAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, string gymName, string pokemonNameOrLevel, TimeSpan timeSpan, int interactiveLimit, FenceConfiguration[] fences)
        {
            if (int.TryParse(pokemonNameOrLevel, out int raidLevel))
            {
                if (raidLevel < 1)
                {
					return new ServiceResponse(false, LocalizationService.Get(textResource, "Raids_Errors_LevelToLow"));
				}
                if (raidLevel > 5)
                {
                    return new ServiceResponse(false, LocalizationService.Get(textResource, "Raids_Errors_LevelToHigh"));
                }
                return await AddResolveGymAsync(textResource, requestStartInUtc, userZone, gymName, Convert.ToByte(raidLevel), null, null, timeSpan, interactiveLimit, fences);
            }

            var pokemonResponse = await PokemonService.GetPokemonAndRaidbossAsync(textResource, pokemonNameOrLevel, interactiveLimit, (selectedPokemonName) => AddResolvePokemonOrLevelAsync(textResource, requestStartInUtc, userZone, gymName, selectedPokemonName, timeSpan, interactiveLimit, fences));
            if (!pokemonResponse.IsSuccess) { return pokemonResponse; }

            var pokemonAndRaidboss = pokemonResponse.Result;
            var pokemon = pokemonAndRaidboss.Pokemon;
            var raidboss = pokemonAndRaidboss.Raidboss;
            return await AddResolveGymAsync(textResource, requestStartInUtc, userZone, gymName, Convert.ToByte(raidboss.Level), pokemon, raidboss, timeSpan, interactiveLimit, fences);
        }

        private async Task<ServiceResponse> AddResolveGymAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, string gymName, byte level, IPokemon pokemon, IRaidboss raidboss, TimeSpan timeSpan, int interactiveLimit, FenceConfiguration[] fences)
        {
            var gymResponse = await GymService.GetGymAsync(textResource, gymName, interactiveLimit, (selectedGymId) => AddResolveGymAsync(textResource, requestStartInUtc, userZone, selectedGymId, level, pokemon, raidboss, timeSpan, interactiveLimit, fences), fences);
            if (!gymResponse.IsSuccess) { return gymResponse; }

            return await AddSaveAsync(textResource, requestStartInUtc, userZone, gymResponse.Result, level, pokemon, raidboss, timeSpan);
        }

        public async Task<ServiceResponse> AddResolveGymAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, int gymId, byte level, IPokemon pokemon, IRaidboss raidboss, TimeSpan timeSpan, int interactiveLimit, FenceConfiguration[] fences)
        {
            var gym = await GymService.GetGymByIdAsync(gymId);
            return await AddSaveAsync(textResource, requestStartInUtc, userZone, gym, level, pokemon, raidboss, timeSpan);
        }

	    private readonly Duration _eggDuration = Duration.FromMinutes(60);
		private readonly Duration _raidDuration = Duration.FromMinutes(45);

        private async Task<ServiceResponse> AddSaveAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, IGym gym, byte level, IPokemon pokemon, IRaidboss raidboss, TimeSpan timeSpan)
        {
	        var utcNowAfterProcessing = SystemClock.Instance.GetCurrentInstant().InUtc();
	        var processingTime = utcNowAfterProcessing.Minus(requestStartInUtc);
	        var durationMinusProcessing = Duration.FromTimeSpan(timeSpan).Minus(processingTime);

	        var expiry = utcNowAfterProcessing.Plus(durationMinusProcessing).ToInstant();

            // Create the raid entry
            var beforeSpawnTime = utcNowAfterProcessing.Minus(Duration.FromMinutes(105)).ToInstant().ToUnixTimeSeconds();
            var raid = await RaidRepository.FindAsync(e => e.FortId == gym.Id && e.TimeSpawn > beforeSpawnTime);
            if (raid == null)
            {
	            raid = RaidRepository.CreateInstance();
	            raid.ExternalId = ThreadLocalRandom.NextLong();
	            raid.FortId = gym.Id;
				RaidRepository.Add(raid);
            }

            string message;
            if (raidboss == null)
            {
                raid.Level = level;
                raid.TimeSpawn = (int)expiry.Minus(_eggDuration).ToUnixTimeSeconds();
                raid.TimeBattle = (int)expiry.ToUnixTimeSeconds();
                raid.TimeEnd = (int)expiry.Plus(_raidDuration).ToUnixTimeSeconds();
	            message = LocalizationService.Get(textResource, "Raids_Messages_EggAdded", level, gym.Name, FormatExpiry(expiry, userZone));
            }
            else
            {
                raid.PokemonId = (short)raidboss.Id;
                raid.Level = level;
                raid.TimeSpawn = (int)expiry.Minus(Duration.FromMinutes(105)).ToUnixTimeSeconds();
                raid.TimeBattle = (int)expiry.Minus(Duration.FromMinutes(45)).ToUnixTimeSeconds();
                raid.TimeEnd = (int)expiry.ToUnixTimeSeconds();
				message = LocalizationService.Get(textResource, "Raids_Messages_BossAdded", pokemon.Name, gym.Name, FormatExpiry(expiry, userZone));
			}

            await RaidRepository.SaveAsync();
            return new ServiceResponse(true, message);
        }

        private TimeSpan? GetTimeSpan(string timeLeft)
        {
	        if (!timeLeft.Contains(":"))
	        {
		        timeLeft = string.Concat(timeLeft, ":00");
	        }
			
	        var timeParts = timeLeft.Split(':');
			if (timeParts.Length != 2)
			{
				return null;
			}
			if (!int.TryParse(timeParts[0], out int minutes))
	        {
				return null;
			}
			if (!int.TryParse(timeParts[1], out int seconds))
			{
				return null;
			}

	        return new TimeSpan(0, minutes, seconds);
        }

	    private string FormatExpiry(Instant instant, DateTimeZone userZone)
	    {
		    var userTime = instant.InZone(userZone);
			return userTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
	    }

        #endregion

        #region Hatch

        public async Task<ServiceResponse> HatchAsync(Type textResource, string gymName, string pokemonName, int interactiveLimit, FenceConfiguration[] fences)
        {
            var pokemonResponse = await PokemonService.GetPokemonAndRaidbossAsync(textResource, pokemonName, interactiveLimit, (selectedPokemonName) => HatchAsync(textResource, gymName, selectedPokemonName, interactiveLimit, fences));
            if (!pokemonResponse.IsSuccess) { return pokemonResponse; }

            var pokemonAndRaidboss = pokemonResponse.Result;
            var pokemon = pokemonAndRaidboss.Pokemon;
            var raidboss = pokemonAndRaidboss.Raidboss;

            return await HatchResolveGymAsync(textResource, gymName, pokemon, raidboss, interactiveLimit, fences);
        }

        public async Task<ServiceResponse> HatchResolveGymAsync(Type textResource, string gymName, IPokemon pokemon, IRaidboss raidboss, int interactiveLimit, FenceConfiguration[] fences)
        {
            var gymResponse = await GymService.GetGymAsync(textResource, gymName, interactiveLimit, (selectedGymId) => HatchResolveGymWithIdAsync(textResource, selectedGymId, pokemon, raidboss, interactiveLimit), fences);
            if (!gymResponse.IsSuccess) { return gymResponse; }

            return await HatchSaveAsync(textResource, gymResponse.Result, pokemon, raidboss, interactiveLimit);
        }

        public async Task<ServiceResponse> HatchResolveGymWithIdAsync(Type textResource, int gymId, IPokemon pokemon, IRaidboss raidboss, int interactiveLimit)
        {
            var gym = await GymService.GetGymByIdAsync(gymId);
            return await HatchSaveAsync(textResource, gym, pokemon, raidboss, interactiveLimit);
        }

        public async Task<ServiceResponse> HatchSaveAsync(Type textResource, IGym gym, IPokemon pokemon, IRaidboss raidboss, int interactiveLimit)
        {
            var beforeSpawnTime = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromMinutes(90)).ToUnixTimeSeconds();
            var raid = await RaidRepository.FindAsync(e => e.FortId == gym.Id && e.TimeSpawn > beforeSpawnTime);
            if (raid == null)
            {
				return new ServiceResponse(false, LocalizationService.Get(textResource, "Raids_Errors_Hatch_NoEntryFound", gym.Name));
			}

            raid.PokemonId = (short)raidboss.Id;
            await RaidRepository.SaveAsync();
			return new ServiceResponse(true, LocalizationService.Get(textResource, "Raids_Messages_BossHatched", pokemon.Name, gym.Name));
		}

		#endregion

		#region List

	    public async Task<ServiceResponse> GetRaidList(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, string pokemonNameOrLevel, FenceConfiguration[] fences, string orderType, int interactiveLimit, Func<RaidListInfo, string> formatResult)
	    {
		    var order = GetRaidListOrder(orderType);
		    var startTime = Instant.FromDateTimeUtc(new DateTime(2018, 5, 1, 0, 0, 0).ToUniversalTime()).ToUnixTimeSeconds();
			if (int.TryParse(pokemonNameOrLevel, out int raidLevel))
		    {
			    if (raidLevel < 1)
			    {
				    return new ServiceResponse<RaidListInfo>(false, LocalizationService.Get(textResource, "Raids_Errors_LevelToLow"), null);
			    }
			    if (raidLevel > 5)
			    {
				    return new ServiceResponse<RaidListInfo>(false, LocalizationService.Get(textResource, "Raids_Errors_LevelToHigh"), null);
			    }

			    // Query Level
			    var raids = await RaidRepository.FindAllWithGymsAsync(e => e.TimeSpawn > startTime && e.Level == raidLevel);
			    return await GetListResult(raidLevel, null, raids, order, formatResult);
		    }
		    else
		    {
			    var pokemonResponse = await PokemonService.GetPokemonAndRaidbossAsync(textResource, pokemonNameOrLevel,
				    interactiveLimit, 
				    (selectedPokemonName) =>
						GetRaidList(textResource, requestStartInUtc, userZone, selectedPokemonName, fences, orderType, interactiveLimit, formatResult));
			    if (!pokemonResponse.IsSuccess)
			    {
				    return pokemonResponse;
			    }

			    var pokemonAndRaidboss = pokemonResponse.Result;
			    var raidboss = pokemonAndRaidboss.Raidboss;
			    // Query Raidboss
			    var raids = await RaidRepository.FindAllWithGymsAsync(e => e.TimeSpawn > startTime && e.PokemonId == raidboss.Id);
			    return await GetListResult(raidboss.Level, pokemonAndRaidboss, raids, order, formatResult);
		    }
	    }

		private RaidListOrder GetRaidListOrder(string order)
	    {
		    switch (order.ToLowerInvariant())
		    {
				case "time":
					return RaidListOrder.StartTime;
				case "distance":
					return RaidListOrder.Distance;
				default:
					return RaidListOrder.None;			
		    }
	    }

	    private async Task<ServiceResponse> GetListResult(int level, RaidbossPokemon raidbossPokemon, List<IRaid> raids, RaidListOrder order, Func<RaidListInfo, string> formatResult)
	    {
		    var raidListInfo = new RaidListInfo
		    {
			    Level = level,
			    RaidbossPokemon = raidbossPokemon,
			    Raids = raids,
			    Order = order
		    };

		    switch (order)
		    {
				case RaidListOrder.StartTime:
					raidListInfo.Raids = raidListInfo.Raids.OrderBy(e => e.TimeSpawn).ToList();
				    break;
		    }

		    var result = formatResult(raidListInfo);
		    return await Task.FromResult(new ServiceResponse<RaidListInfo>(true, result, raidListInfo));
	    }

		#endregion
	}

	public class RaidListInfo
	{
		public int Level { get; set; }
		public RaidbossPokemon RaidbossPokemon { get; set; }
		public List<IRaid> Raids { get; set; }
		public RaidListOrder Order { get; set; }
	}

	public enum RaidListOrder
	{
		None,
		StartTime,
		Distance,
	}
}
