using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Utilities.Ocr;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using NodaTime;
using Raidfelden.Discord.Bot.Models;
using Raidfelden.Discord.Bot.Utilities.Ocr.RaidConfigurations;

namespace Raidfelden.Discord.Bot.Services
{
	public interface IOcrService
	{
		Task<ServiceResponse> AddRaidAsync(ZonedDateTime requestStartInUtc, DateTimeZone userZone, string filePath, int interactiveLimit, FenceConfiguration[] fences, bool testMode);
	}

	public class OcrService : IOcrService, IDisposable
    {
	    protected Hydro74000Context Context { get; }
        protected IConfigurationService ConfigurationService { get; }
        protected IGymService GymService { get; }
	    protected IPokemonService PokemonService { get; }
        protected IRaidService RaidService { get; }
		protected ILocalizationService LocalizationService { get; }
	    protected bool SaveDebugImages { get; private set; }

	    public OcrService(Hydro74000Context context, IConfigurationService configurationService, IGymService gymService, IPokemonService pokemonService, IRaidService raidService, ILocalizationService localizationService)
	    {
			Context = context;
            ConfigurationService = configurationService;
            GymService = gymService;
		    PokemonService = pokemonService;
            RaidService = raidService;
		    LocalizationService = localizationService;
	    }

	    public async Task<ServiceResponse> AddRaidAsync(ZonedDateTime requestStartInUtc, DateTimeZone userZone, string filePath, int interactiveLimit, FenceConfiguration[] fences, bool testMode)
	    {
		    SaveDebugImages = testMode;
			using (var image = Image.Load(filePath))
			{
				var configuration = GetConfiguration(image);
                configuration.PreProcessImage(image);
                if (SaveDebugImages)
				{
					image.Save("_AfterPreprocess.png");
				}

				var raidOcrResult = await GetRaidOcrResultAsync(image, configuration, Context, fences);

				if (!raidOcrResult.IsRaidImage)
				{
					return new ServiceResponse(false, "The picture is probably not a raid image");
				}

				if (raidOcrResult.IsRaidBoss)
				{
					var timeLeft = raidOcrResult.RaidTimer.GetFirst();
					return await InteractivePokemonResolve(requestStartInUtc, userZone, timeLeft, raidOcrResult, fences, interactiveLimit);
				}
				else
				{
					var timeLeft = raidOcrResult.EggTimer.GetFirst();
					var level = raidOcrResult.EggLevel.GetFirst();
					return await InteractiveGymResolve(requestStartInUtc, userZone, timeLeft, level, null, raidOcrResult, fences, interactiveLimit);
				}
			}
	    }

	    private async Task<ServiceResponse> InteractivePokemonResolve(ZonedDateTime requestStartInUtc, DateTimeZone userZone, TimeSpan timeLeft, RaidOcrResult raidOcrResult, FenceConfiguration[] fences, int interactiveLimit)
	    {
			if (UseInteractiveMode(raidOcrResult.Pokemon))
			{
				var pokemonCallbacks = InteractiveServiceHelper.GenericCreateCallbackAsync(interactiveLimit,
					(selectedPokemon) =>
						InteractiveGymResolve(requestStartInUtc, userZone, timeLeft, GetRaidbossPokemonById(selectedPokemon, raidOcrResult).Raidboss.Level, GetRaidbossPokemonById(selectedPokemon, raidOcrResult), raidOcrResult, fences, interactiveLimit),
					pokemon => pokemon.Pokemon.Id,
					(pokemon, list) => Task.FromResult(pokemon.Pokemon.Name),
					list => LocalizationService.Get("Pokemon_Errors_ToManyFound", list.Count, raidOcrResult.Pokemon.OcrValue, interactiveLimit, "raidboss-"),
					list => LocalizationService.Get("Pokemon_Errors_InteractiveMode", list.Count, raidOcrResult.Pokemon.OcrValue, "raidboss-"),
					raidOcrResult.Pokemon.Results.Select(e => e.Key).ToList());
				return await pokemonCallbacks;
			}

		    var raidbossPokemon = raidOcrResult.Pokemon.GetFirst();
		    return await InteractiveGymResolve(requestStartInUtc, userZone, timeLeft, raidbossPokemon.Raidboss.Level, raidbossPokemon, raidOcrResult, fences,
			    interactiveLimit);
	    }

