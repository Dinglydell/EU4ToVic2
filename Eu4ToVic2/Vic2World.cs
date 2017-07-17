using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace Eu4ToVic2
{
	public class Vic2World
	{

		public Eu4Save Eu4Save { get; set; }

		public static readonly string VIC2_DIR = @"C:\Program Files (x86)\Steam\steamapps\common\Victoria 2\";

		public static readonly string OUTPUT = @"C:\Program Files (x86)\Steam\steamapps\common\Victoria 2\mod\converter_test\";

		public ProvinceMapper ProvMapper { get; set; }
		public Mapper V2Mapper { get; set; }
		public List<Vic2Country> Vic2Countries { get; set; }
		/// <summary>
		/// All countries that exist in vic2, not just those that must have files generated
		/// </summary>
		public HashSet<string> ExistingCountries { get; set; }
		public List<Vic2Province> Vic2Provinces { get; set; }

		public Dictionary<string, string> Localisation { get; set; }
		public PdxSublist LocalisationHelper { get; set; }
		public PdxSublist CultureNations { get; set; }
		public PdxSublist CountryEffects { get; set; }
		public PdxSublist ProvinceEffects { get; set; }
		public Dictionary<Ideology, IdeologyModifier> IdeologyModifiers { get; set; }



		/// <summary>
		/// Stores order technology should be granted in each category
		/// </summary>
		public Dictionary<string, List<string>> TechOrder { get; set; }
		public Dictionary<string, Vic2ReligionGroup> ReligiousGroups { get; set; }
		internal Dictionary<string, Vic2CultureGroup> CultureGroups { get; private set; }
		public Dictionary<string, Vic2Culture> Cultures { get; private set; }
		public int NumCultureNations { get; internal set; }
		public bool PrimaryNations { get; internal set; }
		public PdxSublist PopData { get; private set; }
		public PdxSublist Factories { get; private set; }

		public List<Vic2DiploRelation> Alliances { get; set; }
		public List<Vic2DiploRelation> Puppets { get; set; }

		public Dictionary<string, string> VanillaLocalisation { get; set; }

		public Vic2World(Eu4Save eu4Save)
		{

			Eu4Save = eu4Save;
			ReligiousGroups = new Dictionary<string, Vic2ReligionGroup>();
			V2Mapper = new Mapper(this);
			ProvMapper = new ProvinceMapper("province_mappings.txt");
			LoadVanillaLocalisation();
			LoadLocalisationHelper();
			LoadEffects();
			LoadFactories();
			LoadPoliticalParties();
			LoadVicTech();
			LoadVicReligion();
			LoadVicCulture();
			LoadVicPopData();
			LoadExistingCountries();
			Console.WriteLine("Constructing Vic2 world...");
			GenerateCountries();
			GeneratePrimaryNations();
			GenerateProvinces();
			GenerateRelations();
			Console.WriteLine("Generating mod...");
			CreateModFolders();
			CreateCountryFiles();
			CreateProvinceFiles();
			CreatePopFiles();
			CreateReligionFile();
			CreateCultureFile();
			CreateDiplomacyFiles();
			CreateDecisionFiles();
			CreateLocalisationFiles();
			Console.WriteLine("Done!");
		}


		private void LoadVanillaLocalisation()
		{
			Console.WriteLine("Loading EU4 localisation...");
			VanillaLocalisation = new Dictionary<string, string>();
			var files = Directory.GetFiles(Path.Combine(VIC2_DIR, "localisation"));
			foreach (var file in files)
			{

				LoadLocalisationFile(file);

			}
		}

		private void LoadLocalisationFile(string path)
		{
			using (var file = new StreamReader(path))
			{


				var key = new StringBuilder();
				var value = new StringBuilder();
				var readKey = true;
				var readValue = false;
				//var inQuotes = false;
				while (!file.EndOfStream)
				{
					var ch = Convert.ToChar(file.Read());

					if (ch == ';')
					{
						if (readValue)
						{
							readValue = false;
						}
						if (readKey)
						{
							readKey = false;
							readValue = true;
						}

						continue;
					}
					if (Environment.NewLine.Contains(ch))
					{
						readValue = false;
						readKey = true;
						if (key.Length > 0 && value.Length > 0 && !VanillaLocalisation.ContainsKey(key.ToString()))
						{
							VanillaLocalisation.Add(key.ToString(), value.ToString());
						}
						key = new StringBuilder();
						value = new StringBuilder();
						continue;
					}

					if (readKey)
					{
						key.Append(ch);
					}
					if (readValue)
					{
						value.Append(ch);
					}
				}
				if (key.Length > 0 && value.Length > 0)
				{
					VanillaLocalisation.Add(key.ToString(), value.ToString());
				}
			}
		}

		private void CreateDiplomacyFiles()
		{
			Console.WriteLine("Creating diplomacy files...");

			var alliances = new PdxSublist();
			foreach (var alliance in Alliances)
			{
				alliances.AddSublist("alliance", alliance.GetData());
			}
			var puppets = new PdxSublist();
			foreach (var vassal in Puppets)
			{
				puppets.AddSublist("vassal", vassal.GetData());
			}

			var histDir = Directory.CreateDirectory(Path.Combine(OUTPUT, @"history\diplomacy"));

			using (var file = File.CreateText(Path.Combine(histDir.FullName, "Alliances.txt")))
			{
				alliances.WriteToFile(file);
			}
			using (var file = File.CreateText(Path.Combine(histDir.FullName, "PuppetStates.txt")))
			{
				puppets.WriteToFile(file);
			}
		}

		private void GenerateRelations()
		{
			Alliances = new List<Vic2DiploRelation>();
			Puppets = new List<Vic2DiploRelation>();
			foreach (var eu4Relation in Eu4Save.Relations)
			{
				var v = new Vic2DiploRelation(eu4Relation, this);
				if (v.Type == V2Relation.alliance)
				{
					Alliances.Add(v);
				}
				else if (v.Type == V2Relation.vassal)
				{
					Puppets.Add(v);
				}
			}
		}

		private void LoadFactories()
		{
			Factories = PdxSublist.ReadFile("factory.txt");
		}

		private void LoadVicPopData()
		{
			PopData = new PdxSublist();
			var files = Directory.GetFiles(Path.Combine(VIC2_DIR, @"history\pops\1836.1.1"));
			foreach (var file in files)
			{
				var data = PdxSublist.ReadFile(file);
				foreach (var sub in data.Sublists)
				{
					PopData.Sublists[sub.Key] = sub.Value;
				}
			}
		}

		private void LoadLocalisationHelper()
		{
			LocalisationHelper = PdxSublist.ReadFile("localisation.txt");
		}

		private void CreateLocalisationFiles()
		{
			Localisation = new Dictionary<string, string>();
			//countries
			foreach (var country in Vic2Countries)
			{
				country.AddLocalisation(Localisation, LocalisationHelper, this);
			}
			//cultures
			foreach (var cultureGroup in CultureGroups)
			{
				cultureGroup.Value.AddLocalisation(Localisation);
			}
			//religion
			foreach (var religionGroup in ReligiousGroups)
			{
				religionGroup.Value.AddLocalisation(Localisation);
			}

			Directory.CreateDirectory(Path.Combine(OUTPUT, "localisation"));
			using (var file = File.CreateText(Path.Combine(OUTPUT, @"localisation\converted.csv")))
			{
				foreach (var locale in Localisation)
				{
					file.Write(locale.Key);
					for (var i = 0; i < 14; i++)
					{
						file.Write(';');
						file.Write(locale.Value);
					}
					file.WriteLine();
				}
			}
		}

		private void GeneratePrimaryNations()
		{
			foreach (var cultureGroup in CultureGroups)
			{
				cultureGroup.Value.SetupUnionNation(this);
			}
			foreach (var culture in Cultures)
			{
				culture.Value.SetupPrimaryNation(this);
			}
			PrimaryNations = true;

		}

		private void LoadExistingCountries()
		{
			ExistingCountries = new HashSet<string>();
			var countries = PdxSublist.ReadFile(Path.Combine(VIC2_DIR, @"common\countries.txt"));
			foreach (var c in countries.KeyValuePairs.Keys)
			{
				ExistingCountries.Add(c);
			}
		}

		private void LoadVicCulture()
		{
			Console.WriteLine("Loading Vic2 Cultures...");
			CultureGroups = new Dictionary<string, Vic2CultureGroup>();
			Cultures = new Dictionary<string, Vic2Culture>();
			var cultures = PdxSublist.ReadFile(Path.Combine(VIC2_DIR, @"common\cultures.txt"));
			foreach (var culGroup in cultures.Sublists)
			{
				var nextGroup = new Vic2CultureGroup(this, culGroup.Value);
				CultureGroups[culGroup.Key] = nextGroup;
				nextGroup.Cultures.ForEach(c => Cultures.Add(c.Name, c));
			}

			Console.WriteLine("Loading cultureNations.txt");
			CultureNations = PdxSublist.ReadFile("cultureNations.txt");
		}

		private void CreateReligionFile()
		{
			Console.WriteLine("Creating religion file...");
			var data = new PdxSublist();
			foreach (var rel in ReligiousGroups)
			{
				data.AddSublist(rel.Key, rel.Value.GetData());

			}
			using (var file = File.CreateText(Path.Combine(OUTPUT, @"common\religion.txt")))
			{
				data.WriteToFile(file);
			}
		}
		private void CreateCultureFile()
		{
			Console.WriteLine("Creating culture file...");
			var data = new PdxSublist();
			foreach (var cul in CultureGroups)
			{
				data.AddSublist(cul.Key, cul.Value.GetData());

			}
			using (var file = File.CreateText(Path.Combine(OUTPUT, @"common\cultures.txt")))
			{
				data.WriteToFile(file);
			}
		}

		private void LoadVicReligion()
		{
			ReligiousGroups = new Dictionary<string, Vic2ReligionGroup>();
			var religions = PdxSublist.ReadFile(Path.Combine(VIC2_DIR, @"common\religion.txt"));
			foreach (var relGroup in religions.Sublists)
			{
				ReligiousGroups[relGroup.Key] = new Vic2ReligionGroup(relGroup.Value);
			}
		}

		private void CreatePopFiles()
		{
			Console.WriteLine("Creating pop files...");
			var histDir = Directory.CreateDirectory(Path.Combine(OUTPUT, @"history\pops"));
			var startDir = Directory.CreateDirectory(Path.Combine(OUTPUT, @"history\pops\1836.1.1"));
			var vanillaFiles = Directory.GetFiles(Path.Combine(VIC2_DIR, @"history\pops\1836.1.1"));
			foreach (var file in vanillaFiles)
			{
				using (File.CreateText(Path.Combine(startDir.FullName, Path.GetFileName(file)))) { }
			}
			var pops = new PdxSublist(null);
			foreach (var province in Vic2Provinces)
			{
				var pop = province.GetPopData();
				if (pop != null)
				{
					pops.AddSublist(province.ProvID.ToString(), pop);
				}
			}
			using (var file = File.CreateText(Path.Combine(startDir.FullName, "1836.txt")))
			{
				pops.WriteToFile(file);
			}
		}



		private void CreateProvinceFiles()
		{
			Console.WriteLine("Creating province files...");
			var histDir = Directory.CreateDirectory(Path.Combine(OUTPUT, @"history\provinces"));
			foreach (var province in Vic2Provinces)
			{
				if (!Directory.Exists(Path.Combine(histDir.FullName, province.Subfolder)))
				{
					Directory.CreateDirectory(Path.Combine(histDir.FullName, province.Subfolder));
				}
				//history\provinces
				using (var file = File.CreateText(Path.Combine(histDir.FullName, province.Subfolder, province.FileName)))
				{
					province.GetProvinceData().WriteToFile(file);
				}
			}
		}

		private void CreateModFolders()
		{

			if (Directory.Exists(OUTPUT))
			{
				DeleteDir(OUTPUT);

			}
			Directory.CreateDirectory(OUTPUT);
			Directory.CreateDirectory(Path.Combine(OUTPUT, "common"));
			Directory.CreateDirectory(Path.Combine(OUTPUT, "history"));
			Directory.CreateDirectory(Path.Combine(OUTPUT, "gfx"));
		}

		private void DeleteDir(string path)
		{
			foreach (var dir in Directory.GetDirectories(path))
			{
				DeleteDir(dir);
			}
			foreach (var file in Directory.GetFiles(path))
			{
				File.Delete(file);
			}
			Directory.Delete(path, true);
		}

		public string GenerateReligion(string religion)
		{
			var group = Eu4Save.ReligiousGroups.FirstOrDefault(g => g.Value.Religions.Contains(Eu4Save.Religions[religion])).Value;
			if (!ReligiousGroups.ContainsKey(group.Name))
			{
				ReligiousGroups[group.Name] = new Vic2ReligionGroup(group.Name);
			}
			ReligiousGroups[group.Name].AddReligion(religion, Eu4Save);
			return religion;
		}

		public string GenerateCulture(string eu4Culture, string vic2Name = null)
		{
			if (vic2Name == null)
			{
				vic2Name = eu4Culture;
			}
			// todo: look into what happens when eu4 culture groups are named differently to vic2 culture groups
			var group = Eu4Save.CultureGroups.FirstOrDefault(g => g.Value.Cultures.Contains(Eu4Save.Cultures[eu4Culture])).Value;
			if (!CultureGroups.ContainsKey(group.Name))
			{
				CultureGroups[group.Name] = new Vic2CultureGroup(group);
			}
			Cultures[vic2Name] = CultureGroups[group.Name].AddCulture(eu4Culture, this, vic2Name);
			return eu4Culture;
		}
		private void CreateDecisionFiles()
		{
			Console.WriteLine("Creating decision files...");
			Directory.CreateDirectory(Path.Combine(OUTPUT, "decisions"));

			var template = File.ReadAllText("form_nation_template.txt");



			//var cNations = Vic2Countries.Where(c => c.IsCultureNation).ToList();
			//84
			//for (var i = 82; i < 84; i++ )
			//{
			foreach (var country in Vic2Countries)
			{
				if (country.IsCultureNation)
				{
					//var country = cNations[i];
					if (Vic2Provinces.Find(p => p.Cores?.Contains(country) ?? false) != null)
					{
						using (var decisionFile = File.CreateText(Path.Combine(OUTPUT, $@"decisions\form_{country.PrimaryCulture}_nation.txt")))
						{
							decisionFile.WriteLine("political_decisions = {");
							decisionFile.Write(country.GetFormDecision(template));
							decisionFile.WriteLine("}");
						}
					}
				}

			}

		}

		private void CreateCountryFiles()
		{
			//common
			Console.WriteLine("Creating country files...");
			var vanillaCtry = PdxSublist.ReadFile(Path.Combine(VIC2_DIR, @"common\countries.txt"));
			var txt = Path.Combine(OUTPUT, @"common\countries.txt");
			using (var txtFile = File.CreateText(txt))
			{
				foreach (var country in Vic2Countries)
				{
					txtFile.WriteLine($"{country.CountryTag} = \"countries/{country.CountryTag}.txt\"");
				}
				txtFile.WriteLine("# From vanilla");
				foreach (var ctry in vanillaCtry.KeyValuePairs)
				{
					var genCtry = Vic2Countries.Find(c => c.CountryTag == ctry.Key);
					if (genCtry == null)
					{
						txtFile.WriteLine($"{ctry.Key} = \"{ctry.Value}\"");
					}
				}

			}


			var dir = Directory.CreateDirectory(Path.Combine(OUTPUT, @"common\countries"));
			var histDir = Directory.CreateDirectory(Path.Combine(OUTPUT, @"history\countries"));
			var flagDir = Directory.CreateDirectory(Path.Combine(OUTPUT, @"gfx\flags"));
			var eu4Flags = Eu4Save.GetFilesFor(@"gfx\flags");
			var vic2Flags = Directory.GetFiles(Path.Combine(VIC2_DIR, @"gfx\flags"));
			var suffixes = new string[] { "", "communist", "fascist", "monarchy", "republic" };
			foreach (var country in Vic2Countries)
			{
				//common\countries
				using (var file = File.CreateText(Path.Combine(dir.FullName, $"{country.CountryTag}.txt")))
				{
					country.GetCommonCountryFile().WriteToFile(file);
				}
				//history\countries
				using (var file = File.CreateText(Path.Combine(histDir.FullName, $"{country.CountryTag}.txt")))
				{
					country.GetHistoryCountryFile().WriteToFile(file);
				}
				if (country.CountryTag == "GRA")
				{
					Console.WriteLine();
				}
				if (vic2Flags.FirstOrDefault(f => Path.GetFileName(f).StartsWith(country.CountryTag)) == null)
				{

					var eu4Flag = eu4Flags.Find(f => Path.GetFileName(f).StartsWith(country.Eu4Country?.CountryTag ?? "!"));
					foreach (var suff in suffixes)
					{
						var name = country.CountryTag + (suff == string.Empty ? string.Empty : $"_{suff}");
						if (eu4Flag == null)
						{
							country.CreateFlag(Path.Combine(flagDir.FullName, $"{name}.tga"), suff);
						}
						else
						{
							File.Copy(eu4Flag, Path.Combine(flagDir.FullName, $"{name}.tga"));
						}

					}
				}

			}
		}

		private void GenerateProvinces()
		{
			Console.WriteLine("Mapping provinces...");
			Vic2Provinces = new List<Vic2Province>();
			var provs = new Dictionary<int, List<Eu4Province>>();
			ProvMapper.Mappings.Sublists["mappings"].Sublists.Every("link").ForEach((lnk) =>
			{
				var eu4Provs = new List<Eu4Province>();
				if (lnk.FloatValues.ContainsKey("eu4"))
				{
					lnk.FloatValues["eu4"].ForEach((eu4Prov) =>
					{
						var eu4 = (int)eu4Prov;
						if (Eu4Save.Provinces.ContainsKey(eu4))
						{
							eu4Provs.Add(Eu4Save.Provinces[eu4]);
						}
					});
				}
				if (lnk.FloatValues.ContainsKey("v2"))
				{
					lnk.FloatValues["v2"].ForEach((v2Prov) =>
					{
						var v2 = (int)v2Prov;
						if (provs.ContainsKey(v2))
						{
							provs[v2].AddRange(eu4Provs);
						}
						else {
							provs[v2] = new List<Eu4Province>(eu4Provs);
						}

					});
				}
			});



			foreach (var prov in provs)
			{
				var v2Prov = FindProvinceFile(prov.Key);
				if (v2Prov != null)
				{
					Vic2Provinces.Add(new Vic2Province(prov.Key, v2Prov, this, provs.Count(p => prov.Value.All(p.Value.Contains)), prov.Value));
				}
			}

			Console.WriteLine($"Mapped {Vic2Provinces.Count} provinces.");
		}
		public int GetBestVic2ProvinceMatch(int eu4ProvID)
		{
			return (int)ProvMapper.Mappings.Sublists["mappings"].Sublists.Every("link").First(s =>
			{
				return s.FloatValues.ContainsKey("eu4") ? s.FloatValues["eu4"].Any(prov => prov == eu4ProvID) : false;
			}).FloatValues["v2"].First();
		}

		private PdxSublist FindProvinceFile(int key)
		{
			var provinces = Path.Combine(VIC2_DIR, @"history\provinces");
			var provDir = Directory.GetDirectories(provinces);
			foreach (var dir in provDir)
			{
				var files = Directory.GetFiles(dir);
				var provFile = files.FirstOrDefault(f => Path.GetFileName(f).StartsWith(key + " "));
				if (provFile != default(string))
				{
					return PdxSublist.ReadFile(provFile);
				}
			}
			return null;
		}

		public Vic2Country GetCountry(Eu4Country eu4Country)
		{
			if (eu4Country == null)
			{
				return null;
			}
			return Vic2Countries.Find(c => c.Eu4Country == eu4Country);
		}
		public Vic2Country GetCountry(string eu4CountryTag)
		{
			var vic2Tag = V2Mapper.GetV2Country(eu4CountryTag);
			return Vic2Countries.Find(c => c.CountryTag == vic2Tag);
		}
		public void ValueEffect(PdxSublist effects, Action<Dictionary<string, float>> callback, string key, float value)
		{
			if (effects.Sublists.ContainsKey("values") && effects.GetSublist("values").Sublists.ContainsKey(key))
			{
				effects.GetSublist("values").Sublists.ForEach(key, (sub) =>
				{
					var average = 0f;
					if (sub.FloatValues.ContainsKey("average"))
					{
						average = sub.GetFloat("average");
					}
					var min = float.MinValue;
					if (sub.FloatValues.ContainsKey("minimum"))
					{
						min = sub.GetFloat("minimum");
					}
					var max = float.MaxValue;
					if (sub.FloatValues.ContainsKey("maximum"))
					{
						max = sub.GetFloat("maximum");
					}

					callback(sub.FloatValues.ToDictionary(effect => effect.Key, effect => Math.Min(max, Math.Max(min, (value - average))) * effect.Value.Sum()));
				});

			}
		}
		// do not look in here, it's an ugly mess
		private void LoadVicTech()
		{
			Console.WriteLine("Loading vic2 technologies...");
			var techs = PdxSublist.ReadFile(Path.Combine(VIC2_DIR, @"common\technology.txt"));
			var techTypes = techs.Sublists["folders"];
			TechOrder = new Dictionary<string, List<string>>();
			foreach (var techType in techTypes.Sublists)
			{
				TechOrder.Add(techType.Key, new List<string>());
				var techTypeFile = PdxSublist.ReadFile(Path.Combine(VIC2_DIR, $@"technologies\{techType.Key}.txt"));
				//list instead of dictionary to retain order
				var subTypes = new List<KeyValuePair<string, Queue<string>>>();
				foreach (var tech in techTypeFile.Sublists)
				{
					if (!subTypes.Exists(p => p.Key == tech.Value.KeyValuePairs["area"]))
					{
						subTypes.Add(new KeyValuePair<string, Queue<string>>(tech.Value.KeyValuePairs["area"], new Queue<string>()));
					}
					// a big mess
					subTypes.Find(kv => kv.Key == tech.Value.KeyValuePairs["area"]).Value.Enqueue(tech.Key);
				}
				var subTypesList = subTypes.ConvertAll(st => st.Value);
				while (subTypesList.Count > 0)
				{
					for (var i = 0; i < subTypesList.Count; i++)
					{
						TechOrder[techType.Key].Add(subTypesList[i].Dequeue());
						if (subTypesList[i].Count == 0)
						{
							subTypesList.RemoveAt(i--);
						}

					}

				}
			}


		}

		private void LoadPoliticalParties()
		{
			Console.WriteLine("Loading party ideologies...");
			IdeologyModifiers = new Dictionary<Ideology, IdeologyModifier>();
			var parties = PdxSublist.ReadFile("political_parties.txt");
			foreach (var ideology in (Ideology[])Enum.GetValues(typeof(Ideology)))
			{
				var name = Enum.GetName(typeof(Ideology), ideology);
				if (parties.Sublists.ContainsKey(name))
				{
					var party = new IdeologyModifier();

					foreach (var policy in Policies.policyTypes)
					{
						if (parties.Sublists[name].KeyValuePairs.ContainsKey(policy.Name))
						{
							party.AddModifier(policy, float.Parse(parties.Sublists[name].KeyValuePairs[policy.Name]));
						}
					}
					IdeologyModifiers.Add(ideology, party);
				}
			}
		}

		private void LoadEffects()
		{
			Console.WriteLine("Loading countryEffects.txt...");
			CountryEffects = PdxSublist.ReadFile("countryEffects.txt");
			Console.WriteLine("Loading provinceEffects.txt...");
			ProvinceEffects = PdxSublist.ReadFile("provinceEffects.txt");
		}

		private void GenerateCountries()
		{
			Console.WriteLine("Creating Vic2 countries...");
			Vic2Countries = new List<Vic2Country>();
			foreach (var eu4Country in Eu4Save.Countries.Values)
			{
				var v2Country = new Vic2Country(this, eu4Country);
				Vic2Countries.Add(v2Country);
			}

			Console.WriteLine($"Created {Vic2Countries.Count} Vic2 countries...");
		}

	}

	public static class Policies
	{
		public static Type[] policyTypes = { typeof(economic_policy), typeof(trade_policy), typeof(religious_policy), typeof(war_policy), typeof(citizenship_policy) };
	}
	public enum economic_policy
	{
		planned_economy,
		state_capitalism,
		interventionism,
		laissez_faire
	}

	public enum trade_policy
	{
		free_trade,
		protectionism
	}
	public enum religious_policy
	{
		pro_atheism,
		secularized,
		pluralism,
		moralism
	}

	public enum war_policy
	{
		pacifism, anti_military, pro_military, jingoism
	}
	public enum citizenship_policy
	{
		full_citizenship, limited_citizenship, residency
	}

	public class IdeologyModifier
	{
		public Dictionary<Type, float> Policies { get; set; }
		public IdeologyModifier()
		{
			Policies = new Dictionary<Type, float>();
		}
		internal void AddModifier(Type policy, float value)
		{
			if (!Policies.ContainsKey(policy))
			{
				Policies.Add(policy, 0);
			}
			Policies[policy] += value;

		}

		public static IdeologyModifier operator +(IdeologyModifier a, IdeologyModifier b)
		{
			var newMod = new IdeologyModifier();
			foreach (var pol in a.Policies)
			{
				newMod.AddModifier(pol.Key, pol.Value);
			}
			foreach (var pol in b.Policies)
			{
				newMod.AddModifier(pol.Key, pol.Value);
			}
			return newMod;
		}
		//public int EconomicPolicy { get { return Policies[typeof(economic_policy); } }
		//public int TradePolicy { get; }
		//public int ReligiousPolicy { get;  }
		//public int WarPolicy { get;  }
		//public int CitizenshipPolicy { get;  }

	}
}