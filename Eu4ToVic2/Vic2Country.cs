using System;
using System.Collections.Generic;
using System.Linq;

namespace Eu4ToVic2
{
	public class Vic2Country
	{
		//public static readonly Dictionary<string, string> governments = new Dictionary<string, string>(){
		//	["_presidential_dictatorship"] = "{0}",
		//	["_bourgeois_dictatorship"] = "{1}"
		//};
		/// <summary>
		/// Determines whether country exists in vanilla vic2
		/// </summary>
		public bool IsVic2Country { get; set; }
		public Eu4Country Eu4Country { get; set; }
		public string CountryTag { get; set; }
		public Colour MapColour { get; set; }
		public string GraphicalCulture { get; set; }
		public Dictionary<string, string> Localisation { get; set; }
		public int Capital { get; set; }

		public string DisplayNoun { get; set; }
		public string DisplayAdj { get; set; }

		public string PrimaryCulture { get; set; }

		public List<string> AcceptedCultures { get; set; }

		public string Religion { get; set; }

		public string Government { get; set; }

		public NV NationalValues { get; set; }

		public TechSchoolValues TechSchools { get; set; }

		public float Literacy { get; set; }
		public float Consciousness { get; set; }
		public float Plurality { get; set; }
		public float Militancy { get; set; }

		public bool IsCivilised { get; set; }

		public float Prestige { get; set; }

		public Reforms Reforms { get; set; }

		public UHouse UpperHouse { get; set; }

		public PoliticalParty RulingParty { get; set; }

		public List<PoliticalParty> PoliticalParties { get; set; }

		public List<string> Technologies { get; set; }
		public DateTime LastElection { get; private set; }

		public bool FemaleLeaders { get; set; }
		public IdeologyModifier BasePartyModifier { get; private set; }



		public Vic2Country(Vic2World vic2World, Eu4Country eu4Country)
		{
			CountryTag = vic2World.V2Mapper.GetV2Country(eu4Country.CountryTag);
			IsVic2Country = !vic2World.ExistingCountries.Add(CountryTag);
			Eu4Country = eu4Country;
			MapColour = eu4Country.MapColour;
			GraphicalCulture = "Generic";
			DisplayNoun = eu4Country.DisplayNoun;
			DisplayAdj = eu4Country.DisplayAdj;

			PrimaryCulture = vic2World.V2Mapper.GetV2Culture(eu4Country.PrimaryCulture);
			AcceptedCultures = eu4Country.AcceptedCultures.ConvertAll(c => vic2World.V2Mapper.GetV2Culture(c));

			Religion = vic2World.V2Mapper.GetV2Religion(eu4Country.Religion);

			Government = vic2World.V2Mapper.GetV2Government(eu4Country);

			Capital = vic2World.GetBestVic2ProvinceMatch(eu4Country.Capital);

			//base literacy
			Literacy = 0.1f;

			IsCivilised = eu4Country.Institutions.Values.All(b => b);
			// -100 - 100 scaled to 0 - 100
			Prestige = (eu4Country.Prestige + 100) / 2;

			LastElection = eu4Country.LastElection;

			CalcEffects(vic2World);
			// TODO: make this less of a hack
			if(Government == "absolute_monarchy" && Reforms.vote_franschise != vote_franschise.none_voting)
			{
				Government = "prussian_constitutionalism";
			}
			if(Government == null)
			{
				Console.WriteLine("Uh oh");
			}
			//RulingParty = PoliticalParties.First(p => p.Ideology == UpperHouse.GetMode());
			if (CountryTag == "ENG")
			{
				Console.WriteLine("GBR!");
			}
		}

		public void AddLocalisation(Dictionary<string, string> localisation, PdxSublist localeHelper)
		{
			if (!IsVic2Country)
			{
				
				localisation.Add(CountryTag, DisplayNoun);
				localisation.Add($"{CountryTag}_ADJ", DisplayAdj);
				localeHelper.GetSublist("government").ForEachString(gov =>
				{
					// eg. "People's Republic of %NOUN%" -> "People's Republic of France"
					localisation.Add($"{CountryTag}_{gov.Key}", gov.Value.Replace("%NOUN%", DisplayNoun).Replace("%ADJ%", DisplayAdj).Replace("\"", string.Empty));
				});

				foreach (var party in PoliticalParties)
				{
					localisation.Add(party.Name, localeHelper.GetSublist("party").GetString(Enum.GetName(typeof(Ideology), party.Ideology)).Replace("\"", string.Empty));
				}
			}
		}

