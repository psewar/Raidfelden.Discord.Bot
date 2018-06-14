using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Models;
using Raidfelden.Discord.Bot.Configuration;

namespace Raidfelden.Discord.Bot.Services
{
    public interface IRaidService
    {
        Task<ServiceResponse> AddAsync(ZonedDateTime requestStartInUtc, DateTimeZone userZone, string gymName, string pokemonNameOrLevel, string timeLeft, int interactiveLimit, FenceConfiguration[] fences);

	    Task<ServiceResponse> AddResolveGymAsync(ZonedDateTime requestStartInUtc, DateTimeZone userZone, int gymId, byte level, IPokemon pokemon, IRaidboss raidboss, TimeSpan timeSpan, int interactiveLimit, FenceConfiguration[] fences);

		Task<ServiceResponse> HatchAsync(string gymName, string pokemonName, int interactiveLimit, FenceConfiguration[] fences);
    }

    public class RaidService : IRaidService
    {
		protected ILocalizationService LocalizationService { get; }
		protected Hydro74000Context Context { get; }
        protected IGymService GymService { get; }
        protected IPokemonService PokemonService { get; }
        protected IRaidbossService RaidbossService { get; }

        public RaidService(Hydro74000Context context, IGymService gymService, IPokemonService pokemonService, IRaidbossService raidbossService, ILocalizationService localizationService)
        {
	        LocalizationService = localizationService;
			Context = context;
            GymService = gymService;
            PokemonService = pokemonService;
            RaidbossService = raidbossService;
        }

        #region Add

        public async Task<ServiceResponse> AddAsync(ZonedDateTime requestStartInUtc, DateTimeZone userZone, string gymName, string pokemonNameOrLevel, string timeLeft, int interactiveLimit, FenceConfiguration[] fences)
        {
			var startEndTime = GetTimeSpan(timeLeft);
            if (!startEndTime.HasValue) { return new ServiceResponse(false, LocalizationService.Get("Raids_Errors_TimeFormat")); }

            return await AddResolvePokemonOrLevelAsync(requestStartInUtc, userZone, gymName, pokemonNameOrLevel, startEndTime.Value, interactiveLimit, fences);
        }

        private async Task<ServiceResponse> AddResolvePokemonOrLevelAsync(ZonedDateTime requestStartInUtc, DateTimeZone userZone, string gymName, string pokemonNameOrLevel, TimeSpan timeSpan, int interactiveLimit, FenceConfiguration[] fences)
        {
            if (int.TryParse(pokemonNameOrLevel, out int raidLevel))
            {
                if (raidLevel < 1)
                {
					//return new ServiceResponse(false, "Der kleinste zulässige Wert für einen Raid-Level beträgt 1.");
					return new ServiceResponse(false, LocalizationService.Get("Raids_Errors_LevelToLow"));
				}
                if (raidLevel > 5)
                {
                    return new ServiceResponse(false, LocalizationService.Get("Raids_Errors_LevelToHigh"));
                }
                return await AddResolveGymAsync(requestStartInUtc, userZone, gymName, Convert.ToByte(raidLevel), null, null, timeSpan, interactiveLimit, fences);
            }

            var pokemonResponse = await PokemonService.GetPokemonAndRaidbossAsync(pokemonNameOrLevel, interactiveLimit, (selectedPokemonName) => AddResolvePokemonOrLevelAsync(requestStartInUtc, userZone, gymName, selectedPokemonName, timeSpan, interactiveLimit, fences));
            if (!pokemonResponse.IsSuccess) { return pokemonResponse; }

            var pokemonAndRaidboss = pokemonResponse.Result;
            var pokemon = pokemonAndRaidboss.Key;
            var raidboss = pokemonAndRaidboss.Value;
            return await AddResolveGymAsync(requestStartInUtc, userZone, gymName, Convert.ToByte(raidboss.Level), pokemon, raidboss, timeSpan, interactiveLimit, fences);
        }

        private async Task<ServiceResponse> AddResolveGymAsync(ZonedDateTime requestStartInUtc, DateTimeZone userZone, string gymName, byte level, IPokemon pokemon, IRaidboss raidboss, TimeSpan timeSpan, int interactiveLimit, FenceConfiguration[] fences)
        {
            var gymResponse = await GymService.GetGymAsync(Context, gymName, interactiveLimit, (selectedGymId) => AddResolveGymAsync(requestStartInUtc, userZone, selectedGymId, level, pokemon, raidboss, timeSpan, interactiveLimit, fences), fences);
            if (!gymResponse.IsSuccess) { return gymResponse; }

            return await AddSaveAsync(requestStartInUtc, userZone, Context, gymResponse.Result, level, pokemon, raidboss, timeSpan);
        }

        public async Task<ServiceResponse> AddResolveGymAsync(ZonedDateTime requestStartInUtc, DateTimeZone userZone, int gymId, byte level, IPokemon pokemon, IRaidboss raidboss, TimeSpan timeSpan, int interactiveLimit, FenceConfiguration[] fences)
        {
            var gym = await Context.Forts.SingleAsync(e => e.Id == gymId);
            return await AddSaveAsync(requestStartInUtc, userZone, Context, gym, level, pokemon, raidboss, timeSpan);
        }

	    private readonly Duration _eggDuration = Duration.FromMinutes(60);
		private readonly Duration _raidDuration = Duration.FromMinutes(45);

