using System.Collections.Generic;
using SixLabors.Primitives;

namespace Raidfelden.Services.Ocr.RaidConfigurations
{
	public class BottomMenu720X1280Configuration : RaidImageConfiguration
	{
		protected override Rectangle GymNamePosition => new Rectangle(220, 110, 860, 90);
		protected override Rectangle EggTimerPosition => new Rectangle(400, 360, 270, 70);
		protected override Rectangle EggLevelPosition => new Rectangle(285, 510, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 445, 1080, 150);
		protected override Rectangle RaidTimerPosition => new Rectangle(805, 1078, 160, 50);

		public BottomMenu720X1280Configuration() : base(720, 1280) { }

		public override List<Point> Level5Points => new List<Point> { new Point(59, 15), new Point(156, 15), new Point(254, 15), new Point(352, 15), new Point(450, 15) };
		public override List<Point> Level4Points => new List<Point> { new Point(95, 24), new Point(202, 24), new Point(308, 24), new Point(414, 24) };
	}
}