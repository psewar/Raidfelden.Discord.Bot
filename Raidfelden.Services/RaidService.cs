using System;
using System.Globalization;
using System.Threading.Tasks;
using NodaTime;
using Raidfelden.Configuration;
using Raidfelden.Data;
using Raidfelden.Entities;
using Raidfelden.Interfaces.Entities;

namespace Raidfelden.Services
{
    public interface IRaidService
    {
        Task<ServiceResponse> AddAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, string gymName, string pokemonNameOrLevel, string timeLeft, int interactiveLimit, FenceConfiguration[] fences);

	    Task<ServiceResponse> AddResolveGymAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, int gymId, byte level, IPokemon pokemon, IRaidboss raidboss, TimeSpan timeSpan, int interactiveLimit, FenceConfiguration[] fences);

		Task<ServiceResponse> HatchAsync(Type textResource, string gymName, string pokemonName, int interactiveLimit, FenceConfiguration[] fences);
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
            var pokemon = pokemonAndRaidboss.Key;
            var raidboss = pokemonAndRaidboss.Value;
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
	        if (TimeSpan.TryParseExact(timeLeft, "m\\:ss", CultureInfo.InvariantCulture, out TimeSpan result))
	        {
		        return result;
	        }
	        return null;
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
            var pokemon = pokemonAndRaidboss.Key;
            var raidboss = pokemonAndRaidboss.Value;

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
    }
}
