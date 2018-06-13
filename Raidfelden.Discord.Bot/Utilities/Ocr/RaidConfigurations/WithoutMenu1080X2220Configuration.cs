using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace Raidfelden.Discord.Bot.Utilities.Ocr.RaidConfigurations
{
	public class WithoutMenu1080X2220Configuration : RaidImageConfiguration
	{
		protected override Rectangle EggTimerPosition => new Rectangle(410, 535, 260, 70);
		protected override Rectangle EggLevelPosition => new Rectangle(285, 695, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 550, 1080, 170);
		protected override Rectangle RaidTimerPosition => new Rectangle(820, 1300, 180, 50);

		public WithoutMenu1080X2220Configuration() : base(1080, 2220) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image) { }
	}
}