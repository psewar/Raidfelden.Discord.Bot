using System.Collections.Generic;
using SixLabors.Primitives;

namespace Raidfelden.Services.Ocr.RaidConfigurations
{
	public class BottomBarSmall1080X1920Configuration : RaidImageConfiguration
	{
		//protected override Rectangle GymNamePosition => new Rectangle(210, 115, 870, 90);
		protected override Rectangle GymNamePosition => new Rectangle(210, 85, 870, 100);
		protected override Rectangle EggTimerPosition => new Rectangle(420, 360, 250, 70);
		protected override Rectangle EggLevelPosition => new Rectangle(300, 520, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 445, 1080, 140);
		protected override Rectangle PokemonCpPosition => new Rectangle(220, 310, 700, 140);
		//protected override Rectangle RaidTimerPosition => new Rectangle(805, 1075, 170, 60);
		protected override Rectangle RaidTimerPosition => new Rectangle(810, 1120, 170, 60);

		public override List<Point> Level5Points => new List<Point> { new Point(42, 24), new Point(141, 24), new Point(240, 24), new Point(338, 24), new Point(437, 24) };
		public override List<Point> Level4Points => new List<Point> { new Point(91, 24), new Point(190, 24), new Point(289, 24), new Point(387, 24) };

		public BottomBarSmall1080X1920Configuration() : base(1080, 1920) { }
	}
}