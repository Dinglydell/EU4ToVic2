using PdxFile;
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
		public bool isNeoCulture;
		public string Name { get; set; }
		public string DisplayName { get; set; }
		public Vic2Country PrimaryNation { get; set; }

		public Colour Colour { get; set; }
		public Vic2World World { get; set; }
		public List<string> FirstNames { get; set; }
		public List<string> LastNames { get; set; }
		public Vic2CultureGroup Group { get; set; }

		public Vic2Culture(string eu4Name, Vic2World vic2World, string vic2Name, Vic2CultureGroup group, string prefix, bool neoCulture = false)
		{
			
			Group = group;
			Name = vic2Name;
	
			World = vic2World;
			eu4Cultures = new List<Eu4Culture>();
			eu4Cultures.Add(vic2World.Eu4Save.Cultures[eu4Name]);
			var display = string.Empty;
			if (isNeoCulture)
			{
				var culture = vic2World.Cultures[vic2World.V2Mapper.GetV2Culture(eu4Cultures[0].Name)];
				display = culture.DisplayName;
			} else
			{
				display = eu4Cultures[0].DisplayName;
			}
			DisplayName = (string.IsNullOrEmpty(prefix) ? string.Empty : (prefix + "-")) + display;
			FirstNames = eu4Cultures[0].MaleNames.ToList();
			isNeoCulture = neoCulture;
			
			LastNames = eu4Cultures[0].DynastyNames;
			if (vic2World.PrimaryNations)
			{
				SetupPrimaryNation(vic2World);
			}
		}
		private bool isUnique(int r, int g, int b, byte threshhold)
		{
			
			foreach (var culture in World.Cultures)
			{
				if (culture.Value.Colour != null)
				{
					var red = (culture.Value.Colour.Red - r);
					var green = (culture.Value.Colour.Green - g);
					var blue = (culture.Value.Colour.Blue - b);
					if (2 * red * red + 4 * green * green + 3 * blue * blue < threshhold * threshhold)
					{
						return false;
					}
				}
			}
			return true;
		}

		public Vic2Culture(string name, Vic2World vic2World, Vic2CultureGroup group) : this(name, vic2World, name, group, "")
		{

		}

		public Vic2Culture(Vic2World world, PdxSublist data, Vic2CultureGroup group)
		{
			
			World = world;
			Group = group;
			Name = data.Key;
			DisplayName = world.VanillaLocalisation[Name];
			FirstNames = data.GetSublist("first_names").Values;
			LastNames = data.GetSublist("last_names").Values;
			Colour = new Colour(data.GetSublist("color").FloatValues[string.Empty]);
			eu4Cultures = world.V2Mapper.Culture.Where(c => c.Value == Name).Select(s => world.Eu4Save.Cultures.ContainsKey(s.Key) ? world.Eu4Save.Cultures[s.Key] : null).Where(s => s != null).ToList();
			if (data.KeyValuePairs.ContainsKey("primary"))
			{
				primaryKey = data.GetString("primary");

			}
			if (world.PrimaryNations)
			{
				SetupPrimaryNation(world);
			}
		}

		public void SetupPrimaryNation(Vic2World world)
		{
			if(Name == "british_north_america")
			{
				Console.WriteLine();
			}
			//if(isNeoCulture)
			//{
			//	return;
			//}
			if (world.CultureNations.GetSublist("except").Values.Contains(Name))
			{
				// this culture is blacklisted
				return;
			}
			if(world.CultureNations.GetSublist("primary").KeyValuePairs.ContainsKey(Name))
			{
				var tag = world.CultureNations.GetSublist("primary").GetString(Name);
				PrimaryNation = world.Vic2Countries.Find(c => c.CountryTag == tag) ?? new Vic2Country(world, tag, this);
			} else if (primaryKey != null)
			{
				PrimaryNation = world.Vic2Countries.Find(c => c.CountryTag == primaryKey) ?? new Vic2Country(world, primaryKey, this);
			}
			else if (eu4Cultures.Count == 1 && (eu4Cultures[0].PrimaryNation != null || isNeoCulture))
			{
				if (isNeoCulture)
				{
					//find colonial nation matching the culture-region
					PrimaryNation = world.Vic2Countries.Where(c => (c.Eu4Country?.Overlord ?? string.Empty) == PrimaryNation?.Eu4Country?.CountryTag && c.Eu4Country.IsColonialNation && world.Eu4Save.Continents.Where(cul => cul.Value == eu4Cultures[0].Continent && cul.Value.Provinces.Contains(c.Capital)).Count() != 0).FirstOrDefault();
				} else if (eu4Cultures[0].PrimaryNation != null)
				{
					PrimaryNation = world.GetCountry(eu4Cultures[0].PrimaryNation) ?? new Vic2Country(world, world.V2Mapper.GetV2Country(eu4Cultures[0].PrimaryNation), this);
				}
				
			} else if(Group.Union == null && !isNeoCulture)
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
					byte r = 0;
					byte g = 0;
					byte b = 240;
					byte threshhold = 8;
					while (!isUnique(r, g, b, (byte)(threshhold * 4)))
					{
						r += (byte)(threshhold * 2);
						if (r < threshhold * 2)
						{
							g += (byte)(threshhold * 4);
							
							
							if (g < threshhold * 4)
							{
								b += (byte)(threshhold * 3);
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
			colourData.AddValue((Colour.Red / 255f).ToString());
			colourData.AddValue((Colour.Green / 255f).ToString());
			colourData.AddValue((Colour.Blue / 255f).ToString());
			data.AddSublist("color", colourData);

			data.AddSublist("first_names", GetNameData(FirstNames));

			data.AddSublist("last_names", GetNameData(LastNames));


			if (PrimaryNation != null)
			{
				data.AddValue("primary", PrimaryNation.CountryTag);
			}
			return data;
		}

		private PdxSublist GetNameData(List<string> names)
		{
			var nameData = new PdxSublist();
			foreach (var name in names)
			{
				nameData.AddValue(string.Empty, name);
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
			Leader = data.GetString("leader");
			if (data.KeyValuePairs.ContainsKey("unit"))
			{
				Unit = data.GetString("unit");
			}

			data.ForEachSublist(sub =>
			{
				Cultures.Add(new Vic2Culture(world, sub.Value, this));
			});

			if (data.KeyValuePairs.ContainsKey("union"))
			{
				unionKey = data.GetString("union");
			}
		}

		public Vic2CultureGroup(Eu4CultureGroup group): this(group.Name)
		{
			DisplayName = group.DisplayName;
		}

		public Vic2Culture AddCulture(string eu4Culture, Vic2World vic2World, string vic2Name = null, string prefix = "", bool neoCulture = false)
		{
			if(vic2Name == "british_north_america")
			{
				Console.WriteLine();
			}
			var culture = new Vic2Culture(eu4Culture, vic2World, vic2Name, this, prefix, neoCulture);
			Cultures.Add(culture);
			return culture;
		}

		public PdxSublist GetData()
		{

			var data = new PdxSublist(null, Name);

			data.AddValue("leader", Leader);
			if (Unit != null)
			{
				data.AddValue("unit", Unit);
			}

			foreach (var cul in Cultures)
			{
				data.AddSublist(cul.Name, cul.GetData());
			}

			if (Union != null)
			{
				data.AddValue("union", Union.CountryTag);
			}

			return data;
		}

		public void SetupUnionNation(Vic2World world)
		{
			if (world.CultureNations.GetSublist("union").KeyValuePairs.ContainsKey(Name))
			{
				var tag = world.CultureNations.GetSublist("union").GetString(Name);
				Union = world.Vic2Countries.Find(c => c.CountryTag == tag) ?? new Vic2Country(world, tag, this);
			}  else if (unionKey != null)
			{
				Union = new Vic2Country(world, unionKey, this);
			}
		}

		public void AddLocalisation(Dictionary<string, string> localisation)
		{
			if (DisplayName != null && !localisation.ContainsKey(Name))
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
