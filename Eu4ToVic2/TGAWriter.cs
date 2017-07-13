using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	class TGAWriter
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public Colour[][] Pixels { get; set; }

		public void WriteToFile(string path)
		{
			using (var file = File.Create(path))
			{
				byte[] DeCompressed = new byte[] { 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
				file.Write(DeCompressed, 0, DeCompressed.Length);
				file.WriteByte((byte)(Width & 0xFF));
				file.WriteByte((byte)((Width & 0xFF) / 0xFF));
				file.WriteByte((byte)(Height & 0xFF));
				file.WriteByte((byte)((Height & 0xFF) / 0xFF));
				file.WriteByte(24);
				file.WriteByte(0x0);
				var pixls = Pixels.SelectMany(row => row.SelectMany(p => new byte[] { p.Blue, p.Green, p.Red })).ToArray();
				file.Write(pixls, 0, pixls.Length);
				//for (var y = 0; y < Height; y++)
				//{
				//	for (var x = 0; x < Width; x++)
				//	{
				//		
				//		//var pos = y * Width + x;
				//		file.WriteByte(Pixels[y][x].Blue);
				//		file.WriteByte(Pixels[y][x].Green);
				//		file.WriteByte(Pixels[y][x].Red);
				//		
				//
				//
				//	}
				//	//file.Write()
				//}

			}
		}


		public static void WriteUniformTGA(string path, Colour colour)
		{
			var width = 92;
			var height = 64;
			//byte r = 250;
			//byte g = 100;
			//byte b = 100;
			using (var file = File.Create(path))
			{
				byte[] DeCompressed = new byte[] { 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
				file.Write(DeCompressed, 0, DeCompressed.Length);
				file.WriteByte((byte)(width & 0xFF));
				file.WriteByte((byte)((width & 0xFF) / 0xFF));
				file.WriteByte((byte)(height & 0xFF));
				file.WriteByte((byte)((height & 0xFF) / 0xFF));
				file.WriteByte(24);
				file.WriteByte(0x0);
				for (var y = 0; y < height; y++)
				{
					for (var x = 0; x < width; x++)
					{
						file.WriteByte(colour.Blue);
						file.WriteByte(colour.Green);
						file.WriteByte(colour.Red);
						


					}
					//file.Write()
				}

			}
		}

		public static void WriteTricolourTGA(string path, Colour colourLeft, Colour colourMiddle, Colour colourRight)
		{
			var width = 92;
			var height = 64;
			//byte r = 250;
			//byte g = 100;
			//byte b = 100;
			using (var file = File.Create(path))
			{
				byte[] DeCompressed = new byte[] { 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
				file.Write(DeCompressed, 0, DeCompressed.Length);
				file.WriteByte((byte)(width & 0xFF));
				file.WriteByte((byte)((width & 0xFF) / 0xFF));
				file.WriteByte((byte)(height & 0xFF));
				file.WriteByte((byte)((height & 0xFF) / 0xFF));
				file.WriteByte(24);
				file.WriteByte(0x0);
				for (var y = 0; y < height; y++)
				{
					for (var x = 0; x < width; x++)
					{
						if(x < width / 3f)
						{
							WritePixel(file, colourLeft);	
						} else if(x < 2 * width / 3f)
						{
							WritePixel(file, colourMiddle);
						} else
						{
							WritePixel(file, colourRight);
						}
						



					}
					//file.Write()
				}

			}
		}

		private static void WritePixel(FileStream file, Colour colour)
		{
			file.WriteByte(colour.Blue);
			file.WriteByte(colour.Green);
			file.WriteByte(colour.Red);
		}
	}
}
