using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Binarization;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace Raidfelden.Discord.Bot.Utilities
{
    public class BaseRaidImageConfiguration
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

        public Rectangle this[ImageFragmentType fragmentType]
        {
            get
            {
                switch (fragmentType)
                {
                    case ImageFragmentType.EggLevel:
                        return EggLevelPosition;
                    case ImageFragmentType.EggTimer:
                        return EggTimerPosition;
                    case ImageFragmentType.GymName:
                        return GymNamePosition;
                    case ImageFragmentType.PokemonName:
                        return PokemonNamePosition;
                    case ImageFragmentType.RaidTimer:
                        return RaidTimerPosition;
                }
                throw new ArgumentException();
            }
        }

        public BaseRaidImageConfiguration(int originalWidth, int originalHeight, int resizeWidth= 1080, int resizeHeight = 1920)
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
    }

    public class GalaxyS9PlusRaidImageConfiguration : BaseRaidImageConfiguration
    {
        protected override Rectangle PokemonNamePosition => new Rectangle(0, 520, 1080, 150);
        protected override Rectangle RaidTimerPosition => new Rectangle(820, 1235, 180, 50);

        public GalaxyS9PlusRaidImageConfiguration() : base(1080, 2220) { }

        public override void PreProcessImage<TPixel>(Image<TPixel> image)
        {
            return;
        }
    }

    public class GalaxyS9BottomMenuImageConfiguration : BaseRaidImageConfiguration
    {
        public GalaxyS9BottomMenuImageConfiguration() : base(1080, 1920) { }

        public override void PreProcessImage<TPixel>(Image<TPixel> image)
        {
            image.Mutate(m => m.Crop(new Rectangle(0, 156, 1440, 2562)));
            base.PreProcessImage(image);
        }
    }

	// ReSharper disable once InconsistentNaming
	public class IPhoneXImageConfiguration : BaseRaidImageConfiguration
	{
		protected override Rectangle EggTimerPosition => new Rectangle(400, 595, 270, 70);
		protected override Rectangle EggLevelPosition => new Rectangle(285, 755, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 580, 1080, 180);
		protected override Rectangle RaidTimerPosition => new Rectangle(820, 1360, 180, 50);

		public IPhoneXImageConfiguration() : base(1125, 2436, 1080, 2339) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image)
		{
			image.Mutate(m => m.Brightness(1.09f));
			base.PreProcessImage(image);
		}
	}
}