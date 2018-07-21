using Raidfelden.Services.Ocr.RaidConfigurations.Ric1080X2220;
using Raidfelden.Services.Ocr.RaidConfigurations.Ric1440X2960;
using Raidfelden.Services.Ocr.RaidConfigurations.Ric750X1334;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Raidfelden.Services.Ocr.RaidConfigurations
{
    public static class RaidImageConfigurationHelper
    {
	    public static RaidImageConfiguration GetRaidImageConfiguration(this Image<Rgba32> image, bool saveDebugImages)
	    {
			var configuration = new RaidImageConfiguration(1080, 1920);
		    switch (image.Height)
		    {
				case 2960:
					if (image.HasBottomBar())
					{
						configuration = new Ric1440X2960BottomBar();
					}
					else
					{
						configuration = new Ric1440X2960WithoutBar();
					}
					break;
				case 2436:
					configuration = new IPhoneXImageConfiguration();
				    break;
				case 2240:
					if (image.HasBottomBar())
					{
						configuration = new BottomMenu1080X2240Configuration();
					}
					break;
				case 2220:
					if (image.HasBottomBar())
					{
						if (image.HasTopBar())
						{
							configuration = new Ric1080X2220TopAndBottomBar();
						}
						else
						{
							//configuration = new WithoutMenu1080X2220Configuration();
							//configuration.BottomMenuHeight = GetBottomMenuHeight(image);
							configuration = new Ric1080X2220BottomBar();
						}
					}
					else
					{
						configuration = new Ric1080X2220WithoutBar();
					}
				    break;
				case 2160:
				    if (image.HasBottomBar())
				    {
					    configuration = new BottomMenu1080X2160Configuration();
					    configuration.BottomMenuHeight = GetBottomBarHeight(image);
				    }
				    break;
				case 1920:
				    if (image.HasBottomBar())
				    {
						configuration.BottomMenuHeight = GetBottomBarHeight(image);
						if (configuration.BottomMenuHeight < 50)
						{
							configuration.BottomMenuHeight = 128;
						}
					}
				    break;
				case 1600:
				    switch (image.Width)
				    {
						case 739:
							configuration = new WithoutMenu739X1600();
							break;
						case 900:
						    if (image.HasBottomBar())
						    {
								configuration = new BottomMenu900X1600Configuration();
							}
						    break;
				    }
					break;
				case 1334:
					if (image.HasTopBar())
					{
						configuration = new Ric750X1334WithTopBar();
					}
					else
					{
						configuration = new Ric750X1334WithoutBar();
					}
				    break;
				case 1280:
				    if (image.HasBottomBar())
				    {
					    configuration = new BottomMenu720X1280Configuration();
				    }
				    break;
		    }

			configuration.SaveDebugImages = saveDebugImages;
			return configuration;

		}

		private static bool HasTopBar(this Image<Rgba32> image)
		{
			// If the whole line has the exact same color it probably is a menu
			var color = image[0, 0];

			for (int x = 1; x < image.Width; x++)
			{
				if (image[x, 0] != color)
				{
					return false;
				}
			}

			// Check if it might be an (IPhone) open hot spot info instead
			if (RaidImageConfiguration.IsColorWithinTolerance(color, new Rgba32(36, 132, 232, 255), 5))
			{
				return false;
			}

			return true;
		}

		private static bool HasBottomBar(this Image<Rgba32> image)
		{
			// If the whole line has the exact same color it probably is a menu
			var color = image[0, image.Height - 1];
			for (int x = 1; x < image.Width; x++)
			{
				if (image[x, image.Height - 1] != color)
				{
					return false;
				}
			}
			return true;
		}

		public static int GetBottomBarHeight(this Image<Rgba32> image)
		{
			int counter = 0;
			var color = image[0, image.Height - 1];
			for (int i = image.Height - 2; i >= 0; i--)
			{
				if (image[0, i] == color)
				{
					counter++;
				}
				else
				{
					return counter;
				}
			}
			return counter;
		}

	    public static int GetTopBarHeight(this Image<Rgba32> image)
	    {
		    int counter = 0;
		    var color = image[0, 0];
		    for (int i = 1; i < image.Height - 1; i++)
		    {
			    if (image[0, i] == color)
			    {
				    counter++;
			    }
			    else
			    {
				    return counter;
			    }
		    }
		    return counter;
	    }
	}
}
