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
		public string DisplayName { get; set; }
		public Eu4CultureGroup Group { get; set; }
		public string PrimaryNation { get; set; }
		public List<string> MaleNames { get; set; }
		public List<string> FemaleNames { get; set; }
		public List<string> DynastyNames { get; set; }
		

		public Eu4Culture(PdxSublist data, Eu4CultureGroup group, Eu4Save save)
		{
			Name = data.Key;
			DisplayName = save.Localisation.ContainsKey(Name) ? save.Localisation[Name] : Name;
			Group = group;
			if (data.KeyValuePairs.ContainsKey("primary"))
			{
				PrimaryNation = data.GetString("primary");
			}

			MaleNames = new List<string>();
			data.Sublists.ForEach("male_names", (sub) =>
			{
				MaleNames.AddRange(sub.Values);
			});// ? data.GetSublist("male_names").Values : new List<string>();
			FemaleNames = new List<string>();
			data.Sublists.ForEach("female_names", (sub) =>
			{
				FemaleNames.AddRange(sub.Values);
			});

			DynastyNames = data.Sublists.ContainsKey("dynasty_names") ? data.GetSublist("dynasty_names").Values : new List<string>();
			
		}

	}

	public class Eu4CultureGroup
	{
		public string Name { get; set; }
		public List<Eu4Culture> Cultures { get; set; }
		public string DisplayName { get; internal set; }

		public Eu4CultureGroup(string name, Eu4Save save)
		{
			Name = name;
			DisplayName = save.Localisation[name];
			Cultures = new List<Eu4Culture>();
			//foreach(var sub in data.Sublists)
			//{
			//	if (sub.Value.KeyValuePairs.ContainsKey("primary"))
			//	{
			//		Cultures.Add(new Eu4Culture(sub.Value, this));
			//	}
			//}
		}

		public Eu4Culture AddCulture(PdxSublist data, Eu4Save save)
		{
			var culture = new Eu4Culture(data, this, save);
			Cultures.Add(culture);
			return culture;
		}
	}
}