	    private RaidbossPokemon GetRaidbossPokemonById(int id, RaidOcrResult raidOcrResult)
	    {
		    return raidOcrResult.Pokemon.Results.Where(e => e.Key.Pokemon.Id == id).Select(e => e.Key).First();

	    }

	    private async Task<ServiceResponse> InteractiveGymResolve(ZonedDateTime requestStartInUtc, DateTimeZone userZone, TimeSpan timeLeft, int level, RaidbossPokemon raidbossPokemon, RaidOcrResult raidOcrResult, FenceConfiguration[] fences, int interactiveLimit)
	    {
		    if (!UseInteractiveMode(raidOcrResult.Gym))
		    {
			    return await AddRaidAsync(requestStartInUtc, userZone, raidOcrResult.Gym.GetFirst().Id, level, raidbossPokemon, timeLeft, raidOcrResult, fences, interactiveLimit);
		    }

		    if (raidOcrResult.Gym.Results == null || raidOcrResult.Gym.Results.Length == 0)
		    {
			    return new ServiceResponse(false, LocalizationService.Get("Gyms_Errors_NothingFound", raidOcrResult.Gym.OcrValue));
		    }

		    var gymCallbacks = InteractiveServiceHelper.GenericCreateCallbackAsync(interactiveLimit,
			    (selectedGym) =>
				    AddRaidAsync(requestStartInUtc, userZone, selectedGym, level, raidbossPokemon,
					    timeLeft, raidOcrResult, fences, interactiveLimit),
			    gym => gym.Id,
			    (gym, list) => GymService.GetGymNameWithAdditionAsync(gym, list),
				list => LocalizationService.Get("Gyms_Errors_ToManyFound", list.Count, raidOcrResult.Gym.OcrValue, interactiveLimit),
				list => LocalizationService.Get("Gyms_Errors_InteractiveMode", list.Count, raidOcrResult.Gym.OcrValue),
			    raidOcrResult.Gym.Results.Select(e => e.Key).ToList());
		    return await gymCallbacks;
	    }

	    private async Task<ServiceResponse> AddRaidAsync(ZonedDateTime requestStartInUtc, DateTimeZone userZone, int gymId, int level, RaidbossPokemon raidbossPokemon, TimeSpan timeLeft, RaidOcrResult raidOcrResult, FenceConfiguration[] fences, int interactiveLimit)
	    {
		    IPokemon pokemon = null;
		    IRaidboss raidboss = null;
		    if (raidbossPokemon != null)
		    {
			    pokemon = raidbossPokemon.Pokemon;
			    raidboss = raidbossPokemon.Raidboss;
		    }

			// aka (Unit)TestMode
			if (!SaveDebugImages)
			{
				return await RaidService.AddResolveGymAsync(requestStartInUtc, userZone, gymId, (byte)level, pokemon,
				raidboss, timeLeft, interactiveLimit, fences);
			}
			else
			{
				return await ReturnTestInformation(raidOcrResult);
			}
	    }

	    private async Task<ServiceResponse> ReturnTestInformation(RaidOcrResult raidOcrResult)
	    {
		    string message;
			var gym = raidOcrResult.Gym.GetFirst();
			if (raidOcrResult.RaidTimer.IsSuccess)
			{
				var timer = raidOcrResult.RaidTimer.GetFirst();
				var pokemon = raidOcrResult.Pokemon.GetFirst();
				message = $".raids add \"{gym.Name}\" \"{pokemon.Pokemon.Name}\" {string.Concat(timer.Minutes, ":", timer.Seconds)}";
			}
			else
			{
				var eggLevel = raidOcrResult.EggLevel.GetFirst();
				var timer = raidOcrResult.EggTimer.GetFirst();
				message = $".raids add \"{gym.Name}\" \"{eggLevel}\" {string.Concat(timer.Minutes, ":", timer.Seconds)}"; 
			}

			var result = new ServiceResponse(true, message);
			return await Task.FromResult(result);
		}

	    private bool UseInteractiveMode<T>(OcrResult<T> result, double threshold = 0.3)
	    {
		    if (result.Results == null || result.Results.Length == 0)
		    {
			    return true;
		    }

		    var bestMatch = result.Results.First().Value;
		    if (result.Results.Skip(1).Select(e => e.Value).Any(e => e > (bestMatch - 0.05d)))
		    {
			    return true;
		    }
			return result.Results.First().Value < threshold;
	    }

