using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	class Eu4Save
	{
		public static readonly string GAME_PATH = @"C:\Program Files (x86)\Steam\steamapps\common\Europa Universalis IV\";

		public string ModPath { get; set; }
		public string PlayerTag { get; set; }
		public PdxSublist RootList { get; set; }

		public Dictionary<string, Eu4Country> Countries { get; set; }
		public Dictionary<int, Eu4Province> Provinces { get; private set; }

		public Eu4Save(string filePath, string modFilePath)
		{
			ModPath = modFilePath;
			ReadSave(filePath);
			PlayerTag = RootList.KeyValuePairs["player"];

			//LoadCountryTags();
			LoadCountryData();
			Console.WriteLine($"Average merc: {Countries.Where(c => c.Value.Exists).Sum(c => c.Value.Mercantilism) / Countries.Count}");
			LoadProvinceData();
			Console.WriteLine("EU4 data loaded.");
		}



		private void LoadCountryData()
		{
			Console.WriteLine("Loading countries...");
			Countries = new Dictionary<string, Eu4Country>();
			var countries = RootList.Sublists["countries"];
			foreach (var countryList in countries.Sublists)
			{
				//if (!countryList.Value.KeyValuePairs.ContainsKey("primary_culture") || !countryList.Value.KeyValuePairs.ContainsKey("estimated_monthly_income") || float.Parse(countryList.Value.KeyValuePairs["estimated_monthly_income"]) <= 0.001)
				if (!countryList.Value.Sublists.ContainsKey("core_provinces"))
				{
					//country does not exist
					continue;
				}
				var country = new Eu4Country(countryList.Value);
				Countries.Add(country.CountryTag, country);
			}
			Console.WriteLine($"Loaded {Countries.Count} countries.");
			Console.WriteLine($"{Countries.Count(c => c.Value.Exists)} countries exist.");
			Console.WriteLine($"{Countries.Count(c => c.Value.Institutions[6]) } countries have embraced enlightenment.");
		}

		private void LoadProvinceData()
		{
			Console.WriteLine("Loading provinces...");
			Provinces = new Dictionary<int, Eu4Province>();
			var provinces = RootList.Sublists["provinces"];
			foreach (var provList in provinces.Sublists)
			{
				if (!provList.Value.KeyValuePairs.ContainsKey("culture"))
				{
					continue;
				}
				var province = new Eu4Province(provList.Value, this);
				Provinces.Add(province.ProvinceID, province);
			}

			Console.WriteLine($"Loaded {Provinces.Count} provinces.");
		}

		//private void LoadCountryTags()
		//{
		//	Console.WriteLine("Loading list of countries...");
		//	var countryTagFiles = GetFilesFor(@"common\country_tags");
		//	CountryTags = new List<string>();
		//	foreach (var ctf in countryTagFiles)
		//	{
		//		var file = new StreamReader(ctf);
		//		string line;
		//		while ((line = file.ReadLine()) != null)
		//		{
		//			if (line.First() !='#' && line.Contains('='))
		//			{
		//				var tag = line.Substring(0, line.IndexOf('=')).Trim();
		//				CountryTags.Add(tag);
		//			}
		//		}
		//	}
		//
		//	var dynCountries = RootList.Sublists["dynamic_countries"].Values.Select(s => s.Substring(1, 3));
		//	CountryTags.AddRange(dynCountries);
		//	Console.WriteLine(CountryTags[0]);
		//}

		private List<string> GetFilesFor(string path)
		{
			var modPath = Path.Combine(ModPath, path);
			var gameFiles = Directory.GetFiles(GAME_PATH + path);
			var modFileNames = Directory.GetFiles(modPath).Select(Path.GetFileName);

			var files = new List<string>();
			foreach (var name in gameFiles)
			{
				if (modFileNames.Contains(Path.GetFileName(name)))
				{
					files.Add(Path.Combine(modPath, Path.GetFileName(name)));
				} else
				{
					files.Add(name);
				}
			}
			return files;
		}

		private void ReadSave(string filePath)
		{
			Console.WriteLine("Reading save file...");
			RootList = PdxSublist.ReadFile(filePath, "EU4txt");
			Console.WriteLine("Save reading complete.");
			//	
			//	var file = new StreamReader(filePath);
			//	var line = file.ReadLine();
			//	if (line != "EU4txt")
			//	{
			//		throw new Exception("Not an EU4 file");
			//	}
			//	RootList = new PdxSublist(null);
			//	var currentList = RootList;
			//	//var lineNumber = 0;
			//	while ((line = file.ReadLine()) != null)
			//	{
			//		//lineNumber++;
			//		currentList = PdxSublist.RunLine(line, currentList);
			//	}
			//	if(currentList != RootList)
			//	{
			//		throw new Exception("An unknown error occurred.");
			//	}
			//	
		}

		//private PdxSublist RunLine(string line, PdxSublist currentList)
		//{
		//	string key = null;
		//	var value = RemoveWhitespace(line.Substring(line.IndexOf('=') + 1));
			
		//	if (line.Contains('='))
		//	{
		//		key = RemoveWhitespace(line.Substring(0, line.IndexOf('=')));
		//	}
		//	else if (value == "}")
		//	{
		//		return currentList.Parent;
				
		//	}
		//	var parent = false;
		//	if (value.Contains('}'))
		//	{
		//		value = RemoveWhitespace(value.Substring(0, value.IndexOf('}')));
		//		parent = true;
		//	}

		//	if (value.FirstOrDefault() == '{')
		//	{
		//		var list = new PdxSublist(currentList, key);
		//		currentList.AddSublist(key, list);

		//		if (value.Contains('}'))
		//		{
		//			parent = false;
		//			value = value.Substring(1, value.IndexOf('}'));
		//			SingleLineArray(key, value, list);
		//		}
		//		else {
		//			currentList = list;
		//		}

				
		//	}
		//	else if (key == null && !value.Contains('"'))
		//	{
		//		// awkward single line array of numbers
		//		value = line.Substring(line.IndexOf('=') + 1).Trim();
		//		SingleLineArray(key, value, currentList);
		//	}
		//	else
		//	{
		//		currentList.AddString(key, value);
		//	}
		//	return parent ? currentList.Parent : currentList;
		//}

		
	}
}
