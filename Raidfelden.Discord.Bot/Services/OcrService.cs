using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Extensions.ImageSharp;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Utilities;
using Raidfelden.Discord.Bot.Utilities.Ocr;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Binarization;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using NodaTime;

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
        protected bool SaveDebugImages { get; private set; }

	    public OcrService(Hydro74000Context context, IConfigurationService configurationService, IGymService gymService, IPokemonService pokemonService, IRaidService raidService)
	    {
			Context = context;
            ConfigurationService = configurationService;
            GymService = gymService;
		    PokemonService = pokemonService;
            RaidService = raidService;
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
				string message;
				var raidOcrResult = await GetFragmentResultAsync(image, configuration, Context, fences);

				var isRaidBoss = raidOcrResult.RaidTimer.IsSuccess;
				if (isRaidBoss)
				{
					
				}
				else
				{
					
				}
				/*
				if (!raidOcrResult.Pokemon.IsSuccess || raidOcrResult.Pokemon.Results.Length == 0)
				{
					return new ServiceResponse(false, "Der Raidboss konnte nich erkannt werden.");
				}
				if (!raidOcrResult.EggLevel.IsSuccess || raidOcrResult.Pokemon.Results.Length == 0)
				{
					return new ServiceResponse(false, "Das Level konnte nich erkannt werden.");
				}
				if (!raidOcrResult.EggTimer.IsSuccess || raidOcrResult.Pokemon.Results.Length == 0)
				{
					return new ServiceResponse(false, "Die Restzeit konnte nich erkannt werden.");
				}
				if (!raidOcrResult.Gym.IsSuccess || raidOcrResult.Pokemon.Results.Length == 0)
				{
					return new ServiceResponse(false, "Die Arena konnte nich erkannt werden.");
				}
				if (!raidOcrResult.RaidTimer.IsSuccess || raidOcrResult.Pokemon.Results.Length == 0)
				{
					return new ServiceResponse(false, "Die Restzeit konnte nich erkannt werden.");
				}
				*/

				var gym = raidOcrResult.Gym.GetFirst();
				if (raidOcrResult.RaidTimer.IsSuccess)
				{
					var timer = raidOcrResult.RaidTimer.GetFirst();
					var pokemon = raidOcrResult.Pokemon.GetFirst();

					if (!testMode)
					{
						return await RaidService.AddAsync(requestStartInUtc, userZone, gym.Name, pokemon.Name, timer.ToString(@"mm\:ss"), interactiveLimit, fences);
					}
					else
					{
						message = $".raids add \"{gym.Name}\" \"{pokemon.Name}\" {string.Concat(timer.Minutes, ":", timer.Seconds)}";
					}
				}
				else
				{
					var eggLevel = raidOcrResult.EggLevel.GetFirst();
					var timer = raidOcrResult.EggTimer.GetFirst();
					if (!testMode)
					{
						return await RaidService.AddAsync(requestStartInUtc, userZone, gym.Name, eggLevel.ToString(CultureInfo.InvariantCulture), timer.ToString(@"mm\:ss"), interactiveLimit, fences);
					}
					else{message =$".raids add \"{gym.Name}\" \"{eggLevel}\" {string.Concat(timer.Minutes, ":", timer.Seconds)}";}
				}

				var result = new ServiceResponse(true, message);
				//if (probability < 0.3)
				//{
				//	//result.InterActiveCallbacks.Add();
				//	result = new ServiceResponse(true, probability.ToString());
				//}
				return await Task.FromResult(result);
			}
	    }

		private BaseRaidImageConfiguration GetConfiguration(Image<Rgba32> image)
		{
			var configuration = new BaseRaidImageConfiguration(1080, 1920);
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

	    private async Task<RaidOcrResult> GetFragmentResultAsync(Image<Rgba32> image, BaseRaidImageConfiguration imageConfiguration, Hydro74000Context context, FenceConfiguration[] fences = null)
	    {
		    var result = new RaidOcrResult();
			var fragmentTypes = Enum.GetValues(typeof(ImageFragmentType)).Cast<ImageFragmentType>();
			
			//foreach (var type in fragmentTypes)
		    Parallel.ForEach(fragmentTypes, type =>
		    {
			    using (var imageFragment = image.Clone(e => e.Crop(imageConfiguration[type])))
			    {
				    switch (type)
				    {
					    case ImageFragmentType.EggTimer:
						    result.EggTimer = GetTimerValue(imageFragment, type).Result;
						    break;
					    case ImageFragmentType.EggLevel:
						    result.EggLevel = GetEggLevel(imageFragment, imageConfiguration).Result;
						    break;
					    case ImageFragmentType.GymName:
						    result.Gym = GetGymName(imageFragment, context, fences).Result;
						    break;
					    case ImageFragmentType.PokemonName:
						    result.Pokemon = GetPokemonName(imageFragment, imageConfiguration).Result;
						    break;
					    case ImageFragmentType.RaidTimer:
						    result.RaidTimer = GetTimerValue(imageFragment, type).Result;
						    break;
				    }
			    }
		    }
			);

		    return await Task.FromResult(result);
			/*using (var imageFragment = image.Clone(e => e.Crop(imageConfiguration[fragmentType])))
		    {
			    if (SaveDebugImages)
			    {
				    imageFragment.Save($"_{fragmentType}_Created.png");
			    }
			    var result = new OcrResult<T>(false, string.Empty);
			    switch (fragmentType)
			    {
					case ImageFragmentType.EggLevel:
						result= await GetEggLevel<T>(imageFragment, imageConfiguration);
					    break;
					case ImageFragmentType.GymName:
						result= await GetGymName<T>(imageFragment, engine, context, fences);
						break;
					case ImageFragmentType.PokemonName:
						result= await GetPokemonName(imageFragment, imageConfiguration);
						break;
					case ImageFragmentType.EggTimer:
					case ImageFragmentType.RaidTimer:
						result= await GetTimerValue(imageFragment, fragmentType);
						break;
				}

				if (SaveDebugImages)
				{
					imageFragment.Save($"_{fragmentType}_ZFinal.png");
				}
			    return result;
		    }*/
	    }

		private async Task<OcrResult<int>> GetEggLevel(Image<Rgba32> imageFragment, BaseRaidImageConfiguration imageConfiguration)
	    {
            if (SaveDebugImages)
            {
                imageFragment.Save($"_{ImageFragmentType.EggLevel}_Step1_Analyze.png");
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

		private async Task<OcrResult<Forts>> GetGymName(Image<Rgba32> imageFragment, Hydro74000Context context, FenceConfiguration[] fences)
		{
			var multiplier = 2;
			var size = new Size(imageFragment.Width * multiplier, imageFragment.Height * multiplier);
			var resizeOptions = new ResizeOptions
			{
				Mode = ResizeMode.Stretch,
				Size = size,
				Compand = true,
				Sampler = KnownResamplers.Welch
			};

			if (SaveDebugImages)
			{
				imageFragment.Save($"_{ImageFragmentType.GymName}_Step1_Resize.png");
			}

			imageFragment.Mutate(m => m.Resize(resizeOptions).Invert().BinaryThreshold(0.2f));

			if (SaveDebugImages)
			{
				imageFragment.Save($"_{ImageFragmentType.GymName}_Step2_Binary.png");
			}

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

		private async Task<OcrResult<IPokemon>> GetPokemonName(Image<Rgba32> imageFragment, BaseRaidImageConfiguration imageConfiguration)
		{
			const byte floodFillLetterTolerance = 10;
			var floodFillLetters = new QueueLinearFloodFiller
			{
				Bitmap = imageFragment,
				FillColor = Rgba32.Black,
				Tolerance =
				{
					[0] = floodFillLetterTolerance,
					[1] = floodFillLetterTolerance,
					[2] = floodFillLetterTolerance
				}
			};
			var borderColor = imageConfiguration.PokemonNameBorderColor;
			foreach (var encapsulatedPixel in GetEncapsulatedPixels(imageFragment, Rgba32.White, borderColor, 30))
			{
				floodFillLetters.FloodFill(encapsulatedPixel);
			}

			if (SaveDebugImages)
			{
				imageFragment.Save($"_{ImageFragmentType.PokemonName}_Step1_FloodFillLetters.png");
			}

			imageFragment.Mutate(m => m.BinaryThreshold(0.01f).Invert());

			if (SaveDebugImages)
			{
				imageFragment.Save($"_{ImageFragmentType.PokemonName}_Step2_Binary.png");
			}

			const byte floodFillBorderTolerance = 1;
			var floodFillBorders = new QueueLinearFloodFiller
			{
				Bitmap = imageFragment,
				FillColor = Rgba32.Black,
				Tolerance =
				{
					[0] = floodFillBorderTolerance,
					[1] = floodFillBorderTolerance,
					[2] = floodFillBorderTolerance
				}
			};
			foreach (var point in PixelsWithColorAtBorder(imageFragment, Rgba32.White))
			{
				floodFillBorders.FloodFill(point);
			}

			imageFragment.Mutate(m => m.Invert());

			if (SaveDebugImages)
			{
				imageFragment.Save($"_{ImageFragmentType.PokemonName}_Step3_BlackBorderEntriesRemoved.png");
			}

			var ocrResult = await GetOcrResultAsync(imageFragment);

			if (!(ocrResult.Value > 0)) return new OcrResult<IPokemon>(false, ocrResult.Key);
			var similarPokemon = PokemonService.GetSimilarRaidbossByNameAsync(ocrResult.Key, 3).Result;
			if (similarPokemon.Count == 0)
			{
				return new OcrResult<IPokemon>(false, ocrResult.Key);
			}
			var results = similarPokemon.Select(kvp => new KeyValuePair<IPokemon, double>(kvp.Key, kvp.Value)).ToArray();
			return new OcrResult<IPokemon> (true, ocrResult.Key, results);
		}

		private async Task<OcrResult<TimeSpan>> GetTimerValue(Image<Rgba32> imageFragment, ImageFragmentType imageFragmentType)
	    {
			imageFragment.Mutate(m => m.Invert().BinaryThreshold(0.1f));
			if (SaveDebugImages)
			{
				imageFragment.Save($"_{imageFragmentType}_Step1_Binary.png");
			}
			var result = await GetOcrResultAsync(imageFragment);
		    
		    if (result.Value > 0 && TimeSpan.TryParse(result.Key, out TimeSpan timeSpan))
		    {
			    return new OcrResult<TimeSpan>(true, result.Key,
				    new[] {new KeyValuePair<TimeSpan, double>(timeSpan, result.Value)});
		    }
			return new OcrResult<TimeSpan>(false, result.Key);
		}

		//private async Task<OcrResult> GetOcrResultAsync(Image<Rgba32> imageFragment, TesseractEngine engine)
		//   {
		//	var tempImageFile = CreateTempImageFile(imageFragment);
		//    try
		//    {
		//		using (var tempImage = Pix.LoadFromFile(tempImageFile))
		//		{
		//			using (var page = engine.Process(tempImage))
		//			{
		//				var value = RemoveUnwantedCharacters(page.GetText());
		//				var probability = page.GetMeanConfidence();
		//				return
		//					await Task.FromResult(new OcrResult(probability > 0,
		//						new[] {new KeyValuePair<string, double>(value, probability),}));
		//			}
		//		}
		//	}
		//    finally
		//    {
		//		System.IO.File.Delete(tempImageFile);
		//	}
		//}


		private async Task<KeyValuePair<string, double>> GetOcrResultAsync(Image<Rgba32> imageFragment)
        {
            string output = string.Empty;
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

                ProcessStartInfo info = new ProcessStartInfo();
                //info.WorkingDirectory = tesseractPath;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.UseShellExecute = false;
                info.FileName = tesseractPath;
                info.Arguments = arguments.ToString();
                Console.WriteLine(info.Arguments);

                // Start tesseract.
                Process process = Process.Start(info);
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
                    if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    {
                        tesseractPath = Path.Combine("x64", "tesseract.exe");
                    }
                    else
                    {
                        tesseractPath = Path.Combine("x86", "tesseract.exe");
                    }
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

		private static IEnumerable<Point> GetEncapsulatedPixels(Image<Rgba32> image, Rgba32 color, Rgba32 borderColor, int? maxDistanceToSearch = null, int colorTolerance = 30)
		{			
			var imageHeight = image.Height;
			var imageWidth = image.Width;
			for (int y = 0; y < imageHeight; y++)
			{
				for (int x = 0; x < imageWidth; x++)
				{
					if (image[x, y] == color)
					{
						var leftBorderFound = false;
						var rightBorderFound = false;
						// Ok we found a pixel with the given color, let's check if it's encapsulated
						var maxDistance = maxDistanceToSearch ?? Math.Max(x, imageWidth - x);
						for (int increment = 1; increment < maxDistance; increment++)
						{
							// Check left
							var xLeft = x - increment;
							if (xLeft >= 0 && !leftBorderFound)
							{
								if (IsColorWithinTolerance(image[xLeft, y], borderColor, colorTolerance))
								{
									leftBorderFound = true;
								}
							}

							// Check right
							var xRight = x + increment;
							if (xRight <= (imageWidth - 1) && !rightBorderFound)
							{
								if (IsColorWithinTolerance(image[xRight, y], borderColor, colorTolerance))
								{
									rightBorderFound = true;
								}
							}

							if (!leftBorderFound || !rightBorderFound) continue;
							yield return new Point(x, y);
							leftBorderFound = rightBorderFound = false;
							// We do not have to check more pixels as they lay within the same border
							x = Math.Min(xRight, imageWidth);
						}
					}
				}
			}
		}

		private static bool IsColorWithinTolerance(Rgba32 pixel, Rgba32 color, int tolerance)
		{
			return ((pixel.R >= color.R - tolerance) && (pixel.R <= color.R + tolerance) &&
					(pixel.G >= color.G - tolerance) && (pixel.G <= color.G + tolerance) &&
					(pixel.G >= color.B - tolerance) && (pixel.B <= color.B + tolerance));
		}

		private static IEnumerable<Point> PixelsWithColorAtBorder(Image<Rgba32> image, Rgba32 color)
		{
			var imageHeight = image.Height;
			var imageWidth = image.Width;
			for (int y = 0; y < imageHeight; y++)
			{
				if (y > 0 && y < (imageHeight - 1))
				{
					if (image[0, y] == color)
					{
						yield return new Point(0, y);
					}
					if (image[imageWidth - 1, y] == color)
					{
						yield return new Point(imageWidth - 1, y);
					}

					continue;
				}

				for (int x = 0; x < imageWidth; x++)
				{
					if (image[x, y] == color)
					{
						yield return new Point(x, y);
					}
				}
			}
		}

	    public void Dispose()
	    {
		    Context?.Dispose();
	    }

	    private class OcrResult<T>
		{
			public OcrResult(bool isSuccess, string ocrValue, KeyValuePair<T, double>[] results = null)
			{
				IsSuccess = isSuccess;
				OcrValue = ocrValue;
				Results = results;
			}

			public bool IsSuccess { get; }
			public KeyValuePair<T, double>[] Results { get; }
			private string OcrValue { get; }

			public T GetFirst()
			{
				return Results[0].Key;
			}
		}

		private class RaidOcrResult
		{
			public OcrResult<int> EggLevel { get; set; }
			public OcrResult<TimeSpan> EggTimer { get; set; }
			public OcrResult<Forts> Gym { get; set; }
			public OcrResult<IPokemon> Pokemon { get; set; }
			public OcrResult<TimeSpan> RaidTimer { get; set; }
		}
	}
}