		private RaidImageConfiguration GetConfiguration(Image<Rgba32> image)
		{
			var configuration = new RaidImageConfiguration(1080, 1920);
			if (image.Height == 2220 && image.Width == 1080)
			{
				if (HasBottomMenu(image))
				{
					if (HasTopMenu(image))
					{
						configuration = new BothMenu1080X2220Configuration();
					}
					else
					{
						configuration = new BottomMenu1080X2220Configuration();
					}
				}
				else
				{
					configuration = new WithoutMenu1080X2220Configuration();
				}
			}

			if (image.Height == 2960 && HasBottomMenu(image))
			{
				configuration = new GalaxyS9BottomMenuImageConfiguration();
			}

			if (image.Height == 2436 && image.Width == 1125)
			{
				configuration = new IPhoneXImageConfiguration();
			}

			if (image.Height == 1920 && image.Width == 1080 && HasBottomMenu(image))
			{
				configuration = new BottomMenu1080X1920Configuration();
			}

            if (image.Height == 1600 && image.Width == 739)
            {
                configuration = new WithoutMenu739X1600();
            }

            if (image.Height == 1600 && image.Width == 900 && HasBottomMenu(image))
            {
                configuration = new BottomMenu900X1600Configuration();
            }

			configuration.SaveDebugImages = SaveDebugImages;

			return configuration;
		}

		private bool HasTopMenu(Image<Rgba32> image)
		{
			// If the whole line has the exact same color it probably is a menu
			var color = image[0, 0];
			for (int x = 1; x < image.Width; x++)
			{
				if (image[x, 0] != color)
				{
					return false;
				}
			}
			return true;
		}

		private bool HasBottomMenu(Image<Rgba32> image)
		{
            // If the whole line has the exact same color it probably is a menu
            var color = image[0, image.Height - 1];
            for (int x = 1; x < image.Width; x++)
            {
                if (image[x, image.Height -1] != color)
                {
                    return false;
                }
            }
            return true;
		}

	    private async Task<RaidOcrResult> GetRaidOcrResultAsync(Image<Rgba32> image, RaidImageConfiguration imageConfiguration, Hydro74000Context context, FenceConfiguration[] fences = null)
	    {
		    var result = new RaidOcrResult();
			var fragmentTypes = Enum.GetValues(typeof(RaidImageFragmentType)).Cast<RaidImageFragmentType>();
			
			//foreach (var type in fragmentTypes)
		    Parallel.ForEach(fragmentTypes, type =>
		    {
			    using (var imageFragment = image.Clone(e => e.Crop(imageConfiguration[type])))
			    {
				    switch (type)
				    {
					    case RaidImageFragmentType.EggTimer:
						    result.EggTimer = GetTimerValue(imageFragment, imageConfiguration, type).Result;
						    break;
					    case RaidImageFragmentType.EggLevel:
						    result.EggLevel = GetEggLevel(imageFragment, imageConfiguration).Result;
						    break;
					    case RaidImageFragmentType.GymName:
						    result.Gym = GetGym(imageFragment, imageConfiguration, context, fences).Result;
						    break;
					    case RaidImageFragmentType.PokemonName:
						    result.Pokemon = GetPokemon(imageFragment, imageConfiguration).Result;
						    break;
					    case RaidImageFragmentType.RaidTimer:
						    result.RaidTimer = GetTimerValue(imageFragment, imageConfiguration, type).Result;
						    break;
				    }
			    }
		    }
			);

		    return await Task.FromResult(result);
	    }

		public async Task<OcrResult<int>> GetEggLevel(Image<Rgba32> imageFragment, RaidImageConfiguration imageConfiguration)
	    {
            if (SaveDebugImages)
            {
                imageFragment.Save($"_{RaidImageFragmentType.EggLevel}_Step1_Analyze.png");
            }

            byte whiteThreshold = 240;
			// Check the locations for level 1, 3 and 5 raids
			var whitePixelCount = imageConfiguration.Level5Points.Select(levelPoint => imageFragment[levelPoint.X, levelPoint.Y]).Count(pixel => pixel.R > whiteThreshold && pixel.G > whiteThreshold && pixel.B > whiteThreshold && pixel.A > whiteThreshold);

		    // No white pixels found so lets check the locations for level 2 and 4 raids
			if (whitePixelCount == 0)
			{
				whitePixelCount = imageConfiguration.Level4Points.Select(levelPoint => imageFragment[levelPoint.X, levelPoint.Y]).Count(pixel => pixel.R > whiteThreshold && pixel.G > whiteThreshold && pixel.B > whiteThreshold && pixel.A > whiteThreshold);
			}

			// Make sure the level is within the possible range
		    if (whitePixelCount < 1 || whitePixelCount > 5)
		    {
				return await Task.FromResult(new OcrResult<int>(false, string.Empty));
		    }

		    var results = new[] {new KeyValuePair<int, double>(whitePixelCount, 1)};
			return await Task.FromResult(new OcrResult<int>(true, string.Empty, results));
		}

