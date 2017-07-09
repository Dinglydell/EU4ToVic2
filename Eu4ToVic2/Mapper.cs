using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{

	public class Monarchy
	{
		public int MinAbsolutism { get; set; }
		public int MaxAbsolutism { get; set; }
		public string ID { get; set; }
		public Monarchy(PdxSublist monarchy)
		{
			ID = monarchy.Key;
			if (monarchy.KeyValuePairs.ContainsKey("min_absolutism"))
			{
				MinAbsolutism = int.Parse(monarchy.KeyValuePairs["min_absolutism"]);
			} else
			{
				MinAbsolutism = 0;
			}

			if (monarchy.KeyValuePairs.ContainsKey("max_absolutism"))
			{
				MaxAbsolutism = int.Parse(monarchy.KeyValuePairs["max_absolutism"]);
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
		private Dictionary<string, string> Culture { get; set; }
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
			Religion = Mappings("religionMap.txt");
			var gov = MappingsFile("governmentMapping.txt");
			Government = Mappings(gov);
			Monarchies = new List<Monarchy>();
			var monarchies = gov.Sublists["monarchies"];
			foreach (var monarchy in monarchies.Sublists)
			{
				Monarchies.Add(new Monarchy(monarchy.Value));
			}
		}

		private Dictionary<string, string> Mappings(string filePath)
		{
			return Mappings(MappingsFile(filePath));
		}

		private Dictionary<string, string> Mappings(PdxSublist mappings)
		{
			var map = new Dictionary<string, string>();
			mappings.GetAllMatchingSublists("link", (lnk) =>
			{

				if (lnk.KeyValuePairs.Keys.All(key => key.Contains("eu4") || key.Contains("v2")))
				{
					lnk.GetAllMatchingKVPs("eu4", (eu4) =>
					{
						map.Add(eu4, lnk.KeyValuePairs["v2"]);
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
			var culture = Map(Culture, eu4Culture);
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

		public string GetV2Government(Eu4Country eu4Country)
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
			if (!map.ContainsKey(eu4Version))
			{
				return null;
			}
			return map[eu4Version];
		}
	}
}
