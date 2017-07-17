using System.Collections.Generic;
using System.Linq;

namespace Eu4ToVic2
{
	public class Eu4Region
	{
		public string Name { get; set; }

		public HashSet<Eu4Area> Areas { get; set; }

		public Eu4Region(string name, PdxSublist value, Eu4Save save)
		{
			Name = name;
			
			if (value.Sublists.ContainsKey("areas"))
			{
				Areas = new HashSet<Eu4Area>(value.Sublists["areas"].Values.Select(an => save.Areas[an]));
			}
		}
	}
}