		public Vic2Country(Vic2World world, string tag, Vic2CultureGroup cultureGroup): this(world, tag, world.Vic2Countries.Where(c => cultureGroup.Cultures.Find(cul => c.PrimaryCulture == cul.Name) != null), cultureGroup.Cultures.First())
		{

		}

		public Vic2Country(Vic2World world, string tag, Vic2Culture culture) : this(world, tag, world.Vic2Countries.Where(c => c.PrimaryCulture == culture.Name), culture)
		{
		}
		private Vic2Country(Vic2World world, string tag, IEnumerable<Vic2Country> cultureNations, Vic2Culture primaryCulture)
		{
			CountryTag = tag;
			IsVic2Country = !world.ExistingCountries.Add(tag);
			if (!IsVic2Country)
			{
				Console.WriteLine($"Creating nation for {primaryCulture.DisplayName} culture... ({primaryCulture.Name})");
				GraphicalCulture = "Generic";
				world.Vic2Countries.Add(this);
				//setup
				Capital = 1;

				PrimaryCulture = primaryCulture.Name;
				DisplayNoun = world.LocalisationHelper.GetSublist("culture_nation").GetString("noun").Replace("%CULTURE%", primaryCulture.DisplayName).Replace("\"", string.Empty);
                DisplayAdj = world.LocalisationHelper.GetSublist("culture_nation").GetString("adj").Replace("%CULTURE%", primaryCulture.DisplayName).Replace("\"", string.Empty);

				AcceptedCultures = new List<string>();
				PoliticalParties = new List<PoliticalParty>();
				BasePartyModifier = new IdeologyModifier();
				foreach (var nation in cultureNations)
				{
					BasePartyModifier += nation.BasePartyModifier;
				}
				FinalisePoliticalParties(world, BasePartyModifier);

				

				MapColour = primaryCulture.Colour;
				// the most common religion out of nations with that culture
				//Religion = cultureNations.GroupBy(c => c.Religion).Select(c => new {
				//	Name = c.Key, Count = c.Count()
				//}).OrderByDescending(x => x.Count).Select(x => x.Name).First();
				//
				////most common government type
				//Government = cultureNations.GroupBy(c => c.Government).Select(c => new {
				//	Name = c.Key,
				//	Count = c.Count()
				//}).OrderByDescending(x => x.Count).Select(x => x.Name).First();

			}
		}

		public PdxSublist GetHistoryCountryFile()
		{
			var data = new PdxSublist(null);
			data.AddValue("capital", Capital.ToString());
			data.AddValue("primary_culture", PrimaryCulture);
			AcceptedCultures.ForEach(c => data.AddValue("culture", c));
			data.AddValue("religion", Religion);
			data.AddValue("government", Government);
			data.AddValue("plurality", Plurality.ToString());
			if (NationalValues != null)
			{
				data.AddValue("nationalvalue", NationalValues.Value);
			}
			data.AddValue("literacy", Literacy.ToString());
			data.AddValue("civilized", IsCivilised ? "yes" : "no");

			data.AddValue("prestige", Prestige.ToString());
			if (Reforms != null)
			{
				Reforms.AddData(data);
			}
			if (Technologies != null)
			{
				Technologies.ForEach(t => data.AddValue(t, "1"));
			}
			data.AddValue("consciousness", Consciousness.ToString());
			// todo
			data.AddValue("nonstate_consciousness", (Consciousness / 3).ToString());
			if (RulingParty != null)
			{
				data.AddValue("ruling_party", RulingParty.Name);
			}
			data.AddDate("last_election", LastElection);
			if (UpperHouse != null)
			{
				data.AddSublist("upper_house", UpperHouse.GetData(data));
			}
			if (TechSchools != null)
			{
				data.AddValue("schools", Enum.GetName(typeof(TechSchool), TechSchools.TechSchool));
			}

			if (FemaleLeaders)
			{
				var entry = new PdxSublist();
				entry.AddValue("decision", "enact_female_suffrage");

				data.AddSublist("1836.1.1", entry);
			}
			return data;
		}

