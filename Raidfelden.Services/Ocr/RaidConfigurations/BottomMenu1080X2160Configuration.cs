using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace Raidfelden.Services.Ocr.RaidConfigurations
{
	public class BottomMenu1080X2160Configuration : RaidImageConfiguration
	{
		// - 144
		//protected override Rectangle EggTimerPosition => new Rectangle(410, 579 - BottomMenuHeight, 260, 70);

		//protected override Rectangle EggLevelPosition => new Rectangle(285, 739 - BottomMenuHeight, 510, 80);

		protected override Rectangle EggTimerPosition => new Rectangle(410, 569 - BottomMenuHeight, 260, 70);

		protected override Rectangle EggLevelPosition => new Rectangle(285, 727 - BottomMenuHeight, 510, 80);

		//protected override Rectangle PokemonNamePosition => new Rectangle(0, 644 - BottomMenuHeight, 1080, 140);

		protected override Rectangle PokemonNamePosition => new Rectangle(0, 630 - BottomMenuHeight, 1080, 140);

		protected override Rectangle PokemonCpPosition => new Rectangle(220, 472 - BottomMenuHeight, 700, 140);

		//protected override Rectangle RaidTimerPosition => new Rectangle(825, 1344 - BottomMenuHeight, 170, 50);

		protected override Rectangle RaidTimerPosition => new Rectangle(825, 1334 - BottomMenuHeight, 170, 50);

		public BottomMenu1080X2160Configuration() : base(1080, 2160) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image)
		{
			// 143 works
			// 120 does not
			BottomMenuHeight = (image as Image<Rgba32>).GetBottomBarHeight();
		}
	}
}