using System;
using System.Collections.Generic;

namespace Eu4ToVic2
{
	public class Eu4Religion
	{
		public Eu4ReligionGroup Group { get; set; }
		public Colour Colour { get; set; }
		public int Icon { get; set; }
		public Eu4Religion(PdxSublist data, Eu4ReligionGroup group)
		{
			Group = group;
			Colour = new Colour(data.Sublists["color"].Values);
			Icon = int.Parse(data.KeyValuePairs["icon"]);
		}
	}

	public class Eu4ReligionGroup
	{
		public string Name { get; private set; }
		public List<Eu4Religion> Religions { get; set; }

		public Eu4ReligionGroup(string name)
		{
			Name = name;
			Religions = new List<Eu4Religion>();
		}

		public Eu4Religion AddReligion(PdxSublist value)
		{
			var religion = new Eu4Religion(value, this);
			Religions.Add(religion);
			return religion;
		}
	}
}