using Eu4Helper;
using PdxFile;
using PdxUtil;
using System;
using System.Collections.Generic;

namespace Eu4ToVic2
{
	public class Vic2Religion
	{
		public string Name { get; set; }
		public Colour Colour { get; set; }
		public int Icon { get; set; }
		public string DisplayName { get; private set; }

		public Vic2Religion(string name, Eu4Save eu4)
		{
			Name = name;
			var eu4Religion = eu4.Religions[name];
			DisplayName = eu4Religion.DisplayName;
			Colour = eu4Religion.Colour;
		}

		public Vic2Religion(PdxSublist data)
		{
			Name = data.Key;
			Icon = (int)data.GetFloat("icon");
			Colour = new Colour(data.Sublists["color"].FloatValues[string.Empty], 255);
		}

		public PdxSublist GetData()
		{
			var data = new PdxSublist();
			data.AddValue("icon", Icon.ToString());

			var colourData = new PdxSublist();
			colourData.AddValue((Colour.Red / 255f).ToString());
			colourData.AddValue((Colour.Green / 255f).ToString());
			colourData.AddValue((Colour.Blue / 255f).ToString());
			data.AddSublist("color", colourData);
			return data;
		}

		public void AddLocalisation(Dictionary<string, string> localisation)
		{
			if(DisplayName != null)
			{
				localisation.Add(Name, DisplayName);
			}

		}
	}

	public class Vic2ReligionGroup
	{
		public string Name { get; set; }
		public List<Vic2Religion> Religions { get; set; }
		public string DisplayName { get; private set; }

		public Vic2ReligionGroup(string name)
		{
			Name = name;
			Religions = new List<Vic2Religion>();
		}
		public Vic2ReligionGroup(PdxSublist data): this(data.Key)
		{
			foreach (var sub in data.Sublists)
			{
				Religions.Add(new Vic2Religion(sub.Value));
			}
		}

		public Vic2ReligionGroup(Eu4ReligionGroup group): this(group.Name)
		{
			DisplayName = group.DisplayName;
		}

		public PdxSublist GetData()
		{
			var data = new PdxSublist(null, Name);

			foreach (var religion in Religions)
			{
				data.AddSublist(religion.Name, religion.GetData());
			}

			return data;
		}

		public Vic2Religion AddReligion(string name, Eu4Save save)
		{
			var religion = new Vic2Religion(name, save);
			Religions.Add(religion);
			return religion;
		}

		public void AddLocalisation(Dictionary<string, string> localisation)
		{
			if(DisplayName != null)
			{
				localisation.Add(Name, DisplayName);
			}
			foreach (var religion in Religions)
			{
				religion.AddLocalisation(localisation);
			}
		}
	}
}