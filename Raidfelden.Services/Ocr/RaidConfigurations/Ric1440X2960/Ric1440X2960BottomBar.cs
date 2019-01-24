using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Raidfelden.Services.Ocr.RaidConfigurations.Ric1440X2960
{
	public class Ric1440X2960BottomBar : RaidImageConfiguration
	{
		public Ric1440X2960BottomBar() : base(1080, 1920) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image)
		{
			image.Mutate(m => m.Crop(new Rectangle(0, 156, 1440, 2562)));
			base.PreProcessImage(image);
		}
	}
}