		public PdxSublist GetCommonCountryFile()
		{
			var data = new PdxSublist(null);

			var colour = new PdxSublist(data, "color");
			colour.AddValue(MapColour.Red.ToString());
			colour.AddValue(MapColour.Green.ToString());
			colour.AddValue(MapColour.Blue.ToString());
			data.AddSublist("color", colour);

			data.AddValue("graphical_culture", GraphicalCulture);

			foreach (var party in PoliticalParties)
			{
				data.AddSublist("party", party.GetData(data));
			}

			return data;
		}

		private void CalcEffects(Vic2World vic2World)
		{
			NationalValues = new NV();

			TechSchools = new TechSchoolValues();

			Reforms = new Reforms();
			var reforms = new Dictionary<Type, float>();

			UpperHouse = new UHouse();

			PoliticalParties = new List<PoliticalParty>();
			BasePartyModifier = new IdeologyModifier();

			Technologies = new List<string>();
			var techInvestment = new Dictionary<string, float>();
			var femaleLeaders = 0f;
			foreach (var techCategory in vic2World.TechOrder.Keys)
			{
				techInvestment.Add(techCategory, 0);
			}
			IterateEffects(vic2World, (Dictionary<string, float> effects) =>
			{
				CalcNationalValues(effects);
				CalcTechSchool(effects);
				CalcReforms(effects, reforms);
				CalcUpperHouse(effects);
				CalcPoliticalParties(effects, BasePartyModifier);
				CalcLiteracy(effects);
				CalcConsciousness(effects);
				CalcMilitancy(effects);
				CalcTechnology(effects, techInvestment);
				femaleLeaders = CalcFemaleLeaders(effects, femaleLeaders);
			});
			FinaliseReforms(reforms);
			FinalisePoliticalParties(vic2World, BasePartyModifier);
			FinaliseTechnology(techInvestment, vic2World.TechOrder);
			FinaliseFemaleLeaders(femaleLeaders);
		}

		private void FinaliseFemaleLeaders(float femaleLeaders)
		{
			FemaleLeaders = femaleLeaders >= 1;
		}

		private float CalcFemaleLeaders(Dictionary<string, float> effects, float femaleLeaders)
		{
			if (effects.ContainsKey("female_leaders"))
			{
				femaleLeaders += effects["female_leaders"];
			}
			return femaleLeaders;
		}

		private void FinaliseTechnology(Dictionary<string, float> techInvestment, Dictionary<string, List<string>> techOrder)
		{
			foreach (var tech in techInvestment)
			{
				Technologies.AddRange(techOrder[tech.Key].GetRange(0, Math.Max(0, (int)tech.Value)));
			}
		}

		private void CalcTechnology(Dictionary<string, float> effects, Dictionary<string, float> techInvestment)
		{
			foreach (var tech in techInvestment.Keys.ToList())
			{
				if (effects.ContainsKey(tech))
				{
					techInvestment[tech] += effects[tech];
				}
			}
		}

		private void CalcTechSchool(Dictionary<string, float> effects)
		{
			foreach (var ts in Enum.GetNames(typeof(TechSchool)))
			{
				if (effects.ContainsKey(ts))
				{
					TechSchools.AddSchool(ts, effects[ts]);
				}
			}
		}

		private void CalcMilitancy(Dictionary<string, float> effects)
		{
			if (effects.ContainsKey("militancy"))
			{
				Militancy += effects["militancy"];
			}

		}

		private void CalcConsciousness(Dictionary<string, float> effects)
		{
			if (effects.ContainsKey("consciousness"))
			{
				Consciousness += effects["consciousness"];
			}
			if (effects.ContainsKey("plurality"))
			{
				Plurality += effects["plurality"];
			}
		}

		private void CalcLiteracy(Dictionary<string, float> effects)
		{
			if (effects.ContainsKey("literacy"))
			{
				Literacy += effects["literacy"] / 100;
			}
		}

