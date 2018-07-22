﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NodaTime;
using Raidfelden.Configuration;
using Raidfelden.Entities;
using Raidfelden.Services.Ocr;
using Raidfelden.Services.Ocr.RaidConfigurations;
using Raidfelden.Services.Ocr.RaidConfigurations.Ric1080X2220;
using Raidfelden.Services.Ocr.RaidConfigurations.Ric1440X2960;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;

namespace Raidfelden.Services
{
	public interface IOcrService
	{
		Task<ServiceResponse> AddRaidAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, string filePath, int interactiveLimit, FenceConfiguration[] fences, bool testMode);
	}

	public partial class OcrService : IOcrService
    {
        protected IConfigurationService ConfigurationService { get; }
        protected IGymService GymService { get; }
	    protected IPokemonService PokemonService { get; }
        protected IRaidService RaidService { get; }
		protected ILocalizationService LocalizationService { get; }
	    protected bool SaveDebugImages { get; private set; }

	    public OcrService(IConfigurationService configurationService, IGymService gymService, IPokemonService pokemonService, IRaidService raidService, ILocalizationService localizationService)
	    {
            ConfigurationService = configurationService;
            GymService = gymService;
		    PokemonService = pokemonService;
            RaidService = raidService;
		    LocalizationService = localizationService;
	    }

	    public async Task<ServiceResponse> AddRaidAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, string filePath, int interactiveLimit, FenceConfiguration[] fences, bool testMode)
	    {
		    SaveDebugImages = testMode;
			using (var image = Image.Load(filePath))
			{
				var configuration = image.GetRaidImageConfiguration(testMode);
                configuration.PreProcessImage(image);
                if (SaveDebugImages)
				{
					image.Save("_AfterPreprocess.png");
				}

				var raidOcrResult = await GetRaidOcrResultAsync(image, configuration, interactiveLimit, fences);

				if (!raidOcrResult.IsRaidImage)
				{
					return new ServiceResponse(false, LocalizationService.Get(textResource, "Raids_Errors_NotAnRaidImage"));
				}

				if (raidOcrResult.IsRaidBoss)
				{
					var timeLeft = raidOcrResult.RaidTimer.GetFirst();
					return await InteractivePokemonResolve(textResource, requestStartInUtc, userZone, timeLeft, raidOcrResult, fences, interactiveLimit);
				}
				else
				{
					var timeLeft = raidOcrResult.EggTimer.GetFirst();
					var level = raidOcrResult.EggLevel.GetFirst();
					return await InteractiveGymResolve(textResource, requestStartInUtc, userZone, timeLeft, level, null, raidOcrResult, fences, interactiveLimit);
				}
			}
	    }

	    private async Task<ServiceResponse> InteractivePokemonResolve(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, TimeSpan timeLeft, RaidOcrResult raidOcrResult, FenceConfiguration[] fences, int interactiveLimit)
	    {
			if (InteractiveServiceHelper.UseInteractiveMode(raidOcrResult.Pokemon.Results))
			{
				var pokemonCallbacks = InteractiveServiceHelper.GenericCreateCallbackAsync(interactiveLimit,
					(selectedPokemon) =>
						InteractiveGymResolve(textResource, requestStartInUtc, userZone, timeLeft, GetRaidbossPokemonById(selectedPokemon, raidOcrResult).Raidboss.Level, GetRaidbossPokemonById(selectedPokemon, raidOcrResult), raidOcrResult, fences, interactiveLimit),
					pokemon => pokemon.Pokemon.Id,
					(pokemon, list) => Task.FromResult(pokemon.Pokemon.Name),
					list => LocalizationService.Get(textResource, "Pokemon_Errors_ToManyFound", list.Count, raidOcrResult.Pokemon.OcrValue, interactiveLimit, "raidboss-"),
					list => LocalizationService.Get(textResource, "Pokemon_Errors_InteractiveMode", list.Count, raidOcrResult.Pokemon.OcrValue, "raidboss-"),
					raidOcrResult.Pokemon.Results.Select(e => e.Key).ToList());
				return await pokemonCallbacks;
			}

		    var raidbossPokemon = raidOcrResult.Pokemon.GetFirst();
		    return await InteractiveGymResolve(textResource, requestStartInUtc, userZone, timeLeft, raidbossPokemon.Raidboss.Level, raidbossPokemon, raidOcrResult, fences,
			    interactiveLimit);
	    }

	    private RaidbossPokemon GetRaidbossPokemonById(int id, RaidOcrResult raidOcrResult)
	    {
		    return raidOcrResult.Pokemon.Results.Where(e => e.Key.Pokemon.Id == id).Select(e => e.Key).First();

	    }

	    private async Task<ServiceResponse> InteractiveGymResolve(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, TimeSpan timeLeft, int level, RaidbossPokemon raidbossPokemon, RaidOcrResult raidOcrResult, FenceConfiguration[] fences, int interactiveLimit)
	    {
			if (!InteractiveServiceHelper.UseInteractiveMode(raidOcrResult.Gym.Results))
			{
				return await AddRaidAsync(textResource, requestStartInUtc, userZone, raidOcrResult.Gym.GetFirst().Id, level, raidbossPokemon, timeLeft, raidOcrResult, fences, interactiveLimit);
			}

			if (raidOcrResult.Gym.Results == null || raidOcrResult.Gym.Results.Length == 0)
			{
				return new ServiceResponse(false, LocalizationService.Get(textResource, "Gyms_Errors_NothingFound", raidOcrResult.Gym.OcrValue));
			}

			var gymCallbacks = InteractiveServiceHelper.GenericCreateCallbackAsync(interactiveLimit,
				(selectedGym) =>
					AddRaidAsync(textResource, requestStartInUtc, userZone, selectedGym, level, raidbossPokemon,
						timeLeft, raidOcrResult, fences, interactiveLimit),
				gym => gym.Id,
				(gym, list) => GymService.GetGymNameWithAdditionAsync(gym, list),
				list => LocalizationService.Get(textResource, "Gyms_Errors_ToManyFound", list.Count, raidOcrResult.Gym.OcrValue, interactiveLimit),
				list => LocalizationService.Get(textResource, "Gyms_Errors_InteractiveMode", list.Count, raidOcrResult.Gym.OcrValue),
				raidOcrResult.Gym.Results.Select(e => e.Key).ToList());
			return await gymCallbacks;
		}

	    private async Task<ServiceResponse> AddRaidAsync(Type textResource, ZonedDateTime requestStartInUtc, DateTimeZone userZone, int gymId, int level, RaidbossPokemon raidbossPokemon, TimeSpan timeLeft, RaidOcrResult raidOcrResult, FenceConfiguration[] fences, int interactiveLimit)
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
				return await RaidService.AddResolveGymAsync(textResource, requestStartInUtc, userZone, gymId, (byte)level, pokemon,
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

	    private async Task<RaidOcrResult> GetRaidOcrResultAsync(Image<Rgba32> image, RaidImageConfiguration imageConfiguration, int interactiveLimit, FenceConfiguration[] fences = null)
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
						    result.Gym = GetGym(imageFragment, imageConfiguration, fences, interactiveLimit).Result;
						    break;
						case RaidImageFragmentType.PokemonName:
							result.Pokemon = GetPokemon(imageFragment, imageConfiguration, interactiveLimit).Result;
							break;
						case RaidImageFragmentType.PokemonCp:
							result.PokemonCp = GetPokemonCp(imageFragment, imageConfiguration).Result;
							break;
						case RaidImageFragmentType.RaidTimer:
						    result.RaidTimer = GetTimerValue(imageFragment, imageConfiguration, type).Result;
						    break;
				    }
			    }
		    }
			);

		    if (result.PokemonCp.IsSuccess && result.Pokemon.IsSuccess && result.Pokemon.Results.Length > 1)
			{
				var pokemonWithCp =
					result.Pokemon.Results.Where(e => e.Value == 1d || e.Key.Raidboss.Cp == result.PokemonCp.GetFirst())
						.Select(e => new KeyValuePair<RaidbossPokemon, double>(e.Key, Math.Max(e.Value * 2, 1)))
						.ToArray();

				if (pokemonWithCp.Length > 0)
				{
					result.Pokemon = new OcrResult<RaidbossPokemon>(true, result.Pokemon.OcrValue, pokemonWithCp);
				}
		    }

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

		private async Task<OcrResult<IGym>> GetGym(Image<Rgba32> imageFragment, RaidImageConfiguration imageConfiguration, FenceConfiguration[] fences, int interactiveLimit)
		{
			imageFragment = imageConfiguration.PreProcessGymNameFragment(imageFragment);

			var ocrResult = await GetOcrResultAsync(imageFragment);
			
			if (!(ocrResult.Value > 0)) return new OcrResult<IGym>(false, ocrResult.Key);
			var similarGyms = await GymService.GetSimilarGymsByNameAsync(ocrResult.Key, fences, interactiveLimit * 2);
			if (similarGyms.Count == 0)
			{
				return new OcrResult<IGym>(false, ocrResult.Key);
			}

			// Check if there are exact matches
			var exactMatches = similarGyms.Where(e => e.Value == 1).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			if (exactMatches.Count > 0)
			{
				similarGyms = exactMatches;
			}

			var results = similarGyms.Select(kvp => new KeyValuePair<IGym, double>(kvp.Key, kvp.Value)).ToArray();
			return new OcrResult<IGym>(true, ocrResult.Key, results);
		}

		private async Task<OcrResult<RaidbossPokemon>> GetPokemon(Image<Rgba32> imageFragment, RaidImageConfiguration imageConfiguration, int interactiveLimit)
		{
			imageFragment = imageConfiguration.PreProcessPokemonNameFragment(imageFragment);

			var ocrResult = await GetOcrResultAsync(imageFragment);

			if (!(ocrResult.Value > 0)) return new OcrResult<RaidbossPokemon>(false, ocrResult.Key);
			var similarPokemon = PokemonService.GetSimilarRaidbossByNameAsync(ocrResult.Key, interactiveLimit * 2).Result;
			if (similarPokemon.Count == 0)
			{
				return new OcrResult<RaidbossPokemon>(false, ocrResult.Key);
			}
			var results = similarPokemon.Select(kvp => new KeyValuePair<RaidbossPokemon, double>(kvp.Key, kvp.Value)).ToArray();
			return new OcrResult<RaidbossPokemon> (true, ocrResult.Key, results);
		}

	    private async Task<OcrResult<int>> GetPokemonCp(Image<Rgba32> image, RaidImageConfiguration imageConfiguration)
	    {
		    var imageFragment = imageConfiguration.PreProcessPokemonCpFragment(image);
			var ocrResult = await GetOcrResultAsync(imageFragment);
			if (!(ocrResult.Value > 0)) return new OcrResult<int>(false, ocrResult.Key);
		    var cpString = ocrResult.Key.ToLowerInvariant();
		    if (cpString.StartsWith("cp") || cpString.StartsWith("03") || cpString.StartsWith("c3") || cpString.StartsWith("0p"))
		    {
			    cpString = ocrResult.Key.Substring(2).ToLowerInvariant();
		    }
			var cp = GetDigitsOnly(cpString);
		    cp = cp.Substring(Math.Max(cp.Length - 5, 0));
		    if (!int.TryParse(cp, out int result))
		    {
			    return new OcrResult<int>(false, ocrResult.Key);
		    }
		    return new OcrResult<int>(true, ocrResult.Key, new[] {new KeyValuePair<int, double>(result, ocrResult.Value)});
	    }

		private async Task<OcrResult<TimeSpan>> GetTimerValue(Image<Rgba32> imageFragment, RaidImageConfiguration imageConfiguration, RaidImageFragmentType imageFragmentType)
		{
			imageFragment = imageConfiguration.PreProcessTimerFragment(imageFragment, imageFragmentType);
			var result = await GetOcrResultAsync(imageFragment);
			// Remove all characters which can not be part of a timespan
			var arr = result.Key.ToCharArray();
			arr = Array.FindAll(arr, (c => (char.IsDigit(c) || c == ':')));
			var stringCleaned = new string(arr);
			if (result.Value > 0 && TimeSpan.TryParse(stringCleaned, out TimeSpan timeSpan))
		    {
			    return new OcrResult<TimeSpan>(true, stringCleaned,
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
			input = input.Replace("\n", " ");
			input = input.Replace(Environment.NewLine, " ");
			// Replace multiple whitespaces with just one
			RegexOptions options = RegexOptions.None;
			Regex regex = new Regex("[ ]{2,}", options);
			input = regex.Replace(input, " ");
			var arr = input.ToCharArray();

			arr = Array.FindAll(arr, (c => (char.IsLetterOrDigit(c)
										 || char.IsWhiteSpace(c)
										 || c == '.'
										 || c == '\''
										 || c == '-'
										 || c == ':')));
			return new string(arr).TrimEnd('\n').Trim();
		}

	    private static string GetDigitsOnly(string input)
	    {
		    var arr = input.ToCharArray();
		    arr = Array.FindAll(arr, char.IsDigit);
		    return new string(arr);
	    }
    }
}
