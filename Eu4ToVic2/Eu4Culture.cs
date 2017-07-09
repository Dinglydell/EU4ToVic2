using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	public class Eu4Culture
	{
		public string Name { get; set; }
		public Eu4CultureGroup Group { get; set; }
		public string PrimaryNation { get; set; }
		public List<string> MaleNames { get; set; }
		public List<string> FemaleNames { get; set; }
		public List<string> DynastyNames { get; set; }
		

		public Eu4Culture(PdxSublist data, Eu4CultureGroup group)
		{
			Name = data.Key;
			Group = group;
			if (data.KeyValuePairs.ContainsKey("primary"))
			{
				PrimaryNation = data.KeyValuePairs["primary"];
			}
			
			MaleNames = data.Sublists.ContainsKey("male_names") ? data.Sublists["male_names"].Values : new List<string>();
			FemaleNames = data.Sublists.ContainsKey("female_names") ? data.Sublists["female_names"].Values : new List<string>();
		
			DynastyNames = data.Sublists.ContainsKey("dynasty_names") ? data.Sublists["dynasty_names"].Values : new List<string>();
			
		}

	}

	public class Eu4CultureGroup
	{
		public string Name { get; set; }
		public List<Eu4Culture> Cultures { get; set; }

		public Eu4CultureGroup(string name)
		{
			Name = name;
			Cultures = new List<Eu4Culture>();
			//foreach(var sub in data.Sublists)
			//{
			//	if (sub.Value.KeyValuePairs.ContainsKey("primary"))
			//	{
			//		Cultures.Add(new Eu4Culture(sub.Value, this));
			//	}
			//}
		}

		public Eu4Culture AddCulture(PdxSublist data)
		{
			var culture = new Eu4Culture(data, this);
			Cultures.Add(culture);
			return culture;
		}
	}
}
