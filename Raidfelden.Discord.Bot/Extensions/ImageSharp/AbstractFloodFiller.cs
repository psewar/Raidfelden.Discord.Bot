using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace Raidfelden.Discord.Bot.Extensions.ImageSharp
{
    public abstract class AbstractFloodFiller
    {
		protected Image<Rgba32> bitmap;
		protected byte[] tolerance = new byte[] { 25, 25, 25 };
		protected Rgba32 fillColor = Rgba32.Magenta;
		protected bool fillDiagonally = false;
		//protected bool slow = false;

		//cached bitmap properties
		protected int bitmapWidth = 0;
		protected int bitmapHeight = 0;
		//protected int bitmapStride = 0; // No idea for this is used
		//protected int bitmapPixelFormatSize = 0; // Should not be needed
		//protected byte[] bitmapBits = null; // Should not be needed

		//internal int timeBenchmark = 0;
		internal Stopwatch watch = new Stopwatch();

		//internal, initialized per fill
		//protected BitArray pixelsChecked;
		protected bool[,] pixelsChecked;
		protected byte[] byteFillColor;
		protected Rgba32 startColor;
		//protected int stride;

		public AbstractFloodFiller()
		{

		}

		public AbstractFloodFiller(AbstractFloodFiller configSource)
		{
			if (configSource != null)
			{
				this.Bitmap = configSource.Bitmap;
				this.FillColor = configSource.FillColor;
				this.FillDiagonally = configSource.FillDiagonally;
				this.Tolerance = configSource.Tolerance;
			}
		}

		public Rgba32 FillColor
		{
			get { return fillColor; }
			set { fillColor = value; }
		}

		public bool FillDiagonally
		{
			get { return fillDiagonally; }
			set { fillDiagonally = value; }
		}

		public byte[] Tolerance
		{
			get { return tolerance; }
			set { tolerance = value; }
		}

		public Image<Rgba32> Bitmap
		{
			get { return bitmap; }
			set
			{
				bitmap = value;
			}
		}

		public abstract void FloodFill(Point pt);

		protected void PrepareForFloodFill(Point pt)
		{
			//cache data in member variables to decrease overhead of property calls
			//this is especially important with Width and Height, as they call
			//GdipGetImageWidth() and GdipGetImageHeight() respectively in gdiplus.dll - 
			//which means major overhead.
			byteFillColor = new byte[] { fillColor.B, fillColor.G, fillColor.R };
			//bitmapStride = bitmap.Stride;
			//bitmapPixelFormatSize = bitmap.PixelFormatSize;
			//bitmapBits = bitmap.Bits;
			bitmapWidth = bitmap.Width;
			bitmapHeight = bitmap.Height;

			//pixelsChecked = new bool[bitmapBits.Length / bitmapPixelFormatSize];
			pixelsChecked = new bool[bitmapWidth, bitmapHeight];
		}
	}
}
