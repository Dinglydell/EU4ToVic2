using PdxFile;
using System;
using System.Collections.Generic;

namespace Eu4ToVic2
{
	public class Eu4Religion
	{
		public Eu4ReligionGroup Group { get; set; }
		public Colour Colour { get; set; }
		public int Icon { get; set; }
		public string DisplayName { get; internal set; }

		public Eu4Religion(PdxSublist data, Eu4ReligionGroup group, Eu4Save save)
		{
			Group = group;
			Colour = new Colour(data.GetSublist("color").FloatValues[string.Empty]);
			Icon = (int)data.GetFloat("icon");
			DisplayName = save.Localisation[data.Key];
		}
	}

	public class Eu4ReligionGroup
	{
		public string Name { get; private set; }
		public List<Eu4Religion> Religions { get; set; }
		public string DisplayName { get; internal set; }

		public Eu4ReligionGroup(string name, Eu4Save save)
		{
			Name = name;
			Religions = new List<Eu4Religion>();
			DisplayName = save.Localisation[Name];
		}

		public Eu4Religion AddReligion(PdxSublist value, Eu4Save save)
		{
			var religion = new Eu4Religion(value, this, save);
			Religions.Add(religion);
			return religion;
		}
	}
}