		private async Task<OcrResult<Forts>> GetGym(Image<Rgba32> imageFragment, RaidImageConfiguration imageConfiguration, Hydro74000Context context, FenceConfiguration[] fences)
		{
			imageFragment = imageConfiguration.PreProcessGymNameFragment(imageFragment);

			var ocrResult = await GetOcrResultAsync(imageFragment);
			
			if (!(ocrResult.Value > 0)) return new OcrResult<Forts>(false, ocrResult.Key);
			var similarGyms = await GymService.GetSimilarGymsByNameAsync(context, ocrResult.Key, fences, 3);
			if (similarGyms.Count == 0)
			{
				return new OcrResult<Forts>(false, ocrResult.Key);
			}
			var results = similarGyms.Select(kvp => new KeyValuePair<Forts, double>(kvp.Key, kvp.Value)).ToArray();
			return new OcrResult<Forts>(true, ocrResult.Key, results);
		}

		private async Task<OcrResult<RaidbossPokemon>> GetPokemon(Image<Rgba32> imageFragment, RaidImageConfiguration imageConfiguration)
		{
			imageFragment = imageConfiguration.PreProcessPokemonNameFragment(imageFragment);

			var ocrResult = await GetOcrResultAsync(imageFragment);

			if (!(ocrResult.Value > 0)) return new OcrResult<RaidbossPokemon>(false, ocrResult.Key);
			var similarPokemon = PokemonService.GetSimilarRaidbossByNameAsync(ocrResult.Key, 3).Result;
			if (similarPokemon.Count == 0)
			{
				return new OcrResult<RaidbossPokemon>(false, ocrResult.Key);
			}
			var results = similarPokemon.Select(kvp => new KeyValuePair<RaidbossPokemon, double>(kvp.Key, kvp.Value)).ToArray();
			return new OcrResult<RaidbossPokemon> (true, ocrResult.Key, results);
		}

		private async Task<OcrResult<TimeSpan>> GetTimerValue(Image<Rgba32> imageFragment, RaidImageConfiguration imageConfiguration, RaidImageFragmentType imageFragmentType)
		{
			imageFragment = imageConfiguration.PreProcessTimerFragment(imageFragment, imageFragmentType);
			var result = await GetOcrResultAsync(imageFragment);
		    
		    if (result.Value > 0 && TimeSpan.TryParse(result.Key, out TimeSpan timeSpan))
		    {
			    return new OcrResult<TimeSpan>(true, result.Key,
				    new[] {new KeyValuePair<TimeSpan, double>(timeSpan, result.Value)});
		    }
			return new OcrResult<TimeSpan>(false, result.Key);
		}

		private async Task<KeyValuePair<string, double>> GetOcrResultAsync(Image<Rgba32> imageFragment)
        {
            string output;
            var tempOutputFile = Path.GetTempPath() + Guid.NewGuid();
            var tempImageFile = CreateTempImageFile(imageFragment);
            try
            {
                var ocrConfiguration = ConfigurationService.GetOcrConfiguration();
                var tesseractPath = GetTesseractPath(ocrConfiguration);
                var tessdataDir = GetTessdataPath(ocrConfiguration);
                var languages = GetOcrLanguages(ocrConfiguration);

                var arguments = new StringBuilder();
                arguments.Append("--tessdata-dir " + tessdataDir);
                arguments.Append(" " + tempImageFile);  // Image file.
                arguments.Append(" " + tempOutputFile); // Output file (tesseract add '.txt' at the end)
                if (!string.IsNullOrWhiteSpace(ocrConfiguration.AdditionalParameters))
                {
                    arguments.Append(" " + ocrConfiguration.AdditionalParameters);
                }
                arguments.Append(" -l " + languages);    // Languages.
                arguments.Append(" " + Path.Combine(tessdataDir, "configs", "bazaar"));    // Config.

	            var info = new ProcessStartInfo
	            {
					// WorkingDirectory = tesseractPath;
					WindowStyle = ProcessWindowStyle.Hidden,
		            UseShellExecute = false,
		            FileName = tesseractPath,
		            Arguments = arguments.ToString()
	            };

                // Start tesseract.
                var process = Process.Start(info);
	            if (process == null)
	            {
		            throw new Exception("Unable to start the OCR-Recognition service.");
	            }
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    // Exit code: success.
                    output = File.ReadAllText(tempOutputFile + ".txt");
                }
                else
                {
                    throw new Exception("Error. Tesseract stopped with an error code = " + process.ExitCode);
                }
            }
            finally
            {
                File.Delete(tempImageFile);
                File.Delete(tempOutputFile + ".txt");
            }

