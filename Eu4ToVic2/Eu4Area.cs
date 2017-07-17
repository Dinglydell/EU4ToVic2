using System.Collections.Generic;
using System.Linq;

namespace Eu4ToVic2
{
	public class Eu4Area
	{
		public string Name { get; set; }

		public HashSet<int> Provinces { get; set; }
		public float Prosperity { get; internal set; }

		public Eu4Area(string name, PdxSublist value)
		{
			Name = name;
			Provinces = new HashSet<int>(value.FloatValues.Values.SelectMany(f => f.Select(e => (int)e)));
		}
	}
}