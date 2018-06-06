using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Raidfelden.Discord.Bot.Configuration;
using Raidfelden.Discord.Bot.Extensions.ImageSharp;
using Raidfelden.Discord.Bot.Monocle;
using Raidfelden.Discord.Bot.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Binarization;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using Tesseract;
using SixLabors.ImageSharp.Processing.Convolution;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Drawing.Brushes;
using SixLabors.ImageSharp.Processing.Transforms.Resamplers;

using CoreBrushes = SixLabors.ImageSharp.Processing.Drawing.Brushes.Brushes;

namespace Raidfelden.Discord.Bot.Utilities
{
    public class RaidImage<TPixel> : IDisposable where TPixel : struct, IPixel<TPixel>
    {
        // Base rectangles for 1080 * 1920 Screen-Size
        protected Rectangle BaseGymNameRectangle = new Rectangle(220, 115, 860, 90);
        protected Rectangle BaseRaidLevelRectangle = new Rectangle(370, 260, 350, 70);
        //protected Rectangle BasePokemonNameRectangle = new Rectangle(0, 480, 1080, 105);
        protected Rectangle BasePokemonNameRectangle = new Rectangle(0, 480, 1080, 140);
        protected Rectangle BaseRaidTimerRectangle = new Rectangle(820, 1150, 180, 50);
        protected Rectangle BaseEggTimerRectangle = new Rectangle(400, 385, 270, 70);
        protected Rectangle BaseEggLevelRectangle = new Rectangle(285, 545, 510, 80);

        // Points to detect the raid level on non hatched Raid Images these contain the lower ones also
        protected List<Point> BaseLevel5Points = new List<Point> { new Point(42, 24), new Point(148, 24), new Point(254, 24), new Point(360, 24), new Point(466, 24) };
        protected List<Point> BaseLevel4Points = new List<Point> { new Point(95, 24), new Point(202, 24), new Point(308, 24), new Point(414, 24) };

        public Dictionary<ImageFragmentType, Rectangle> FragmentLocations { get; }
        public List<Point> Level5Points { get; }
        public List<Point> Level4Points { get; }

        public Image<TPixel> Image { get; }

		protected IGymService GymService { get; }
		protected IPokemonService PokemonService { get; }
        protected BaseRaidImageConfiguration ImageConfiguration { get; }

        private BaseRaidImageConfiguration GetConfiguration<TPixel>(Image<TPixel> image) where TPixel : struct, IPixel<TPixel>
        {
            var configuration = new BaseRaidImageConfiguration(1080, 1920);
            if (image.Height == 2220)
            {
                //configuration = new GalaxyS9PlusRaidImageConfiguration();
            }

            if (image.Height == 2960 && HasBottomMenu(image as Image<Rgba32>))
            {
                configuration = new GalaxyS9BottomMenuImageConfiguration();
            }
            return configuration;
        }

        private bool HasBottomMenu(Image<Rgba32> image) 
        {
            if (image[0, image.Height - 1] == Rgba32.Black)
            {
                return true;
            }
            return false;
        }

