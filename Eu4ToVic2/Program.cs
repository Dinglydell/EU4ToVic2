using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	class Program
	{
		static void Main(string[] args)
		{

			//WARNING: Highly experimental:
			ExperimentalTGAWriter();

			var save = new Eu4Save("uncompressed 2.eu4", @"C:\Users\Blake\Documents\Paradox Interactive\Crusader Kings II\eu4_export\mod\Converted_England1444_11_11");

			//Console.WriteLine(save.RootList);
			var V2World = new Vic2World(save);


			Console.Read();
		}

		private static void ExperimentalTGAWriter()
		{
			//unsafe
			//{
			//	// important to use the BitmapData object's Width and Height
			//	// properties instead of the Bitmap's.
			//	
			//	var Width = 92;
			//	var Height = 64;
			//	for (int x = 0; x < Width; x++)
			//	{
			//		int columnOffset = x * 4;
			//		for (int y = 0; y < Height; y++)
			//		{
			//			//byte* row = (byte*)data.Scan0 + (y * data.Stride);
			//			byte B = row[columnOffset];
			//			byte G = row[columnOffset + 1];
			//			byte R = row[columnOffset + 2];
			//			byte alpha = row[columnOffset + 3];
			//		}
			//	}
			//}
			var width = 92;
			var height = 64;
			byte r = 250;
			byte g = 100;
			byte b = 100;
			using (var file = File.Create("test.tga"))
			{
				byte[] DeCompressed = new byte[]{ 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
				file.Write(DeCompressed, 0, DeCompressed.Length);
				file.WriteByte((byte)(width & 0xFF));
				file.WriteByte((byte)((width & 0xFF) / 0xFF));
				file.WriteByte((byte)(height & 0xFF));
				file.WriteByte((byte)((height & 0xFF) / 0xFF));
				file.WriteByte(24);
				file.WriteByte(0x0);
				for (var y = 0; y < height; y++)
				{
					for(var x = 0; x < width; x++)
					{
						file.WriteByte(b);
						file.WriteByte(g);
						file.WriteByte(r);
						
					
					}
					//file.Write()
				}
				
			}
		}
	}
}
