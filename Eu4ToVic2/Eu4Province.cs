using Eu4Helper;
using PdxFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	public class Eu4Province: Eu4ProvinceBase
	{
		

		public Eu4Province(PdxSublist province, Eu4Save save)
		{
			ProvinceID = -int.Parse(province.Key);
			if (province.KeyValuePairs.ContainsKey("owner"))
			{
				Owner = save.Countries[province.GetString("owner")];
			}
			var institutions = province.GetSublist("institutions");
			Institutions = institutions.Values.Select(ins => float.Parse(ins)).ToList();
			try
			{
				Area = save.Areas.Values.Single(a => a.Provinces.Contains(ProvinceID));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"WARNING: {ProvinceID} exists in multiple areas!");
				var areas = save.Areas.Values.Where(a => a.Provinces.Contains(ProvinceID));
				foreach (var area in areas)
				{
					Console.WriteLine($"{ProvinceID} exists in {area.Name}");
				}
				Area = areas.Last();
			}
			IsState = Owner?.States.Contains(Area) ?? false;

			Continent = save.Continents.Values.Single(c => c.Provinces.Contains(ProvinceID));

			Cores = new List<Eu4CountryBase>();

			province.KeyValuePairs.ForEach("core", (value) =>
			{
				Cores.Add(save.Countries[value]);
			});

			Culture = province.GetString("culture");
			if (province.KeyValuePairs.ContainsKey("religion"))
			{
				Religion = province.GetString("religion");
			} else
			{
				// this probably happened because you messed up the conversion from ck2
				Religion = "no_religion";
			}

			var history = province.GetSublist("history");
			CulturalHistory = AddHistory(history, "culture");
			if(CulturalHistory.Count == 0)
			{
				OriginalCulture = Culture;
			} else
			{
				OriginalCulture = CulturalHistory.OrderBy(entry => entry.Key.Ticks).First().Value;
			}
			ReligiousHistory = AddHistory(history, "religion");

			//Culture = CulturalHistory.Last().Value;
			//Religion = ReligiousHistory.Last().Value;

			if (province.KeyValuePairs.ContainsKey("fort_level"))
			{
				FortLevel = (int)province.GetFloat("fort_level");
			}

			BaseTax = (int)province.GetFloat("base_tax");
			BaseProduction = (int)province.GetFloat("base_production");
			BaseManpower = (int)province.GetFloat("base_manpower");

			if (province.KeyValuePairs.ContainsKey("estate"))
			{
				Estate = Eu4Helper.Estate.EstateTypes[(int)province.GetFloat("estate")];
			}

			if (province.Sublists.ContainsKey("flags"))
			{
				Flags = province.GetSublist("flags").KeyValuePairs.Keys.ToList();
			}

			Buildings = new List<string>();
			if (province.Sublists.ContainsKey("buildings"))
			{
				foreach (var build in save.Buildings)
				{
					if (province.Sublists["buildings"].BoolValues.ContainsKey(build) && province.Sublists["buildings"].GetBool(build))
					{
						Buildings.Add(build);
					}
				}
			}

			if (ProvinceID == 233)
			{
				//Console.WriteLine("Cornwall!");
			}

		}

		public override PdxSublist GetHistoryFile()
		{
			throw new NotImplementedException();
		}

		private Dictionary<DateTime, string> AddHistory(PdxSublist history, string type)
		{
			var storedHistory = new Dictionary<DateTime, string>();
			if (!history.KeyValuePairs.ContainsKey(type))
			{
				return storedHistory;
			}
			var startDate = new DateTime(1444, 11, 11);
			history.KeyValuePairs.ForEach(type, (h =>
			{
				storedHistory[startDate] = h;
			}));
			foreach (var entry in history.Sublists)
			{
				var sub = entry.Value;
				if (sub.KeyValuePairs.ContainsKey(type))
				{

					var date = PdxSublist.ParseDate(sub.Key);
					sub.KeyValuePairs.ForEach(type, v =>
					{
						storedHistory[date] = v;
					});
				}
			}
			return storedHistory;

		}
	}
}