		private void FinaliseReforms(Dictionary<Type, float> reforms)
		{
			foreach (var reform in reforms)
			{
				//this is stupid what am I doing

				var prop = typeof(Reforms).GetProperty(reform.Key.Name);
				// divide score by 5 and make sure it's in the range of the enum
				var value = Math.Min(Enum.GetValues(reform.Key).Length - 1, Math.Max(0, (int)(reform.Value / 10)));
				// assign score to enum
				prop.SetValue(Reforms, Enum.Parse(reform.Key, (value).ToString()));
			}
		}

		private void FinalisePoliticalParties(Vic2World vic2World, IdeologyModifier baseModifier)
		{
			foreach (var ideology in vic2World.IdeologyModifiers)
			{
				var polParty = new PoliticalParty(CountryTag + "_" + Enum.GetName(typeof(Ideology), ideology.Key), ideology.Key);
				var totalModifier = baseModifier + ideology.Value;
				foreach (var pol in totalModifier.Policies)
				{
					polParty.SetIssue(pol.Key, pol.Value);
				}
				PoliticalParties.Add(polParty);
				if (ideology.Key == UpperHouse?.GetMode())
				{
					RulingParty = polParty;
				}
			}
		}

		private void CalcPoliticalParties(Dictionary<string, float> effects, IdeologyModifier baseModifier)
		{
			//PoliticalParties = new List<PoliticalParty>();

			//IterateIdeaEffects(vic2World, (Dictionary<string, float> effects, byte ideaLevel) =>
			//{
			foreach (var policy in Policies.policyTypes)
			{
				if (effects.ContainsKey(policy.Name))
				{
					baseModifier.AddModifier(policy, effects[policy.Name]);
				}
			}
			//});


		}

		private void CalcUpperHouse(Dictionary<string, float> effects)
		{
			//throw new NotImplementedException();
			//UpperHouse = new UHouse();
			//IterateIdeaEffects(vic2World, (Dictionary<string, float> effects, byte ideaLevel) =>
			//{
			if (effects.ContainsKey("UH_liberal"))
			{
				UpperHouse.Liberal += (int)effects["UH_liberal"];
			}
			if (effects.ContainsKey("UH_reactionary"))
			{
				UpperHouse.Reactionary += (int)effects["UH_reactionary"];
			}
			if (effects.ContainsKey("UH_conservative"))
			{
				UpperHouse.Conservative += (int)effects["UH_conservative"];
			}

			//);
		}

		private void CalcReforms(Dictionary<string, float> effects, Dictionary<Type, float> reforms)
		{

			//Reforms = new Reforms();
			//
			//
			//IterateIdeaEffects(vic2World, (Dictionary<string, float> effects, byte ideaLevel) =>
			//{

			// go through each reform (property) and add to its score
			foreach (var reform in typeof(Reforms).GetProperties())
			{
				if (effects.ContainsKey(reform.Name))
				{
					if (!reforms.ContainsKey(reform.PropertyType))
					{
						reforms[reform.PropertyType] = 0;
					}
					reforms[reform.PropertyType] += effects[reform.Name];
				}

			}
			//});


		}