        public RaidImage(Image<TPixel> image, IGymService gymService, IPokemonService pokemonService)
        {
            Image = image;
	        GymService = gymService;
			PokemonService = pokemonService;

            ImageConfiguration = GetConfiguration(image);

            ImageConfiguration.PreProcessImage(image);
            /*
            var resizeOptions = new ResizeOptions
            {
                Mode = ResizeMode.Pad,
                Size = new Size(1080, 1920),
                Compand = true,
                //Sampler = new WelchResampler()
            };
            image.Mutate(m => m.Resize(resizeOptions));*/
            
#if DEBUG
            image.Save("_Image.png");
#endif

            // Detect the aspect ratio of the image, could be helpfull when resizing the image or the rects
            var gcdBase = GreatestCommonDivisor(1080, 1920);
            var aspectRatioBase = $"{1080 / gcdBase} x {1920 / gcdBase}";

            FragmentLocations = new Dictionary<ImageFragmentType, Rectangle>(5)
            {
				// Common Locations
				{ ImageFragmentType.GymName, ResizeRectangleToImage(BaseGymNameRectangle, image) },
				// Raidboss Locations
				{ ImageFragmentType.PokemonName, ResizeRectangleToImage(BasePokemonNameRectangle, image) },
                { ImageFragmentType.RaidTimer, ResizeRectangleToImage(BaseRaidTimerRectangle, image) },
				// Egg Locations
				{ ImageFragmentType.EggTimer, ResizeRectangleToImage(BaseEggTimerRectangle, image) },
                { ImageFragmentType.EggLevel, ResizeRectangleToImage(BaseEggLevelRectangle, image) }
            };

            Level5Points = BaseLevel5Points.Select(e => ResizePointToImage(e, image)).ToList();
            Level4Points = BaseLevel4Points.Select(e => ResizePointToImage(e, image)).ToList();
        }

