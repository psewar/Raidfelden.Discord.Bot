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

namespace Raidfelden.Discord.Bot.Services
{
	public interface IOcrService
	{
		Task<ServiceResponse> AddRaidAsync(string filePath, int interactiveLimit, FenceConfiguration[] fences, bool testMode);
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

	    public async Task<ServiceResponse> AddRaidAsync(string filePath, int interactiveLimit, FenceConfiguration[] fences, bool testMode)
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
				var gymName = await GetFragmentResultAsync(image, configuration, ImageFragmentType.GymName, Context, fences);
				var timerValue = await GetFragmentResultAsync(image, configuration, ImageFragmentType.EggTimer, Context, fences);
				var isRaidboss = !timerValue.IsSuccess || !TimeSpan.TryParse(timerValue.Value, out TimeSpan timer);
				if (isRaidboss)
				{
					timerValue = await GetFragmentResultAsync(image, configuration, ImageFragmentType.RaidTimer, Context, fences);
					if (!timerValue.IsSuccess)
					{
						return new ServiceResponse(false, "Die Restzeit konnte nich erkannt werden.");
					}
					timer = TimeSpan.Parse(timerValue.Value);
					var pokemonName = await GetFragmentResultAsync(image, configuration, ImageFragmentType.PokemonName, Context, fences);
					if (!pokemonName.IsSuccess)
					{
						return new ServiceResponse(false, "Der Raidboss konnte nich erkannt werden.");
					}

					if (!testMode)
					{
						return await RaidService.AddAsync(gymName.Value, pokemonName.Value, timer.ToString(@"mm\:ss"), interactiveLimit, fences);
					}
					else
					{
						message = $".raids add \"{gymName.Value}\" \"{pokemonName.Value}\" {string.Concat(timer.Minutes, ":", timer.Seconds)}";
					}
				}
				else
				{
					var eggLevel = await GetFragmentResultAsync(image, configuration, ImageFragmentType.EggLevel, Context, fences);
					if (!eggLevel.IsSuccess)
					{
						return new ServiceResponse(false, "Das Level konnte nich erkannt werden.");
					}

					if (!testMode)
					{
						return await RaidService.AddAsync(gymName.Value, eggLevel.Value, timer.ToString(@"mm\:ss"), interactiveLimit, fences);
					}
					else{message =$".raids add \"{gymName.Value}\" \"{eggLevel.Value}\" {string.Concat(timer.Minutes, ":", timer.Seconds)}";}
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

	    private async Task<OcrResult> GetFragmentResultAsync(Image<Rgba32> image, BaseRaidImageConfiguration imageConfiguration, ImageFragmentType fragmentType, Hydro74000Context context, FenceConfiguration[] fences = null)
	    {
		    using (var imageFragment = image.Clone(e => e.Crop(imageConfiguration[fragmentType])))
		    {
			    if (SaveDebugImages)
			    {
				    imageFragment.Save($"_{fragmentType}_Created.png");
			    }
			    var result = new OcrResult(false);
			    switch (fragmentType)
			    {
					case ImageFragmentType.EggLevel:
						result= await GetEggLevel(imageFragment, imageConfiguration);
					    break;
					case ImageFragmentType.GymName:
						result= await GetGymName(imageFragment, context, fences);
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
		    }
	    }

		private async Task<OcrResult> GetEggLevel(Image<Rgba32> imageFragment, BaseRaidImageConfiguration imageConfiguration)
	    {
			byte whiteThreshold = 250;
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
				return await Task.FromResult(new OcrResult(false));
		    }

		    var results = new[] {new KeyValuePair<string, double>(whitePixelCount.ToString(CultureInfo.InvariantCulture), 1)};
			return await Task.FromResult(new OcrResult(true, results));
		}

		private async Task<OcrResult> GetGymName(Image<Rgba32> imageFragment, Hydro74000Context context, FenceConfiguration[] fences)
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
			if (!ocrResult.IsSuccess)
			{
				return ocrResult;
			}

			var ocrValue = ocrResult.Results[0].Key;
			var similarGyms = GymService.GetSimilarGymsByNameAsync(context, ocrValue, fences, 1).Result;
			if (similarGyms.Count == 0)
			{
				return new OcrResult(false);
			}

			var results = similarGyms.Select(kvp => new KeyValuePair<string, double>(kvp.Key.Name, kvp.Value)).ToArray();
			return new OcrResult(true, results);
		}

		private async Task<OcrResult> GetPokemonName(Image<Rgba32> imageFragment, BaseRaidImageConfiguration imageConfiguration)
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
			if (!ocrResult.IsSuccess)
			{
				return ocrResult;
			}

			var ocrValue = ocrResult.Results[0].Key;
			var similarPokemon = PokemonService.GetSimilarRaidbossByNameAsync(ocrValue, 1).Result;
			if (similarPokemon.Count == 0)
			{
				return new OcrResult(false);
			}

			var results = similarPokemon.Select(kvp => new KeyValuePair<string, double>(kvp.Key.Name, kvp.Value)).ToArray();
			return new OcrResult(true, results);
		}

		private async Task<OcrResult> GetTimerValue(Image<Rgba32> imageFragment, ImageFragmentType imageFragmentType)
	    {
			imageFragment.Mutate(m => m.Invert().BinaryThreshold(0.1f));
			if (SaveDebugImages)
			{
				imageFragment.Save($"_{imageFragmentType}_Step1_Binary.png");
			}
			return await GetOcrResultAsync(imageFragment);
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


        private async Task<OcrResult> GetOcrResultAsync(Image<Rgba32> imageFragment)
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
                arguments.Append(" -l" + languages);    // Languages.

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

            return
                await Task.FromResult(new OcrResult(probability > 0,
                    new[] { new KeyValuePair<string, double>(value, probability), }));
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

	    private class OcrResult
		{
			public OcrResult(bool isSuccess, KeyValuePair<string, double>[] results = null)
			{
				IsSuccess = isSuccess;
				Results = results;
			}

			public bool IsSuccess { get; }
			public KeyValuePair<string, double>[] Results { get; }
			public string Value { get { return Results[0].Key; } }
		}
	}
}
