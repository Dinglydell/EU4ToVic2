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

		public static void WriteUniformTGA()
		{
			var width = 92;
			var height = 64;
			byte r = 250;
			byte g = 100;
			byte b = 100;
			using (var file = File.Create("test.tga"))
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
