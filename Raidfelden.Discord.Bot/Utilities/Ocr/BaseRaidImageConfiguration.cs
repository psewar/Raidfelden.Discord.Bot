using System;
using System.Collections.Generic;
using Raidfelden.Discord.Bot.Extensions.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Binarization;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace Raidfelden.Discord.Bot.Utilities.Ocr
{
    public class RaidImageConfiguration
    {
        protected virtual Rectangle GymNamePosition => new Rectangle(220, 115, 860, 90);
        protected virtual Rectangle PokemonNamePosition => new Rectangle(0, 480, 1080, 140);
        protected virtual Rectangle RaidTimerPosition => new Rectangle(820, 1150, 180, 50);
        protected virtual Rectangle EggTimerPosition => new Rectangle(400, 385, 270, 70);
        protected virtual Rectangle EggLevelPosition => new Rectangle(285, 545, 510, 80);

		// Points to detect the raid level on non hatched raid images these contain the lower level ones also
		public virtual List<Point> Level5Points => new List<Point> { new Point(42, 24), new Point(148, 24), new Point(254, 24), new Point(360, 24), new Point(466, 24) };
		public virtual List<Point> Level4Points => new List<Point> { new Point(95, 24), new Point(202, 24), new Point(308, 24), new Point(414, 24) };

		public virtual Rgba32 PokemonNameBorderColor => new Rgba32(168, 185, 189, 255);

		protected virtual int ResizeWidth { get; }
        protected virtual int ResizeHeight { get; }

	    public int OriginalWidth { get; }
        public int OriginalHeight { get; }

	    public bool SaveDebugImages { get; set; } 
		
	    public Rectangle this[RaidImageFragmentType fragmentType]
        {
            get
            {
                switch (fragmentType)
                {
                    case RaidImageFragmentType.EggLevel:
                        return EggLevelPosition;
                    case RaidImageFragmentType.EggTimer:
                        return EggTimerPosition;
                    case RaidImageFragmentType.GymName:
                        return GymNamePosition;
                    case RaidImageFragmentType.PokemonName:
                        return PokemonNamePosition;
                    case RaidImageFragmentType.RaidTimer:
                        return RaidTimerPosition;
                }
                throw new ArgumentException();
            }
        }

        public RaidImageConfiguration(int originalWidth, int originalHeight, int resizeWidth= 1080, int resizeHeight = 1920)
        {
            ResizeWidth = resizeWidth;
            ResizeHeight = resizeHeight;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
        }

        public virtual void PreProcessImage<TPixel>(Image<TPixel> image) where TPixel : struct, IPixel<TPixel>
        {
            var resizeOptions = new ResizeOptions
            {
                Mode = ResizeMode.Pad,
                Size = new Size(ResizeWidth, ResizeHeight),
                Compand = true,
            };
            image.Mutate(m => m.Resize(resizeOptions));
        }

	    public virtual Image<Rgba32> PreProcessGymNameFragment(Image<Rgba32> imageFragment)
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
				imageFragment.Save($"_{RaidImageFragmentType.GymName}_Step1_Resize.png");
			}

			imageFragment.Mutate(m => m.Resize(resizeOptions).Invert().BinaryThreshold(0.2f));

			if (SaveDebugImages)
			{
				imageFragment.Save($"_{RaidImageFragmentType.GymName}_Step2_Binary.png");
			}

		    return imageFragment;
	    }

	    public virtual Image<Rgba32> PreProcessPokemonNameFragment(Image<Rgba32> imageFragment)
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
			var borderColor = PokemonNameBorderColor;
			foreach (var encapsulatedPixel in GetEncapsulatedPixels(imageFragment, Rgba32.White, borderColor, 30))
			{
				floodFillLetters.FloodFill(encapsulatedPixel);
			}

			if (SaveDebugImages)
			{
				imageFragment.Save($"_{RaidImageFragmentType.PokemonName}_Step1_FloodFillLetters.png");
			}

			imageFragment.Mutate(m => m.BinaryThreshold(0.01f).Invert());

			if (SaveDebugImages)
			{
				imageFragment.Save($"_{RaidImageFragmentType.PokemonName}_Step2_Binary.png");
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
				imageFragment.Save($"_{RaidImageFragmentType.PokemonName}_Step3_BlackBorderEntriesRemoved.png");
			}

		    return imageFragment;
	    }

	    public Image<Rgba32> PreProcessTimerFragment(Image<Rgba32> imageFragment, RaidImageFragmentType imageFragmentType)
	    {
			imageFragment.Mutate(m => m.Invert().BinaryThreshold(0.1f));
			if (SaveDebugImages)
			{
				imageFragment.Save($"_{imageFragmentType}_Step1_Binary.png");
			}

		    return imageFragment;
	    }

		protected static IEnumerable<Point> GetEncapsulatedPixels(Image<Rgba32> image, Rgba32 color, Rgba32 borderColor, int? maxDistanceToSearch = null, int colorTolerance = 30)
		{
			var imageHeight = image.Height;
			var imageWidth = image.Width;
			for (var y = 0; y < imageHeight; y++)
			{
				for (var x = 0; x < imageWidth; x++)
				{
					if (image[x, y] != color) continue;
					var leftBorderFound = false;
					var rightBorderFound = false;
					// Ok we found a pixel with the given color, let's check if it's encapsulated
					var maxDistance = maxDistanceToSearch ?? Math.Max(x, imageWidth - x);
					for (var increment = 1; increment < maxDistance; increment++)
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

		protected static bool IsColorWithinTolerance(Rgba32 pixel, Rgba32 color, int tolerance)
		{
			return ((pixel.R >= color.R - tolerance) && (pixel.R <= color.R + tolerance) &&
					(pixel.G >= color.G - tolerance) && (pixel.G <= color.G + tolerance) &&
					(pixel.G >= color.B - tolerance) && (pixel.B <= color.B + tolerance));
		}

		protected static IEnumerable<Point> PixelsWithColorAtBorder(Image<Rgba32> image, Rgba32 color)
		{
			var imageHeight = image.Height;
			var imageWidth = image.Width;
			for (var y = 0; y < imageHeight; y++)
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

				for (var x = 0; x < imageWidth; x++)
				{
					if (image[x, y] == color)
					{
						yield return new Point(x, y);
					}
				}
			}
		}
	}
}