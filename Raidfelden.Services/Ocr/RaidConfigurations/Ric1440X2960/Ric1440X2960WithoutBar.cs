using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System.Collections.Generic;

namespace Raidfelden.Services.Ocr.RaidConfigurations.Ric1440X2960
{
	public class Ric1440X2960WithoutBar : RaidImageConfiguration
	{
		protected override Rectangle GymNamePosition => new Rectangle(290, 140, 1150, 140);
		protected override Rectangle EggTimerPosition => new Rectangle(530, 710, 370, 100);
		protected override Rectangle EggLevelPosition => new Rectangle(380, 930, 680, 105);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 550, 1080, 170);
		protected override Rectangle RaidTimerPosition => new Rectangle(820, 1320, 180, 50);

		public override List<Point> Level5Points => new List<Point> { new Point(55, 30), new Point(200, 30), new Point(340, 30), new Point(480, 30), new Point(620, 30) };
		public override List<Point> Level4Points => new List<Point> { new Point(95, 24), new Point(202, 24), new Point(308, 24), new Point(414, 24) };

		public Ric1440X2960WithoutBar() : base(1440, 2960) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image) { }
	}
}