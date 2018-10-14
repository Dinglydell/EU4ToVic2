using Eu4Helper;
using PdxFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{

	public class Monarchy
	{
		public float MinAbsolutism { get; set; }
		public float MaxAbsolutism { get; set; }
		public string ID { get; set; }
		public Monarchy(PdxSublist monarchy)
		{
			ID = monarchy.Key;
			if (monarchy.KeyValuePairs.ContainsKey("min_absolutism"))
			{
				MinAbsolutism = monarchy.GetFloat("min_absolutism");
			} else
			{
				MinAbsolutism = 0;
			}

			if (monarchy.KeyValuePairs.ContainsKey("max_absolutism"))
			{
				MaxAbsolutism = monarchy.GetFloat("max_absolutism");
			}
			else
			{
				MaxAbsolutism = int.MaxValue;
			}

		}
	}
	public class Mapper
	{
		private Dictionary<string, string> Country { get; set; }
		public Dictionary<string, string> Culture { get; set; }
		/// <summary>
		/// Maps regular a generated overseas culture to its regular vic2 version
		/// </summary>
		public Dictionary<string, string> NeoCultures { get; set; }
		private Dictionary<string, string> Religion { get; set; }
		private Dictionary<string, string> Government { get; set; }
		public List<Monarchy> Monarchies { get; set; }

		public List<string> Vic2Cultures { get; set; }
		public Vic2World World { get; private set; }

		public Mapper(Vic2World vic2World)//string mappingPath)
		{
			World = vic2World;
			//instance = this;
			Country = Mappings("country_mappings.txt");
			Culture = Mappings("cultureMap.txt");
			NeoCultures = new Dictionary<string,string>();
			Religion = Mappings("religionMap.txt");
			var gov = MappingsFile("governmentMapping.txt");
			Government = Mappings(gov);
			Monarchies = new List<Monarchy>();
			var monarchies = gov.GetSublist("monarchies");
			monarchies.ForEachSublist(monarchy =>
			{
				Monarchies.Add(new Monarchy(monarchy.Value));
			});
		}

		private Dictionary<string, string> Mappings(string filePath)
		{
			return Mappings(MappingsFile(filePath));
		}

		private Dictionary<string, string> Mappings(PdxSublist mappings)
		{
			var map = new Dictionary<string, string>();
			mappings.Sublists.ForEach("link", (lnk) =>
			{

				if (lnk.KeyValuePairs.Keys.All(key => key.Contains("eu4") || key.Contains("v2")))
				{
					lnk.KeyValuePairs.ForEach("eu4", (eu4) =>
					{
						map.Add(eu4, lnk.GetString("v2"));
					});
				}
			
				
				//Vic2Cultures.Add(lnk.KeyValuePairs["vic2"]);
			});
			return map;
		}

		private PdxSublist MappingsFile(string filePath)
		{
			Console.WriteLine($"Loading {filePath}...");
			return PdxSublist.ReadFile(filePath);
		}

		//public string this[string culture]
		//{
		//	get
		//	{
		//		if (!Mappings.ContainsKey(culture))
		//		{
		//			return culture;
		//		}
		//		return Mappings[culture];
		//	}
		//}
		public string GetV2Culture(string eu4Culture)
		{
			return GetV2Culture(eu4Culture, null, null);
		}
		public string GetV2Culture(string eu4Culture, Eu4ProvinceBase prov, Vic2Province vProv)
		{
			var culture = Map(Culture, eu4Culture);
			if (prov != null) {
				Vic2CultureGroup group = null;
				if (World.Cultures.ContainsKey(culture))
				{
					group = World.Cultures[culture].Group;
				} else if (World.CultureGroups.ContainsKey(culture))
				{
					group = World.CultureGroups[culture];
				} else
				{
					group = new Vic2CultureGroup(World.Eu4Save.Cultures[eu4Culture].Group, World);
				}
				var dist =	group.GetDistance(vProv);
				if(dist > 20){ //diff cul, same group
					
					// different cultures for off colonies
					var neoEu4Culture = $"{eu4Culture}:{prov.Continent.Name}";
					var neoCulture = Map(Culture, neoEu4Culture);
					if (neoCulture == null)
					{
						// todo: fix what this will do to nations on borders between continents such as the ottomans
						neoCulture = World.GenerateCulture(eu4Culture, culture + '_' + prov.Continent.Name + 'n', World.Eu4Save.Localisation[prov.Continent.Name], true);

						NeoCultures[neoCulture] = culture;
						Culture.Add(neoEu4Culture, neoCulture);
					}
					return neoCulture;
				}
			}
			
			if (culture == null || !World.Cultures.ContainsKey(culture))
			{
				culture = World.GenerateCulture(eu4Culture, culture);
				if (!Culture.ContainsKey(eu4Culture)) {
					Culture.Add(eu4Culture, culture);
				}
			}
			return culture;
		}

		public string GetV2Religion(string eu4Religion)
		{
			var religion = Map(Religion, eu4Religion);
			if(religion == null)
			{
				religion = World.GenerateReligion(eu4Religion);
				Religion.Add(eu4Religion, religion);
			}
			return religion;
		}

		public string GetV2Country(string eu4Country)
		{
			return Map(Country, eu4Country) ?? eu4Country;
		}

		public string GetV2Government(Eu4CountryBase eu4Country)
		{
			var govern = Map(Government, eu4Country.Government);
			if(govern == "monarchy")
			{
				foreach (var mon in Monarchies)
				{
					if(eu4Country.Absolutism < mon.MaxAbsolutism && eu4Country.Absolutism >= mon.MinAbsolutism)
					{
						govern = mon.ID;
						break;
					}
				}
			}
			return govern;
		}

		private string Map(Dictionary<string, string> map, string eu4Version)
		{
			if (eu4Version == null || !map.ContainsKey(eu4Version))
			{
				return null;
			}
			return map[eu4Version];
		}

		/// <summary>
		/// check culture
		/// </summary>
		/// <param name="eu4Culture"></param>
		/// <returns></returns>
		internal string GetV2CultureBase(string eu4Culture)
		{
			var culture = Map(Culture, eu4Culture);
			return culture;
		}
	}
}
