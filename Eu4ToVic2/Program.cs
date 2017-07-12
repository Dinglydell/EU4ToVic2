using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	class Program
	{
		static void Main(string[] args)
		{
			
			var save = new Eu4Save("uncompressed 2.eu4", @"C:\Users\Blake\Documents\Paradox Interactive\Crusader Kings II\eu4_export\mod\Converted_England1444_11_11");

			//Console.WriteLine(save.RootList);
			var V2World = new Vic2World(save);
			

			Console.Read();
		}
	}
}