            var value = RemoveUnwantedCharacters(output);
            var probability = 1;
            if (string.IsNullOrWhiteSpace(value))
            {
                probability = 0;
            }

	        return await Task.FromResult(new KeyValuePair<string, double>(value, probability));
        }

        private string GetTesseractPath(OcrConfiguration ocrConfiguration)
        {
            var tesseractPath = ocrConfiguration.PathToTesseract;
            // Try some defaults if nothing is set
            if (string.IsNullOrWhiteSpace(tesseractPath))
            {
                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                if (isWindows)
                {
	                // Default Windows installation
                    //tesseractPath = @"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe";
	                tesseractPath = Path.Combine(RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "x64" : "x86", "tesseract.exe");
                }
                else
                {
                    // Default Homebrew installation
                    tesseractPath = Path.Combine("/usr/local/Cellar/tesseract/3.05.01/bin", "tesseract");
                }
            }
            return tesseractPath;
        }

        private string GetTessdataPath(OcrConfiguration ocrConfiguration)
        {
            var result = ocrConfiguration.PathToTessdata;
            if(string.IsNullOrWhiteSpace(result))
            {
                result = Path.Combine(".", "tessdata");
            }

            return result;
        }

        private string GetOcrLanguages(OcrConfiguration ocrConfiguration)
        {
            var languages = ocrConfiguration.Languages;
            if (languages == null || languages.Length == 0)
            {
                languages = new[] { "deu", "eng" };
            }
            return string.Join("+", languages);
        }

        private static string CreateTempImageFile<TPixel>(Image<TPixel> image) where TPixel : struct, IPixel<TPixel>
		{
			var tempImageFile = Path.GetTempFileName() + ".png";
			image.Save(tempImageFile);
			return tempImageFile;
		}

		private static string RemoveUnwantedCharacters(string input)
		{
			//return input;
			input = input.Replace("—", "-");
			var arr = input.ToCharArray();

			arr = Array.FindAll(arr, (c => (char.IsLetterOrDigit(c)
										 || char.IsWhiteSpace(c)
										 || c == '.'
										 || c == '\''
										 || c == '-'
										 || c == ':')));
			return new string(arr).TrimEnd('\n').Trim();
		}


	    public void Dispose()
	    {
		    Context?.Dispose();
	    }

	    public class OcrResult<T>
		{
			public OcrResult(bool isSuccess, string ocrValue, KeyValuePair<T, double>[] results = null)
			{
				IsSuccess = isSuccess;
				OcrValue = ocrValue;
				Results = results;
			}

			public bool IsSuccess { get; }
			public KeyValuePair<T, double>[] Results { get; }
			public string OcrValue { get; }

			public T GetFirst()
			{
				return Results[0].Key;
			}
		}

		public class RaidOcrResult
		{
			public OcrResult<int> EggLevel { get; set; }
			public OcrResult<TimeSpan> EggTimer { get; set; }
			public OcrResult<Forts> Gym { get; set; }
			public OcrResult<RaidbossPokemon> Pokemon { get; set; }
			public OcrResult<TimeSpan> RaidTimer { get; set; }

			public bool IsRaidImage => EggTimer.IsSuccess || RaidTimer.IsSuccess;
			public bool IsRaidBoss => RaidTimer.IsSuccess && Pokemon.IsSuccess;

			public bool IsSuccess
			{
				get
				{
					if (!IsRaidImage)
					{
						return false;
					}

					if (IsRaidBoss)
					{
						return Gym.IsSuccess && Pokemon.IsSuccess && RaidTimer.IsSuccess;
					}

					return Gym.IsSuccess && EggLevel.IsSuccess && EggTimer.IsSuccess;
				}
			}
		}
	}
}
