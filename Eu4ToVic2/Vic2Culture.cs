using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	public class Vic2Culture
	{
		public string Name { get; set; }

		public Vic2Country PrimaryNation { get; set; }

		public Colour Colour { get; set; }
		public Vic2World World { get; set; }
		public List<string> FirstNames { get; set; }
		public List<string> LastNames { get; set; }
		public Vic2Culture(string eu4Name, Vic2World vic2World, string vic2Name)
		{
			Name = vic2Name;
			World = vic2World;
			var eu4Culture = vic2World.Eu4Save.Cultures[eu4Name];
			if (eu4Culture.PrimaryNation != null)
			{
				PrimaryNation = vic2World.GetCountry(eu4Culture.PrimaryNation) ?? new Vic2Country(vic2World.V2Mapper.GetV2Country(eu4Culture.PrimaryNation));
			}
			FirstNames = eu4Culture.MaleNames.ToList();

			if(PrimaryNation?.FemaleLeaders ?? false)
			{
				FirstNames.AddRange(eu4Culture.FemaleNames);
			}
			LastNames = eu4Culture.DynastyNames;
			Colour = PrimaryNation?.MapColour;
			if(Colour == null)
			{
				byte r = 240;
				byte g = 0;
				byte b = 0;
				byte threshhold = 32;
				while(!isUnique(r,g, b, threshhold))
				{
					b += threshhold;
					if(b == 0)
					{
						g += threshhold;
						if(g == 0)
						{
							r += threshhold;
						}
					}
				}
				Colour = new Colour(r, g, b);
			}
		}

		private bool isUnique(int r, int g, int b, byte threshhold)
		{
			byte divisor = threshhold;
			foreach(var culture in World.Cultures)
			{
				if(culture.Value.Colour.Red / divisor == r / divisor && culture.Value.Colour.Green / divisor == g / divisor && culture.Value.Colour.Blue / divisor == b / divisor)
				{
					return false;
				}
			}
			return true;
		}

		public Vic2Culture(string name, Vic2World vic2World): this(name, vic2World, name)
		{

		}

		public Vic2Culture(PdxSublist data)
		{
			Name = data.Key;
			FirstNames = data.Sublists["first_names"].Values;
			LastNames = data.Sublists["last_names"].Values;
			Colour = new Colour(data.Sublists["color"].Values);
			if (data.KeyValuePairs.ContainsKey("primary")) {
				PrimaryNation = new Vic2Country(data.KeyValuePairs["primary"]);
			}
		}

		public PdxSublist GetData()
		{
			var data = new PdxSublist();


			var colourData = new PdxSublist();
			colourData.Values.Add((Colour.Red / 255f).ToString());
			colourData.Values.Add((Colour.Green / 255f).ToString());
			colourData.Values.Add((Colour.Blue / 255f).ToString());
			data.AddSublist("color", colourData);

				data.AddSublist("first_names", GetNameData(FirstNames));

				data.AddSublist("last_names", GetNameData(LastNames));


			if (PrimaryNation != null)
			{
				data.AddString("primary", PrimaryNation.CountryTag);
			}
			return data;
		}

		private PdxSublist GetNameData(List<string> names)
		{
			var nameData = new PdxSublist();
			foreach (var name in names)
			{
				nameData.AddString(null, name);
			}
			return nameData;
		}
	}

	class Vic2CultureGroup
	{
		public string Name { get; set; }
		public Vic2Country Union { get; set; }
		public string Leader { get; set; }
		public string Unit { get; set; }

		public List<Vic2Culture> Cultures { get; set; }
		public Vic2CultureGroup(string name)
		{
			Name = name;
			Cultures = new List<Vic2Culture>();
			// todo: better default values (dynamic?)
			Leader = "european";
			Unit = "EuropeanGC";
		}
		public Vic2CultureGroup(PdxSublist data): this(data.Key)
		{
			Leader = data.KeyValuePairs["leader"];
			if (data.KeyValuePairs.ContainsKey("unit"))
			{
				Unit = data.KeyValuePairs["unit"];
			}

			foreach (var sub in data.Sublists)
			{
				Cultures.Add(new Vic2Culture(sub.Value));
			}

			if (data.KeyValuePairs.ContainsKey("union")) {
				Union = new Vic2Country(data.KeyValuePairs["union"]);
			}
		}

		public Vic2Culture AddCulture(string eu4Culture, Vic2World vic2World, string vic2Name = null)
		{
			var culture = new Vic2Culture(eu4Culture, vic2World, vic2Name);
			Cultures.Add(culture);
			return culture;
		}

		public PdxSublist GetData()
		{

			var data = new PdxSublist(null, Name);

			data.AddString("leader", Leader);
			if (Unit != null)
			{
				data.AddString("unit", Unit);
			}

			foreach (var cul in Cultures)
			{
				data.AddSublist(cul.Name, cul.GetData());
			}

			if(Union != null)
			{
				data.AddString("union", Union.CountryTag);
			}

			return data;
		}
	}
}
