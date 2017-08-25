using PdxFile;
using System.Collections.Generic;
using System.Linq;

namespace Eu4ToVic2
{
	public class Eu4ProvCollection
	{
		public string Name { get; set; }

		public HashSet<int> Provinces { get; set; }
		

		public Eu4ProvCollection(string name, PdxSublist data)
		{
			Name = name;
			Provinces = new HashSet<int>(data.FloatValues.Values.SelectMany(f => f.Select(e => (int)e)));
		}
	}

	public class Eu4Continent: Eu4ProvCollection
	{


		public Eu4Continent(string name, PdxSublist value): base(name, value)
		{
		}
	}
}