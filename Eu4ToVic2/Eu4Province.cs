using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	class Eu4Province
	{
		public int ProvinceID { get; set; }

		public Eu4Country Owner { get; set; }

		public List<Eu4Country> Cores { get; set; }

		public List<float> Institutions { get; private set; }

		public string Culture { get; set; }
		public string Religion { get; set; }

		public Dictionary<DateTime, string> CulturalHistory { get; set; }
		public Dictionary<DateTime, string> ReligiousHistory { get; set; }

		public Eu4Province(PdxSublist province, Eu4Save save)
		{
			ProvinceID = -int.Parse(province.Key);
			if (province.KeyValuePairs.ContainsKey("owner")) { 
				Owner = save.Countries[province.GetStringValue("owner")];
			}
			var institutions = province.Sublists["institutions"];
			Institutions = institutions.Values.Select(ins => float.Parse(ins)).ToList();

			Cores = new List<Eu4Country>();

			province.GetAllMatchingKVPs("core", (value) =>
			{
				Cores.Add(save.Countries[value]);
			});

			Culture = province.GetStringValue("culture");
			Religion = province.GetStringValue("religion");

			var history = province.Sublists["history"];
			CulturalHistory = AddHistory(history, "culture");
			ReligiousHistory = AddHistory(history, "religion");

			//Culture = CulturalHistory.Last().Value;
			//Religion = ReligiousHistory.Last().Value;

			if (ProvinceID == 233)
			{
				//Console.WriteLine("Cornwall!");
			}
			
		}

		private Dictionary<DateTime,string> AddHistory(PdxSublist history, string type)
		{
			var storedHistory = new Dictionary<DateTime, string>();
			if (!history.KeyValuePairs.ContainsKey(type))
			{
				return storedHistory;
			}
			storedHistory.Add(new DateTime(1444, 11, 11), history.GetStringValue(type));
			foreach (var entry in history.Sublists)
			{
				if (entry.Value.KeyValuePairs.ContainsKey(type))
				{
					var date = PdxSublist.ParseDate(entry.Value.Key);
					storedHistory[date] = entry.Value.GetStringValue(type);
				}
			}
			return storedHistory;

		}
	}
}