        private async Task<ServiceResponse> AddSaveAsync(ZonedDateTime requestStartInUtc, DateTimeZone userZone, Hydro74000Context context, Forts gym, byte level, IPokemon pokemon, IRaidboss raidboss, TimeSpan timeSpan)
        {
	        var utcNowAfterProcessing = SystemClock.Instance.GetCurrentInstant().InUtc();
	        var processingTime = utcNowAfterProcessing.Minus(requestStartInUtc);
	        var durationMinusProcessing = Duration.FromTimeSpan(timeSpan).Minus(processingTime);

	        var expiry = utcNowAfterProcessing.Plus(durationMinusProcessing).ToInstant();

            // Create the raid entry
            var beforeSpawnTime = utcNowAfterProcessing.Minus(Duration.FromMinutes(105)).ToInstant().ToUnixTimeSeconds();
            var raid = context.Raids.FirstOrDefault(e => e.FortId == gym.Id && e.TimeSpawn > beforeSpawnTime);
            if (raid == null)
            {
                raid = new Raids
                {
                    ExternalId = ThreadLocalRandom.NextLong(),
                    Fort = gym,
                    FortId = gym.Id
                };
                context.Raids.Add(raid);
            }

            string message;
            if (raidboss == null)
            {
                raid.Level = level;
                raid.TimeSpawn = (int)expiry.Minus(_eggDuration).ToUnixTimeSeconds();
                raid.TimeBattle = (int)expiry.ToUnixTimeSeconds();
                raid.TimeEnd = (int)expiry.Plus(_raidDuration).ToUnixTimeSeconds();
                //message = $"Level {level} Raid an der Arena \"{gym.Name}\", Start um {expiry.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}";
	            message = LocalizationService.Get("Raids_Messages_EggAdded", level, gym.Name, FormatExpiry(expiry, userZone));
            }
            else
            {
                raid.PokemonId = (short)raidboss.Id;
                raid.Level = level;
                raid.TimeSpawn = (int)expiry.Minus(Duration.FromMinutes(105)).ToUnixTimeSeconds();
                raid.TimeBattle = (int)expiry.Minus(Duration.FromMinutes(45)).ToUnixTimeSeconds();
                raid.TimeEnd = (int)expiry.ToUnixTimeSeconds();
				//message = $"{pokemon.Name} an der Arena \"{gym.Name}\", Ende um {expiry.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}";
				message = LocalizationService.Get("Raids_Messages_BossAdded", pokemon.Name, gym.Name, FormatExpiry(expiry, userZone));
			}

            await context.SaveChangesAsync();
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

        public async Task<ServiceResponse> HatchAsync(string gymName, string pokemonName, int interactiveLimit, FenceConfiguration[] fences)
        {
            var pokemonResponse = await PokemonService.GetPokemonAndRaidbossAsync(pokemonName, interactiveLimit, (selectedPokemonName) => HatchAsync(gymName, selectedPokemonName, interactiveLimit, fences));
            if (!pokemonResponse.IsSuccess) { return pokemonResponse; }

            var pokemonAndRaidboss = pokemonResponse.Result;
            var pokemon = pokemonAndRaidboss.Key;
            var raidboss = pokemonAndRaidboss.Value;

            return await HatchResolveGymAsync(gymName, pokemon, raidboss, interactiveLimit, fences);
        }

        public async Task<ServiceResponse> HatchResolveGymAsync(string gymName, IPokemon pokemon, IRaidboss raidboss, int interactiveLimit, FenceConfiguration[] fences)
        {
            var gymResponse = await GymService.GetGymAsync(Context, gymName, interactiveLimit, (selectedGymId) => HatchResolveGymWithIdAsync(selectedGymId, pokemon, raidboss, interactiveLimit), fences);
            if (!gymResponse.IsSuccess) { return gymResponse; }

            return await HatchSaveAsync(Context, gymResponse.Result, pokemon, raidboss, interactiveLimit);
        }

        public async Task<ServiceResponse> HatchResolveGymWithIdAsync(int gymId, IPokemon pokemon, IRaidboss raidboss, int interactiveLimit)
        {
            var gym = await Context.Forts.SingleAsync(e => e.Id == gymId);
            return await HatchSaveAsync(Context, gym, pokemon, raidboss, interactiveLimit);
        }

        public async Task<ServiceResponse> HatchSaveAsync(Hydro74000Context context, Forts gym, IPokemon pokemon, IRaidboss raidboss, int interactiveLimit)
        {
            var beforeSpawnTime = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromMinutes(90)).ToUnixTimeSeconds();
            var raid = context.Raids.FirstOrDefault(e => e.FortId == gym.Id && e.TimeSpawn > beforeSpawnTime);
            if (raid == null)
            {
				//return new ServiceResponse(false, $"Momentan ist kein Raid an der Arena \"{gym.Name}\" eingetragen.");
				return new ServiceResponse(false, LocalizationService.Get("Raids_Errors_Hatch_NoEntryFound", gym.Name));
			}

            raid.PokemonId = (short)raidboss.Id;
            await context.SaveChangesAsync();
			//return new ServiceResponse(true, $"{pokemon.Name} ist nun der neue Raidboss bei der Arena \"{gym.Name}\".");
			return new ServiceResponse(true, LocalizationService.Get("Raids_Messages_BossHatched", pokemon.Name, gym.Name));
		}

        #endregion
    }
}
