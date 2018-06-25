using SixLabors.Primitives;

namespace Raidfelden.Services.Ocr.RaidConfigurations
{
	public class BottomMenu900X1600Configuration : RaidImageConfiguration
	{
		protected override Rectangle GymNamePosition => new Rectangle(220, 110, 860, 90);
		protected override Rectangle EggTimerPosition => new Rectangle(400, 500, 270, 70);
		protected override Rectangle EggLevelPosition => new Rectangle(285, 660, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 445, 1080, 150);
		protected override Rectangle RaidTimerPosition => new Rectangle(805, 1078, 160, 50);

		public BottomMenu900X1600Configuration() : base(900, 1600) { }
	}
}