		private void CalcNationalValues(Dictionary<string, float> effects)
		{
			//throw new NotImplementedException();
			//NationalValues = new NV();
			//
			//IterateIdeaEffects(vic2World, (Dictionary<string, float> effects, byte ideaLevel) =>
			//{
			if (effects.ContainsKey("NV_order"))
			{
				NationalValues.Order += effects["NV_order"];
			}
			if (effects.ContainsKey("NV_liberty"))
			{
				NationalValues.Liberty += effects["NV_liberty"];
			}
			if (effects.ContainsKey("NV_equality"))
			{
				NationalValues.Equality += effects["NV_equality"];
			}
			//});
		}
		private void IterateEffects(Vic2World vic2World, Action<Dictionary<string, float>> callback)
		{
			IterateCountryEffects(vic2World, Eu4Country, vic2World.CountryEffects, callback);

		}
		public static void IterateCountryEffects(Vic2World vic2World, Eu4Country country, PdxSublist effects, Action<Dictionary<string, float>> callback)
		{
			
			if (effects.Sublists.ContainsKey("ideas"))
			{
				//idea groups
				foreach (var idea in country.Ideas)
				{
					if (effects.GetSublist("ideas").Sublists.ContainsKey(idea.Key))
					{
						callback(effects.GetSublist("ideas").GetSublist(idea.Key).FloatValues.ToDictionary(effect => effect.Key, effect => (idea.Value + 1) * effect.Value.Sum()));
					}
				}
			}

			// country flags
			if (effects.Sublists.ContainsKey("country_flags"))
			{
				foreach (var flag in country.Flags)
				{
					if (effects.GetSublist("country_flags").Sublists.ContainsKey(flag))
					{
						callback(effects.GetSublist("country_flags").GetSublist(flag).FloatValues.ToDictionary(effect => effect.Key, effect => effect.Value.Sum()));
					}
				}
			}
			// religion
			if (effects.Sublists.ContainsKey("religion"))
			{
				if (effects.GetSublist("religion").Sublists.ContainsKey(country.Religion))
				{
					callback(effects.GetSublist("religion").GetSublist(country.Religion).FloatValues.ToDictionary(effect => effect.Key, effect => effect.Value.Sum()));
					//newCallback(effects.GetSublist("religion").GetSublist(country.Religion), 1);
				}
			}

			// government
			if (effects.Sublists.ContainsKey("government"))
			{
				if (effects.GetSublist("government").Sublists.ContainsKey(country.Government))
				{
					callback(effects.GetSublist("government").GetSublist(country.Government).FloatValues.ToDictionary(effect => effect.Key, effect => effect.Value.Sum()));
					//newCallback(effects.GetSublist("government").GetSublist(country.Government), 1);
					//callback(effects.Sublists["government"].Sublists[country.Government].KeyValuePairs.ToDictionary(effect => effect.Key, effect => float.Parse(effect.Value)));
				}
			}

			// policies
			if (effects.Sublists.ContainsKey("policies"))
			{
				foreach (var policy in country.Policies)
				{
					if (effects.GetSublist("policies").Sublists.ContainsKey(policy))
					{
						callback(effects.GetSublist("policies").GetSublist(policy).FloatValues.ToDictionary(effect => effect.Key, effect => effect.Value.Sum()));
						//newCallback(effects.GetSublist("policies").GetSublist(policy), 1);
						//callback(effects.Sublists["policies"].Sublists[policy].KeyValuePairs.ToDictionary(effect => effect.Key, effect => float.Parse(effect.Value)));
					}
				}
			}

			// values

			vic2World.ValueEffect(effects, callback, "mercantilism", country.Mercantilism);
			vic2World.ValueEffect(effects, callback, "legitimacy", country.Legitimacy);// - 50);
			vic2World.ValueEffect(effects, callback, "republican_tradition", country.RepublicanTradition);// - 50);
			vic2World.ValueEffect(effects, callback, "stability", country.Stability);
			vic2World.ValueEffect(effects, callback, "absolutism", country.Absolutism);
			// tech
			vic2World.ValueEffect(effects, callback, "adm_tech", country.AdmTech);
			vic2World.ValueEffect(effects, callback, "dip_tech", country.DipTech);
			vic2World.ValueEffect(effects, callback, "mil_tech", country.MilTech);


			//institutions
			if (effects.Sublists.ContainsKey("institutions"))
			{
				foreach (var institution in country.Institutions)
				{
					var key = institution.Value ? institution.Key : ("not_" + institution.Key);
					if (effects.Sublists["institutions"].Sublists.ContainsKey(key))
					{
						callback(effects.GetSublist("institutions").GetSublist(key).FloatValues.ToDictionary(effect => effect.Key, effect => effect.Value.Sum()));
					}
					
				}
			}
		}

	}

	public enum TechSchool
	{
		traditional_academic, army_tech_school, naval_tech_school, industrial_tech_school, culture_tech_school, commerce_tech_school, prussian_tech_school
	}

	public class TechSchoolValues
	{
		private Dictionary<TechSchool, float> TechSchools { get; set; }
		public TechSchool TechSchool
		{
			get
			{
				return TechSchools.FirstOrDefault(ts => ts.Value == TechSchools.Values.Max()).Key;
			}
		}

		public TechSchoolValues()
		{
			TechSchools = new Dictionary<TechSchool, float>();
		}

		public void AddSchool(string key, float value)
		{
			var school = (TechSchool)Enum.Parse(typeof(TechSchool), key);
			if (!TechSchools.ContainsKey(school))
			{
				TechSchools.Add(school, 0);
			}
			TechSchools[school] += value;
		}



	}

