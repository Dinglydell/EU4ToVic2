using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	public class Vic2Culture
	{
		private string primaryKey;
		private List<Eu4Culture> eu4Cultures;
		public string Name { get; set; }
		public string DisplayName { get; set; }
		public Vic2Country PrimaryNation { get; set; }

		public Colour Colour { get; set; }
		public Vic2World World { get; set; }
		public List<string> FirstNames { get; set; }
		public List<string> LastNames { get; set; }
		public Vic2CultureGroup Group { get; set; }

		public Vic2Culture(string eu4Name, Vic2World vic2World, string vic2Name, Vic2CultureGroup group)
		{
			Group = group;
			Name = vic2Name;
			World = vic2World;
			eu4Cultures = new List<Eu4Culture>();
			eu4Cultures.Add(vic2World.Eu4Save.Cultures[eu4Name]);
			DisplayName = eu4Cultures[0].DisplayName;
			FirstNames = eu4Cultures[0].MaleNames.ToList();

			
			LastNames = eu4Cultures[0].DynastyNames;
			if (vic2World.PrimaryNations)
			{
				SetupPrimaryNation(vic2World);
			}
		}

		private bool isUnique(int r, int g, int b, byte threshhold)
		{
			byte divisor = threshhold;
			foreach (var culture in World.Cultures)
			{	
				if (culture.Value.Colour != null && culture.Value.Colour.Red / divisor == r / divisor && culture.Value.Colour.Green / divisor == g / divisor && culture.Value.Colour.Blue / divisor == b / divisor)
				{
					return false;
				}
			}
			return true;
		}

		public Vic2Culture(string name, Vic2World vic2World, Vic2CultureGroup group) : this(name, vic2World, name, group)
		{

		}

		public Vic2Culture(Vic2World world, PdxSublist data, Vic2CultureGroup group)
		{
			World = world;
			Group = group;
			Name = data.Key;
			FirstNames = data.Sublists["first_names"].Values;
			LastNames = data.Sublists["last_names"].Values;
			Colour = new Colour(data.Sublists["color"].Values);
			eu4Cultures = world.V2Mapper.Culture.Where(c => c.Value == Name).Select(s => world.Eu4Save.Cultures.ContainsKey(s.Key) ? world.Eu4Save.Cultures[s.Key] : null).Where(s => s != null).ToList();
			if (data.KeyValuePairs.ContainsKey("primary"))
			{
				primaryKey = data.KeyValuePairs["primary"];

			}
			if (world.PrimaryNations)
			{
				SetupPrimaryNation(world);
			}
		}

		public void SetupPrimaryNation(Vic2World world)
		{
			if(world.CultureNations.Sublists["primary"].KeyValuePairs.ContainsKey(Name))
			{
				var tag = world.CultureNations.Sublists["primary"].KeyValuePairs[Name];
				PrimaryNation = world.Vic2Countries.Find(c => c.CountryTag == tag) ?? new Vic2Country(world, tag, this);
			} else if (primaryKey != null)
			{
				PrimaryNation = world.Vic2Countries.Find(c => c.CountryTag == primaryKey) ?? new Vic2Country(world, primaryKey, this);
			}
			else if (eu4Cultures.Count == 1 && eu4Cultures[0].PrimaryNation != null)
			{
				PrimaryNation = world.GetCountry(eu4Cultures[0].PrimaryNation) ?? new Vic2Country(world, world.V2Mapper.GetV2Country(eu4Cultures[0].PrimaryNation), this);
			} else if(Group.Union == null)
			{
				var tag = 'P' + world.NumCultureNations.ToString("D2");
				world.NumCultureNations++;
				PrimaryNation = new Vic2Country(world, tag, this);
			}


			if (PrimaryNation?.FemaleLeaders ?? false)
			{
				foreach (var eu4Culture in eu4Cultures)
				{
					FirstNames.AddRange(eu4Culture.FemaleNames);
				}
				
			}
			if (Colour == null)
			{
				Colour = PrimaryNation?.MapColour;
				if (Colour == null)
				{
					byte r = 240;
					byte g = 0;
					byte b = 0;
					byte threshhold = 32;
					while (!isUnique(r, g, b, threshhold))
					{
						b += threshhold;
						if (b == 0)
						{
							g += threshhold;
							if (g == 0)
							{
								r += threshhold;
							}
						}
					}
					Colour = new Colour(r, g, b);
				}

				if(PrimaryNation != null && PrimaryNation.MapColour == null)
				{
					PrimaryNation.MapColour = Colour;
				}
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

		public void AddLocalisation(Dictionary<string, string> localisation)
		{
			if (DisplayName != null && !localisation.ContainsKey(Name))
			{
				localisation.Add(Name, DisplayName);
			}
		}
	}

	public class Vic2CultureGroup
	{
		private string unionKey;

		public string Name { get; set; }
		public Vic2Country Union { get; set; }
		public string Leader { get; set; }
		public string Unit { get; set; }

		public List<Vic2Culture> Cultures { get; set; }
		public string DisplayName { get; private set; }

		public Vic2CultureGroup(string name)
		{
			Name = name;
			Cultures = new List<Vic2Culture>();
			// todo: better default values (dynamic?)
			Leader = "european";
			Unit = "EuropeanGC";
		}
		public Vic2CultureGroup(Vic2World world, PdxSublist data) : this(data.Key)
		{
			Leader = data.KeyValuePairs["leader"];
			if (data.KeyValuePairs.ContainsKey("unit"))
			{
				Unit = data.KeyValuePairs["unit"];
			}

			foreach (var sub in data.Sublists)
			{
				Cultures.Add(new Vic2Culture(world, sub.Value, this));
			}

			if (data.KeyValuePairs.ContainsKey("union"))
			{
				unionKey = data.KeyValuePairs["union"];
			}
		}

		public Vic2CultureGroup(Eu4CultureGroup group): this(group.Name)
		{
			DisplayName = group.DisplayName;
		}

		public Vic2Culture AddCulture(string eu4Culture, Vic2World vic2World, string vic2Name = null)
		{
			var culture = new Vic2Culture(eu4Culture, vic2World, vic2Name, this);
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

			if (Union != null)
			{
				data.AddString("union", Union.CountryTag);
			}

			return data;
		}

		public void SetupUnionNation(Vic2World world)
		{
			if (world.CultureNations.Sublists["union"].KeyValuePairs.ContainsKey(Name))
			{
				var tag = world.CultureNations.Sublists["union"].KeyValuePairs[Name];
				Union = world.Vic2Countries.Find(c => c.CountryTag == tag) ?? new Vic2Country(world, tag, this);
			}  else if (unionKey != null)
			{
				Union = new Vic2Country(world, unionKey, this);
			}
		}

		public void AddLocalisation(Dictionary<string, string> localisation)
		{
			if (DisplayName != null)
			{
				localisation.Add(Name, DisplayName);
			}
			foreach (var culture in Cultures)
			{
				culture.AddLocalisation(localisation);
			}
		}
	}
}
