using SixLabors.Primitives;

namespace Raidfelden.Services.Ocr.RaidConfigurations
{
	public class BottomMenu720X1280Configuration : RaidImageConfiguration
	{
		protected override Rectangle GymNamePosition => new Rectangle(220, 110, 860, 90);
		protected override Rectangle EggTimerPosition => new Rectangle(400, 365, 270, 70);
		protected override Rectangle EggLevelPosition => new Rectangle(285, 515, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 445, 1080, 150);
		protected override Rectangle RaidTimerPosition => new Rectangle(805, 1078, 160, 50);

		public BottomMenu720X1280Configuration() : base(720, 1280) { }
	}
}