using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace Raidfelden.Services.Ocr.RaidConfigurations.Ric1080X2220
{
	public class Ric1080X2220BottomBar : RaidImageConfiguration
	{
		protected override Rectangle EggTimerPosition => new Rectangle(400, 470, 270, 70);
		protected override Rectangle EggLevelPosition => new Rectangle(285, 625, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 520, 1080, 150);
		protected override Rectangle RaidTimerPosition => new Rectangle(820, 1235, 180, 50);

		public Ric1080X2220BottomBar() : base(1080, 2220) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image) {}
	}
}