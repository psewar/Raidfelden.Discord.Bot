using System.Collections.Generic;
using SixLabors.Primitives;

namespace Raidfelden.Discord.Bot.Utilities.Ocr.RaidConfigurations
{
	public class BottomMenu1080X1920Configuration : RaidImageConfiguration
	{
		protected override Rectangle GymNamePosition => new Rectangle(210, 115, 870, 90);
		protected override Rectangle EggTimerPosition => new Rectangle(420, 360, 250, 70);
		protected override Rectangle EggLevelPosition => new Rectangle(300, 510, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 440, 1080, 140);
		protected override Rectangle RaidTimerPosition => new Rectangle(805, 1075, 170, 60);

		public override List<Point> Level5Points => new List<Point> { new Point(42, 24), new Point(141, 24), new Point(240, 24), new Point(338, 24), new Point(437, 24) };
		public override List<Point> Level4Points => new List<Point> { new Point(91, 24), new Point(190, 24), new Point(289, 24), new Point(387, 24) };

		public BottomMenu1080X1920Configuration() : base(1080, 1920) { }
	}
}