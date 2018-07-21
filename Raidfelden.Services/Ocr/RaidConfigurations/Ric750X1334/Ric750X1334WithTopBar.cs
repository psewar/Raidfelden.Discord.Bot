using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace Raidfelden.Services.Ocr.RaidConfigurations.Ric750X1334
{
    public class Ric750X1334WithTopBar : RaidImageConfiguration
    {
	    protected override Rectangle GymNamePosition => new Rectangle(150, 60 + TopBarHeight, 600, 100);
	    protected override Rectangle PokemonNamePosition => new Rectangle(0, 330+ TopBarHeight, 750, 100);
	    protected override Rectangle PokemonCpPosition => new Rectangle(170, 230+ TopBarHeight, 420, 100);
	    protected override Rectangle RaidTimerPosition => new Rectangle(575, 800+ TopBarHeight, 110, 30);
	    protected override Rectangle EggTimerPosition => new Rectangle(285, 270+ TopBarHeight, 175, 50);
	    protected override Rectangle EggLevelPosition => new Rectangle(200, 380+ TopBarHeight, 350, 55);

		// Points to detect the raid level on non hatched raid images these contain the lower level ones also
		public override List<Point> Level5Points => new List<Point> { new Point(28, 15), new Point(101, 15), new Point(175, 15), new Point(248, 15), new Point(322, 15) };
	    public override List<Point> Level4Points => new List<Point> { new Point(64, 15), new Point(138, 15), new Point(211, 15), new Point(285, 15) };

		public Ric750X1334WithTopBar() : base(750, 1334)
	    {
		}

	    public int TopBarHeight { get; set; }

	    public override void PreProcessImage<TPixel>(Image<TPixel> image)
	    {
		    TopBarHeight = (int)((image as Image<Rgba32>).GetTopBarHeight()/2.5);
	    }
	}
}