using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Binarization;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using Tesseract;
using SixLabors.ImageSharp.Processing.Convolution;
using SixLabors.ImageSharp.Processing.Transforms.Resamplers;

namespace Raidfelden.Discord.Bot.Utilities
{
    public class RaidImage<TPixel> : IDisposable where TPixel : struct, IPixel<TPixel>
    {
        // Base rectangles for 1080 * 1920 Screen-Size
        protected Rectangle BaseGymNameRectangle = new Rectangle(220, 125, 860, 70);
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

        public RaidImage(Image<TPixel> image)
        {
            Image = image;
            var resizeOptions = new ResizeOptions
            {
                Mode = ResizeMode.Pad,
                Size = new Size(1080, 1920),
                Compand = true,
                //Sampler = new WelchResampler()
            };
            image.Mutate(m => m.Resize(resizeOptions));
#if DEBUG
            image.Save("Image.png");
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

        public string GetFragmentString(TesseractEngine engine, ImageFragmentType fragmentType)
        {
            using (var imageFragment = Image.Clone(e => e.Crop(FragmentLocations[fragmentType])))
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
                    // Adding .Opacity(0.8f) would allow the OCR to recognize "Theilsiefje Säule", but will fail on others
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

#if DEBUG
                imageFragment.Save(fragmentType.ToString() + ".png");
#endif

                var tempImageFile = CreateTempImageFile(imageFragment);
                string ocrResult = string.Empty;
                using (var tempImage = Pix.LoadFromFile(tempImageFile))
                {
                    using (var page = engine.Process(tempImage))
                    {
                        ocrResult = RemoveUnwantedCharacters(page.GetText());
                    }
                }
                System.IO.File.Delete(tempImageFile);
                return ocrResult;
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
            input = input.Replace("—", "-");
            var arr = input.ToCharArray();

            arr = Array.FindAll(arr, (c => (char.IsLetterOrDigit(c)
                                         || char.IsWhiteSpace(c)
                                         || c == '-')
                                         || c == ':'));
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