	public enum Ideology
	{
		anarcho_liberal, liberal, conservative, reactionary, socialist, communist, fascist
	}
	public class PoliticalParty
	{

		public Ideology Ideology { get; set; }
		public economic_policy economic_policy { get; set; }
		public trade_policy trade_policy { get; set; }
		public religious_policy religious_policy { get; set; }
		public war_policy war_policy { get; set; }
		public citizenship_policy citizenship_policy { get; set; }
		public string Name { get; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		/// <summary>
		/// defines the upper limit of each position on each policy
		/// </summary>
		static Dictionary<Type, Dictionary<Enum, int>> policyPositions = new Dictionary<Type, Dictionary<Enum, int>>
		{
			[typeof(economic_policy)] = new Dictionary<Enum, int>
			{
				[economic_policy.planned_economy] = -10,
				[economic_policy.state_capitalism] = 0,
				[economic_policy.interventionism] = 10,
				[economic_policy.laissez_faire] = int.MaxValue
			},
			[typeof(war_policy)] = new Dictionary<Enum, int>
			{
				[war_policy.pacifism] = -10,
				[war_policy.anti_military] = 0,
				[war_policy.pro_military] = 10,
				[war_policy.jingoism] = int.MaxValue
			},
			[typeof(religious_policy)] = new Dictionary<Enum, int>
			{
				[religious_policy.pro_atheism] = -10,
				[religious_policy.secularized] = 0,
				[religious_policy.pluralism] = 10,
				[religious_policy.moralism] = int.MaxValue
			},
			[typeof(trade_policy)] = new Dictionary<Enum, int>
			{
				[trade_policy.free_trade] = 0,
				[trade_policy.protectionism] = int.MaxValue
			},
			[typeof(citizenship_policy)] = new Dictionary<Enum, int>
			{
				[citizenship_policy.full_citizenship] = -5,
				[citizenship_policy.limited_citizenship] = 5,
				[citizenship_policy.residency] = int.MaxValue
			},
		};

		public PoliticalParty(string name, Ideology ideology)
		{
			Name = name;
			Ideology = ideology;
			StartDate = new DateTime(1830, 1, 1);
			EndDate = new DateTime(1940, 1, 1);
		}

		public void SetIssue(Type key, float value)
		{
			// property that stores this type of policy 
			var prop = typeof(PoliticalParty).GetProperty(key.Name);

			// must be in ascending order for foreach loop to function
			var positions = policyPositions[key].OrderBy(e => e.Value);

			// find the policy for this type that matches the value
			Enum eVal = null;
			foreach (var pos in positions)
			{
				if (value <= pos.Value)
				{
					eVal = pos.Key;
					break;
				}
			}
			// set that policy
			prop.SetValue(this, eVal);

		}

		public PdxSublist GetData(PdxSublist parent)
		{
			var partyData = new PdxSublist(parent, "party");
			partyData.AddValue("name", Name);
			partyData.AddDate("start_date", StartDate);
			partyData.AddDate("end_date", EndDate);
			partyData.AddValue("ideology", Enum.GetName(typeof(Ideology), Ideology));

			foreach (var pos in policyPositions.Keys)
			{
				var prop = typeof(PoliticalParty).GetProperty(pos.Name);

				partyData.AddValue(pos.Name, Enum.GetName(pos, prop.GetValue(this)));
			}

			return partyData;
		}
	}

	public class UHouse
	{

		private int liberal;

		public int Liberal
		{
			get { return liberal; }
			set
			{
				conservative -= value;
				liberal += value;
			}
		}
		private int conservative;

		public int Conservative
		{
			get { return conservative; }
			set
			{
				liberal -= value / 2;
				reactionary -= value / 2;
				conservative += value;
			}
		}

		private int reactionary;

		public int Reactionary
		{
			get { return reactionary; }
			set
			{
				conservative -= value;
				reactionary += value;
			}
		}

		public UHouse()
		{
			conservative = 100;
			liberal = 0;
			reactionary = 0;
		}

		/// <summary>
		/// Gets the modal ideology in the upper house. (most popular ideology)
		/// </summary>
		/// <returns></returns>
		internal Ideology GetMode()
		{
			if (conservative > 33)
			{
				return Ideology.conservative;
			}
			else if (liberal > 33)
			{
				return Ideology.liberal;
			}
			else
			{
				return Ideology.reactionary;
			}
		}

