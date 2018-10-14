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


			var keepStartDate = !args.Contains("1836");
			//TGAWriter.WriteTricolourTGA("test.tga", new Colour(0, 85, 164), new Colour(255, 255, 255), new Colour(239, 65, 53));

			var save = new Eu4Save("Grindia.eu4", @"C:\Users\Blake\Documents\Paradox Interactive\Europa Universalis IV\mod\converter_test");

			//Console.WriteLine(save.RootList);
			var V2World = new Vic2World(save, keepStartDate);


			Console.Read();
		}

		
	}
}
