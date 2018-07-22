using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace Raidfelden.Services.Ocr.RaidConfigurations.Ric1080X2220
{
	public class Ric1080X2220TopAndBottomBar : RaidImageConfiguration
	{
		protected override Rectangle GymNamePosition => new Rectangle(220, 230, 860, 90);
		protected override Rectangle EggTimerPosition => new Rectangle(400, 500, 270, 70);
		protected override Rectangle EggLevelPosition => new Rectangle(285, 660, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 590, 1080, 150);
		protected override Rectangle RaidTimerPosition => new Rectangle(820, 1265, 180, 50);

		public Ric1080X2220TopAndBottomBar() : base(1080, 2220) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image) { }
	}
}