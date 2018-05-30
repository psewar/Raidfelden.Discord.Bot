using Discord.Commands;
using NodaTime;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Extensions;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Raidfelden.Discord.Bot.Modules
{
    [Group("pokemon")]
    public class PokemonModule : BaseModule<SocketCommandContext, PokemonChannel>
    {
        private readonly IPokemonService _pokemonService;

        public PokemonModule(IPokemonService pokemonService, IConfigurationService configurationService, IEmojiService emojiService)
            :base(configurationService, emojiService)
        {
            _pokemonService = pokemonService;
        }

        [Command("add"), Summary("Erlaubt es manuell Pokemon zu erfassen, die dann auf den Karten angezeigt werden.")]
        public async Task AddPokemonAsync([Summary("Der Name des Pokemon.")]string pokemonName, string latitude, string longitude, short cp, int level = 1, byte atkIv = 0, byte defIv = 0, byte staIv = 0)
        {
            try
            {
                // Only listen to commands in the configured channels or to exceptions
                if (!CanProcessRequest)
                {
                    return;
                }

                var latString = latitude.TrimEnd(',');
                var lat = double.Parse(latString, CultureInfo.InvariantCulture);
                var lonString = longitude.TrimEnd(',');
                var lon = double.Parse(lonString, CultureInfo.InvariantCulture);

                using (var context = new Hydro74000Context())
                {
                    IPokemon pokemon = null;
                    try
                    {
                        pokemon = GetPokemon(pokemonName);
                    }
                    catch (Exception ex)
                    {
                        await ReplyFailureAsync(ex.Message);
                        //await ReplyAsync(ex.Message);
                        return;
                    }

                    var sighting = new Sightings();
                    sighting.PokemonId = (short)pokemon.Id;
                    sighting.Cp = cp;
                    sighting.Level = (short)level;
                    sighting.AtkIv = atkIv;
                    sighting.DefIv = defIv;
                    sighting.StaIv = staIv;

                    var utcNow = SystemClock.Instance.GetCurrentInstant().InUtc();
                    var seconds = (utcNow.Minute * 60) + utcNow.Second;

                    var spawnpoints = await context.GetNearestSpawnpointsAsync(lat, lon);
                    foreach(var spawnpoint in spawnpoints)
                    {
                        // Ignore spawnpoints with missing despawntime
                        if (!spawnpoint.DespawnTime.HasValue)
                        {
                            continue;
                        }
                        
                        // If the duration has a value it's 60 minutes else it defaults to 30 minutes
                        var duration = spawnpoint.Duration ?? 30;

                        var despawnTime = (double)spawnpoint.DespawnTime.Value;
                        if (duration == 30)
                        {
                            despawnTime = despawnTime + 1800;
                        }

                        // Calculate the spawn time at a certain minute
                        var spawnTimeAtMinute = despawnTime / 60;
                        if ( spawnTimeAtMinute > 60)
                        {
                            spawnTimeAtMinute = spawnTimeAtMinute - 60;
                        }

                        // Calculate spawn and Despawn Time
                        var spawnTime = new LocalDateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, (int)spawnTimeAtMinute, utcNow.Second).InUtc().ToInstant();
                        if (utcNow.Minute < spawnTimeAtMinute)
                        {
                            spawnTime = new LocalDateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour - 1, (int)spawnTimeAtMinute, utcNow.Second).InUtc().ToInstant();
                        }
                        var endTime = spawnTime.Plus(Duration.FromMinutes(duration));

                        // Check if there could be a pokemon there during this time
                        var utcInstant = utcNow.ToInstant();
                        if (utcInstant > spawnTime && utcInstant < endTime)
                        {
                            sighting.ExpireTimestamp = (int)endTime.ToUnixTimeSeconds();
                            break;
                        }
                    }

                    if (!sighting.ExpireTimestamp.HasValue)
                    {
                        // We found no spawnpoint so lets guess for 15 minutes
                        int spawn_time = 15 * 60;
                        sighting.ExpireTimestamp = (int)SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(spawn_time)).ToUnixTimeSeconds();
                    }

                    sighting.Updated = (int)SystemClock.Instance.GetCurrentInstant().ToUnixTimeSeconds();
                    sighting.Lat = lat;
                    sighting.Lon = lon;
                    sighting.EncounterId = (ulong)ThreadLocalRandom.NextLong();

                    // Add present as a move so that sites like festzeit will show IV-Informations
                    if (atkIv != 0 || defIv != 0 || staIv != 0)
                    {
                        sighting.Move1 = 291;
                    }

                    await context.Sightings.AddAsync(sighting);
                    await context.SaveChangesAsync();
                    await ReplySuccessAsync("Pokemon erfolgreich hinzugefügt", $"Danke {Context.Message.Author.Username}, ich habe das Pokemon {pokemon.Name} hinzugefügt.");
                    //await ReplyAsync($"Danke {Context.Message.Author.Username}, ich habe das Pokemon {pokemon.Name} hinzugefügt.");
                }
            }
            catch (Exception ex)
            {
                var innerstEx = ex.GetInnermostException();
                await ReplyFailureAsync(innerstEx.Message);
                //await ReplyAsync($"Ein unerwarteter Fehler ist aufgetreten: {innerstEx.Message} Stack: {ex.StackTrace.Substring(5400)}");
            }
        }

        private IPokemon GetPokemon(string name)
        {
            return _pokemonService.GetPokemonByName(name);
            //var provider = Program.ServiceProvider.Resolve<IPokemonService>();
            //var pokemon = provider.GetPokemonByName(name);
            //return pokemon;
        }
    }
}
