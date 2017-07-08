using System;
using System.Collections.Generic;

namespace Eu4ToVic2
{
	public class Vic2Religion
	{
		public string Name { get; set; }
		public Colour Colour { get; set; }
		public int Icon { get; set; }
		public Vic2Religion(string name, Eu4Save eu4)
		{
			Name = name;
			var eu4Religion = eu4.Religions[name];
			Colour = eu4Religion.Colour;
		}

		public Vic2Religion(PdxSublist data)
		{
			Name = data.Key;
			Icon = int.Parse(data.KeyValuePairs["icon"]);
			Colour = new Colour(data.Sublists["color"].Values, 255);
		}

		public PdxSublist GetData()
		{
			var data = new PdxSublist();
			data.AddString("icon", Icon.ToString());

			var colourData = new PdxSublist();
			colourData.Values.Add((Colour.Red / 255f).ToString());
			colourData.Values.Add((Colour.Green / 255f).ToString());
			colourData.Values.Add((Colour.Blue / 255f).ToString());
			data.AddSublist("color", colourData);
			return data;
		}
	}

	public class Vic2ReligionGroup
	{
		public string Name { get; set; }
		public List<Vic2Religion> Religions { get; set; }

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
	}
}