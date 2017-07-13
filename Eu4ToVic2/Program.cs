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

			
			
			//TGAWriter.WriteTricolourTGA("test.tga", new Colour(0, 85, 164), new Colour(255, 255, 255), new Colour(239, 65, 53));

			var save = new Eu4Save("uncompressed 2.eu4", @"C:\Users\Blake\Documents\Paradox Interactive\Crusader Kings II\eu4_export\mod\Converted_England1444_11_11");

			//Console.WriteLine(save.RootList);
			var V2World = new Vic2World(save);


			Console.Read();
		}

		
	}
}
