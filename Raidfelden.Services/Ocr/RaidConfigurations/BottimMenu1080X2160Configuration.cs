using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace Raidfelden.Services.Ocr.RaidConfigurations
{
	public class BottomMenu1080X2160Configuration : RaidImageConfiguration
	{
		protected override Rectangle EggTimerPosition => new Rectangle(410, 435, 260, 70);

		protected override Rectangle EggLevelPosition => new Rectangle(285, 595, 510, 80);

		protected override Rectangle PokemonNamePosition => new Rectangle(0, 500, 1080, 140);

		protected override Rectangle RaidTimerPosition => new Rectangle(825, 1200, 170, 50);

		public BottomMenu1080X2160Configuration() : base(1080, 2160) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image) { }
	}
}