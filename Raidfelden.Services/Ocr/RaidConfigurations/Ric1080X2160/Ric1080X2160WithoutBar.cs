using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace Raidfelden.Services.Ocr.RaidConfigurations.Ric1080X2160
{
	public class Ric1080X2160WithoutBar : RaidImageConfiguration
	{
		protected override Rectangle GymNamePosition => new Rectangle(210, 100, 870, 130);
		protected override Rectangle EggTimerPosition => new Rectangle(410, 500, 260, 80);
		protected override Rectangle EggLevelPosition => new Rectangle(285, 666, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 540, 1080, 150);
		protected override Rectangle PokemonCpPosition => new Rectangle(220, 370, 700, 160);
		protected override Rectangle RaidTimerPosition => new Rectangle(825, 1270, 170, 50);

		public Ric1080X2160WithoutBar() : base(1080, 2160) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image) { }
	}
}