        public string GetFragmentString(TesseractEngine engine, ImageFragmentType fragmentType, Hydro74000Context context, FenceConfiguration[] fences = null)
        {
	        bool saveTestImages = false;
#if DEBUG
	        saveTestImages = true;
#endif
			using (var imageFragment = Image.Clone(e => e.Crop(ImageConfiguration[fragmentType])))
            {
                if (fragmentType == ImageFragmentType.EggLevel)
                {
                    byte whiteThreshold = 250;
                    var pixels = GetPixelArray(imageFragment);
                    // Check the locations for level 1, 3 and 5 raids
                    var whitePixelCount = Level5Points.Select(levelPoint => pixels[levelPoint.Y][levelPoint.X]).Count(pixel => pixel.R > whiteThreshold && pixel.G > whiteThreshold && pixel.B > whiteThreshold && pixel.A > whiteThreshold);

                    // No white pixels found so lets check the locations for level 2 and 4 raids
                    if (whitePixelCount == 0)
                    {
                        whitePixelCount += Level4Points.Select(levelPoint => pixels[levelPoint.Y][levelPoint.X]).Count(pixel => pixel.R > whiteThreshold && pixel.G > whiteThreshold && pixel.B > whiteThreshold && pixel.A > whiteThreshold);
                    }

                    return whitePixelCount.ToString(CultureInfo.InvariantCulture);
                }

                if (fragmentType == ImageFragmentType.GymName)
                {
	                if (saveTestImages)
	                {
		                imageFragment.Save("_" + fragmentType + "_BeforeResize.png");
	                }
                    var multiplier = 2;
                    var size = new Size(imageFragment.Width * multiplier, imageFragment.Height * multiplier);
                    var resizeOptions = new ResizeOptions
                    {
                        Mode = ResizeMode.Stretch,
                        Size = size,
                        Compand = true,
                        Sampler = KnownResamplers.Welch
                    };

                    imageFragment.Mutate(m => m.Hue(180).Resize(resizeOptions));
					// if the image is a bit dark apply a bit of brightness to get a better readable Text after the BinaryThreshold
					var pixelData = GetPixelArray(imageFragment);
					var avgColor = pixelData.Average(e => e.Average(f => f.R + f.G + f.B));
					var averageColor = new Rgb24(80, 80, 80);
					if (avgColor < (averageColor.R + averageColor.G + averageColor.B))
	                {
		                imageFragment.Mutate(m => m.Brightness(2f));
						// Perhaps use .Opacity(0.8f) as it also allowed the OCR to recognize "Theilsiefje Säule"
					}
					if (saveTestImages)
					{
						imageFragment.Save("_" + fragmentType + "_AfterResize.png");
					}
				}

	            if (fragmentType == ImageFragmentType.PokemonName)
	            {
					if (saveTestImages)
					{
						imageFragment.Save("_" + fragmentType + "_BeforeResize.png");
					}

					var floodFillLetters = new QueueLinearFloodFiller();
					floodFillLetters.Bitmap = imageFragment as Image<Rgba32>;
					floodFillLetters.FillColor = Rgba32.Black;
					floodFillLetters.Tolerance[0] = 10;
					floodFillLetters.Tolerance[1] = 10;
					floodFillLetters.Tolerance[2] = 10;
					var borderColor = new Rgba32(168, 185, 189, 255);
					foreach (var encapsulatedPixel in GetEncapsulatedPixels(imageFragment as Image<Rgba32>, Rgba32.White, borderColor, 20))
		            {
						floodFillLetters.FloodFill(encapsulatedPixel);
					}

					imageFragment.Save("_" + fragmentType + "_AfterFloodFillLetters.png");

					imageFragment.Mutate(m => m.BinaryThreshold(0.01f));

					imageFragment.Save("_" + fragmentType + "_AfterFloodFillLetters_Binary.png");

					imageFragment.Mutate(m => m.Invert());
					//var pixelData = GetPixelArray(imageFragment);
					//var avgColor = pixelData.Average(e => e.Average(f => f.R + f.G + f.B));
					//var averageColor = new Rgb24(235, 235, 235);
					//if (avgColor > (averageColor.R + averageColor.G + averageColor.B))
					//{
					//	// Brightness(0.5f) == Brightness -200 in paint.net
					//	imageFragment.Mutate(m => m.GaussianBlur().BinaryThreshold(0.9f));
					//	//imageFragment.Mutate(m => m.GaussianBlur().Brightness(0.5f).Contrast(200f));
					//	//imageFragment.Mutate(m => m.GaussianBlur().BinaryThreshold(0.9f).Invert().Fill(CoreBrushes.BackwardDiagonal(Rgba32.White) as IBrush<TPixel>));
					//	//imageFragment.Mutate(x => x.Fill(CoreBrushes.BackwardDiagonal(Rgba32.HotPink) as IBrush<TPixel>));
					//	var floodFill = new QueueLinearFloodFiller();
					//	floodFill.Bitmap = imageFragment as Image<Rgba32>;
					//	floodFill.FillColor = Rgba32.Black;
					//	floodFill.FloodFill(new Point(0, 0));
					//	floodFill.FloodFill(new Point(imageFragment.Width-1, imageFragment.Height-1));
					//}

					//FloodFill Regions of the image which got a black entry from the border
					if (saveTestImages)
					{
						imageFragment.Save("_" + fragmentType + "_BeforeFloodFill.png");
					}
					var floodFillBorders = new QueueLinearFloodFiller();
					floodFillBorders.Bitmap = imageFragment as Image<Rgba32>;
					floodFillBorders.FillColor = Rgba32.Black;
					floodFillBorders.Tolerance[0] = 1;
					floodFillBorders.Tolerance[1] = 1;
					floodFillBorders.Tolerance[2] = 1;
					foreach (var point in PixelsWithColorAtBorder(imageFragment as Image<Rgba32>, Rgba32.White))
					{
						floodFillBorders.FloodFill(point);
					}
					//imageFragment.Mutate(m => m.Invert());

					//imageFragment.Mutate(m => m.Brightness(0.9f));
					if (saveTestImages)
					{
						imageFragment.Save("_" + fragmentType + "_AfterResize.png");
					}
				}

                // Run OCR
                imageFragment.Mutate(e => e.Invert());
                if (fragmentType == ImageFragmentType.PokemonName)
                {
                    imageFragment.Mutate(m => m.BinaryThreshold(0.000000000000000000000000000001f));
                    //imageFragment.Mutate(m => m.BinaryThreshold(0.1f));
                }
                else
                {
                    imageFragment.Mutate(m => m.BinaryThreshold(0.1f));
                }

	            if (saveTestImages)
	            {
		            imageFragment.Save("_" + fragmentType.ToString() + ".png");
	            }

	            var tempImageFile = CreateTempImageFile(imageFragment);
                string ocrResult = string.Empty;
                using (var tempImage = Pix.LoadFromFile(tempImageFile))
                {
                    using (var page = engine.Process(tempImage))
                    {
                        ocrResult = RemoveUnwantedCharacters(page.GetText());
	                    switch (fragmentType)
	                    {
							case ImageFragmentType.GymName:
			                    var similarGym = GymService.GetSimilarGymsByNameAsync(context, ocrResult, fences, 1).Result;
								ocrResult = similarGym.First().Key.Name;
								break;
							case ImageFragmentType.PokemonName:
								var similarPokemon = PokemonService.GetSimilarPokemonByNameAsync(ocrResult, 1).Result;
								ocrResult = similarPokemon.First().Key.Name;
								break;
	                    }
                    }
                }
                System.IO.File.Delete(tempImageFile);
                return ocrResult;
            }
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
					if (image[imageWidth-1, y] == color)
					{
						yield return new Point(imageWidth-1, y);
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

	    private static IEnumerable<Point> GetEncapsulatedPixels(Image<Rgba32> image, Rgba32 color, Rgba32 borderColor, int? maxDistanceToSearch = null)
	    {
		    int colorTolerance = 30;
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
								if (IsColorWithTolerance(image[xLeft, y], borderColor, colorTolerance))
								{
									leftBorderFound = true;
								}
							}

							// Check right
							var xRight = x + increment;
							if (xRight <= (imageWidth -1) && !rightBorderFound)
							{
								if (IsColorWithTolerance(image[xRight, y], borderColor, colorTolerance))
								{
									rightBorderFound = true;
								}
							}

							if (leftBorderFound && rightBorderFound)
							{
								yield return new Point(x, y);
								// We do not have to check more pixels as they lay within the same border
								x = xRight;
							}
						}
					}
				}
			}
		}

	    private static bool IsColorWithTolerance(Rgba32 pixel, Rgba32 color, int tolerance)
	    {
			return ((pixel.R >= color.R - tolerance) && (pixel.R <= color.R + tolerance) &&
					(pixel.G >= color.G - tolerance) && (pixel.G <= color.G + tolerance) &&
					(pixel.G >= color.B - tolerance) && (pixel.B <= color.B + tolerance));
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

        public Image<TPixel> GetFragmentImage(ImageFragmentType fragmentType)
        {
            return Image.Clone(e => e.Crop(FragmentLocations[fragmentType]));
        }

        private static Point ResizePointToImage(Point source, Image<TPixel> image)
        {
            return new Point(source.X * image.Width / 1080, source.Y * image.Height / 1920);
        }

        private static Rectangle ResizeRectangleToImage(Rectangle source, Image<TPixel> image)
        {
            return new Rectangle(source.X * image.Width / 1080, source.Y * image.Height / 1920, source.Width * image.Width / 1080, source.Height * image.Height / 1920);
        }

        private static int GreatestCommonDivisor(int a, int b)
        {
            return (b == 0) ? a : GreatestCommonDivisor(b, a % b);
        }

        public void Dispose()
        {
            Image?.Dispose();
        }

        private static Rgba32[][] GetPixelArray<TPixel>(Image<TPixel> bitmap) where TPixel : struct, IPixel<TPixel>
        {
            var result = new Rgba32[bitmap.Height][];
            TPixel[] buffer = new TPixel[bitmap.Height * bitmap.Width];
            bitmap.SavePixelData(buffer);
            for (int y = 0; y < bitmap.Height; ++y)
            {
                result[y] = new Rgba32[bitmap.Width];
                for (int x = 0; x < bitmap.Width; ++x)
                {
                    var rgba32 = new Rgba32();
                    buffer[y * bitmap.Width + x].ToRgba32(ref rgba32);
                    result[y][x] = rgba32;
                }
            }
            return result;
        }
    }

    public enum ImageFragmentType
    {
        GymName,
        PokemonName,
        RaidTimer,
        EggTimer,
        EggLevel
    }
}