		internal PdxSublist GetData(PdxSublist parent)
		{
			var data = new PdxSublist(parent, "upper_house");
			data.AddValue("liberal", Liberal.ToString());
			data.AddValue("conservative", Conservative.ToString());
			data.AddValue("reactionary", Reactionary.ToString());
			data.AddValue("fascist", "0");
			data.AddValue("communist", "0");
			data.AddValue("anarcho_liberal", "0");
			data.AddValue("socialist", "0");
			return data;
		}
	}
	public enum slavery
	{
		no_slavery, yes_slavery
	}
	public enum upper_house_composition
	{
		party_appointed, appointed, state_equal_weight, population_equal_weight
	}
	public enum vote_franschise
	{
		none_voting, landed_voting, wealth_weighted_voting, wealth_voting, universal_weighted_voting, universal_voting
	}
	public enum voting_system
	{
		first_past_the_post, jefferson_method, proportional_representation
	}
	public enum public_meetings
	{
		no_meeting, yes_meeting
	}
	public enum press_rights
	{
		state_press, censored_press, free_press
	}
	public enum trade_unions
	{
		no_trade_unions, state_controlled, non_socialist, all_trade_unions
	}
	public enum political_parties
	{
		underground_parties, harassment, gerrymandering, non_secret_ballots, secret_ballots
	}
	public enum wage_reform
	{
		no_minimum_wage, trinket_wage, low_minimum_wage, acceptable_minimum_wage, good_minimum_wage
	}
	public enum work_hours
	{
		no_work_hour_limit, fourteen_hours, twelve_hours, ten_hours, eight_hours
	}
	public enum safety_regulations
	{
		no_safety, trinket_safety, low_safety, acceptable_safety, good_safety
	}
	public enum unemployment_subsidies
	{
		no_subsidies, trinket_subsidies, low_subsidies, acceptable_subsidies, good_subsidies
	}
	public enum pensions
	{
		no_pensions, trinket_pensions, low_pensions, acceptable_pensions, good_pensions
	}
	public enum health_care
	{
		no_health_care, trinket_health_care, low_health_care, acceptable_health_care, good_health_care
	}
	public enum school_reforms
	{
		no_schools, low_schools, acceptable_schools, good_schools
	}

	public class Reforms
	{
		//public static Type[] Types = { typeof(slavery), typeof(vote_franschise), typeof(upper_house_composition), typeof(voting_system), typeof(public_meetings), typeof(press_rights), typeof(trade_unions), typeof(political_parties), typeof(wage_reform), typeof( };

		public slavery slavery { get; set; }
		public upper_house_composition upper_house_composition { get; set; }
		public vote_franschise vote_franschise { get; set; }
		public voting_system voting_system { get; set; }
		public public_meetings public_meetings { get; set; }
		public press_rights press_rights { get; set; }
		public trade_unions trade_unions { get; set; }
		public political_parties political_parties { get; set; }


		//todo: fix
		public wage_reform wage_reform { get; set; }
		public work_hours work_hours { get; set; }
		public safety_regulations safety_regulations { get; set; }
		public unemployment_subsidies unemployment_subsidies { get; set; }
		public pensions pensions { get; set; }
		public health_care health_care { get; set; }
		public school_reforms school_reforms { get; set; }

		internal void AddData(PdxSublist data)
		{
			foreach (var reform in typeof(Reforms).GetProperties())
			{
				data.AddValue(reform.Name, Enum.GetName(reform.PropertyType, reform.GetValue(this)));

			}
		}
	}



	public class NV
	{
		public float Order { get; set; }
		public float Liberty { get; set; }
		public float Equality { get; set; }
		public string Value
		{
			get
			{
				if (Order > Liberty)
				{
					if (Order >= Equality)
					{
						return "nv_order";
					}
					else
					{
						return "nv_equality";
					}
				}
				else if (Equality > Liberty)
				{
					return "nv_equality";
				}
				else
				{
					return "nv_liberty";
				}
			}
		}

		public NV()
		{
			Order = 0;
			Liberty = 0;
			Equality = 0;
		}

	}
}