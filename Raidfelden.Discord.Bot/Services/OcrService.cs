using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Extensions.ImageSharp;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Binarization;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using Tesseract;

namespace Raidfelden.Discord.Bot.Services
{
	public interface IOcrService
	{
		Task<ServiceResponse> AddRaidAsync(string filePath, int interactiveLimit, FenceConfiguration[] fences, bool testMode);
	}

	public class OcrService : IOcrService, IDisposable
    {
	    protected TesseractEngine Engine { get; }
	    protected Hydro74000Context Context { get; }
	    protected IGymService GymService { get; }
	    protected IPokemonService PokemonService { get; }
        protected IRaidService RaidService { get; }
        protected bool SaveDebugImages { get; private set; }

	    public OcrService(Hydro74000Context context, IGymService gymService, IPokemonService pokemonService, IRaidService raidService)
	    {
		    Engine = new TesseractEngine(@"./tessdata", "deu+eng", EngineMode.Default, "bazaar");
			Context = context;
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
				double probability = 1;
				var gymName = await GetFragmentStringAsync(image, configuration, ImageFragmentType.GymName, Engine, Context, fences);
				probability *= gymName.Probability;
				var timerValue = await GetFragmentStringAsync(image, configuration, ImageFragmentType.EggTimer, Engine, Context, fences);
				var isRaidboss = !timerValue.IsSuccess || !TimeSpan.TryParse(timerValue.Value, out TimeSpan timer);
				if (isRaidboss)
				{
					var pokemonName = await GetFragmentStringAsync(image, configuration, ImageFragmentType.PokemonName, Engine, Context, fences);
					probability *= pokemonName.Probability;
					timerValue = await GetFragmentStringAsync(image, configuration, ImageFragmentType.RaidTimer, Engine, Context, fences);
					probability *= timerValue.Probability;
					timer = TimeSpan.Parse(timerValue.Value);
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
					probability *= timerValue.Probability;
					var eggLevel = await GetFragmentStringAsync(image, configuration, ImageFragmentType.EggLevel, Engine, Context, fences);
					probability *= eggLevel.Probability;
					if (!testMode)
					{
						return await RaidService.AddAsync(gymName.Value, eggLevel.Value, timer.ToString(@"mm\:ss"), interactiveLimit, fences);
					}
					else{message =$".raids add \"{gymName.Value}\" \"{eggLevel.Value}\" {string.Concat(timer.Minutes, ":", timer.Seconds)}";}
				}

				var result = new ServiceResponse(true, message);
				if (probability < 0.3)
				{
					//result.InterActiveCallbacks.Add();
					result = new ServiceResponse(true, probability.ToString());
				}
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
					configuration = new BottomMenu1080X2220Configuration();
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

	    private async Task<OcrResult> GetFragmentStringAsync(Image<Rgba32> image, BaseRaidImageConfiguration imageConfiguration, ImageFragmentType fragmentType, TesseractEngine engine, Hydro74000Context context, FenceConfiguration[] fences = null)
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
						result= await GetGymName(imageFragment, engine, context, fences);
						break;
					case ImageFragmentType.PokemonName:
						result= await GetPokemonName(imageFragment, imageConfiguration, engine);
						break;
					case ImageFragmentType.EggTimer:
					case ImageFragmentType.RaidTimer:
						result= await GetTimerValue(imageFragment, engine, fragmentType);
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

			return await Task.FromResult(new OcrResult(whitePixelCount > 0, whitePixelCount.ToString(CultureInfo.InvariantCulture), 1));
		}

		private async Task<OcrResult> GetGymName(Image<Rgba32> imageFragment, TesseractEngine engine, Hydro74000Context context, FenceConfiguration[] fences)
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

			imageFragment.Mutate(m => m.Resize(resizeOptions).Invert().BinaryThreshold(0.15f));

			if (SaveDebugImages)
			{
				imageFragment.Save($"_{ImageFragmentType.GymName}_Step2_Binary.png");
			}

			var ocrResult = await GetOcrResultAsync(imageFragment, engine);
			if (!ocrResult.IsSuccess)
			{
				return ocrResult;
			}
			var similarGym = GymService.GetSimilarGymsByNameAsync(context, ocrResult.Value, fences, 1).Result;
			if (similarGym.Count == 0)
			{
				return new OcrResult(false);
			}
			var gym = similarGym.First();
			var value = gym.Key.Name;
			var probability = gym.Value; // perhaps multiply here with the confivence oft the ocr itself
			return new OcrResult(true, value, probability);
			//imageFragment.Mutate(m => m.Hue(180).Resize(resizeOptions));
			//// if the image is a bit dark apply a bit of brightness to get a better readable Text after the BinaryThreshold
			//var pixelData = GetPixelArray(imageFragment);
			//var avgColor = pixelData.Average(e => e.Average(f => f.R + f.G + f.B));
			//var averageColor = new Rgb24(80, 80, 80);
			//if (avgColor < (averageColor.R + averageColor.G + averageColor.B))
			//{
			//	imageFragment.Mutate(m => m.Brightness(2f));
			//	// Perhaps use .Opacity(0.8f) as it also allowed the OCR to recognize "Theilsiefje Säule"
			//}
		}

		private async Task<OcrResult> GetPokemonName(Image<Rgba32> imageFragment, BaseRaidImageConfiguration imageConfiguration, TesseractEngine engine)
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

			var ocrResult = await GetOcrResultAsync(imageFragment, engine);
			if (!ocrResult.IsSuccess)
			{
				return ocrResult;
			}
			var similarPokemon = PokemonService.GetSimilarRaidbossByNameAsync(ocrResult.Value, 1).Result;
			if (similarPokemon.Count == 0)
			{
				return new OcrResult(false);
			}
			var pokemon = similarPokemon.First();
			var value = pokemon.Key.Name;
			var probability = pokemon.Value; // perhaps multiply here with the confidence of the ocr itself
			return new OcrResult(true, value, probability);
		}

		private async Task<OcrResult> GetTimerValue(Image<Rgba32> imageFragment, TesseractEngine engine, ImageFragmentType imageFragmentType)
	    {
			imageFragment.Mutate(m => m.Invert().BinaryThreshold(0.1f));
			if (SaveDebugImages)
			{
				imageFragment.Save($"_{imageFragmentType}_Step1_Binary.png");
			}
			return await GetOcrResultAsync(imageFragment, engine);
		}


		private async Task<OcrResult> GetOcrResultAsync(Image<Rgba32> imageFragment, TesseractEngine engine)
	    {
			var tempImageFile = CreateTempImageFile(imageFragment);
		    try
		    {
				using (var tempImage = Pix.LoadFromFile(tempImageFile))
				{
					using (var page = engine.Process(tempImage))
					{
						var value = RemoveUnwantedCharacters(page.GetText());
						var probability = page.GetMeanConfidence();
						return await Task.FromResult(new OcrResult(probability > 0, value, probability));
					}
				}
			}
		    finally
		    {
				System.IO.File.Delete(tempImageFile);
			}
		}

		private static string CreateTempImageFile<TPixel>(Image<TPixel> image) where TPixel : struct, IPixel<TPixel>
		{
			var tempImageFile = System.IO.Path.GetTempFileName() + ".png";
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
		    Engine?.Dispose();
		    Context?.Dispose();
	    }

	    private class OcrResult
		{
			public OcrResult(bool isSuccess, string value = null, double probability = 0)
			{
				IsSuccess = isSuccess;
				Value = value;
				Probability = probability;
			}

			public string Value { get; }
			public double Probability { get; }
			public bool IsSuccess { get; }
		}
	}
}
