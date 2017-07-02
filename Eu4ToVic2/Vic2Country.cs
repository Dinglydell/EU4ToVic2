using System;
using System.Collections.Generic;
using System.Linq;

namespace Eu4ToVic2
{
	internal class Vic2Country
	{
		public Eu4Country Eu4Country { get; set; }
		public string CountryTag { get; set; }
		public Colour MapColour { get; set; }
		public string GraphicalCulture { get; set; }

		public int Capital { get; set; }

		public string PrimaryCulture { get; set; }

		public List<string> AcceptedCultures { get; set; }

		public string Religion { get; set; }

		public string Government { get; set; }

		public NV NationalValues { get; set; }

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

		public Vic2Country(Vic2World vic2World, Eu4Country eu4Country)
		{
			CountryTag = vic2World.V2Mapper.GetV2Country(eu4Country.CountryTag);
			Eu4Country = eu4Country;
			MapColour = eu4Country.MapColour;
			GraphicalCulture = "Generic";

			PrimaryCulture = vic2World.V2Mapper.GetV2Culture(eu4Country.PrimaryCulture);
			AcceptedCultures = eu4Country.AcceptedCultures.ConvertAll(c => vic2World.V2Mapper.GetV2Culture(c));

			Religion = vic2World.V2Mapper.GetV2Religion(eu4Country.Religion);

			Government = vic2World.V2Mapper.GetV2Government(eu4Country);


			//base literacy
			Literacy = 0.1f;

			IsCivilised = eu4Country.Institutions.TrueForAll(b => b);
			// -100 - 100 scaled to 0 - 100
			Prestige = (eu4Country.Prestige + 100) / 2;

			CalcEffects(vic2World);

			if (CountryTag == "ENG")
			{
				Console.WriteLine("GBR!");
			}
		}

		private void CalcEffects(Vic2World vic2World)
		{
			NationalValues = new NV();

			Reforms = new Reforms();
			var reforms = new Dictionary<Type, float>();

			UpperHouse = new UHouse();

			PoliticalParties = new List<PoliticalParty>();
			var baseModifier = new IdeologyModifier();
			IterateEffects(vic2World, (Dictionary<string, float> effects) =>
			{
				CalcNationalValues(effects);
				CalcReforms(effects, reforms);
				CalcUpperHouse(effects);
				CalcPoliticalParties(effects, baseModifier);
				CalcLiteracy(effects);
				CalcConsciousness(effects);
				CalcMilitancy(effects);
			});
			FinaliseReforms(reforms);
			FinalisePoliticalParties(vic2World, baseModifier);


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
				var polParty = new PoliticalParty(ideology.Key);
				var totalModifier = baseModifier + ideology.Value;
				foreach (var pol in totalModifier.Policies)
				{
					polParty.SetIssue(pol.Key, pol.Value);
				}
				PoliticalParties.Add(polParty);
				if(ideology.Key == UpperHouse.GetMode())
				{

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
			//idea groups
			foreach (var idea in Eu4Country.Ideas)
			{
				if (vic2World.Effects.Sublists["ideas"].Sublists.ContainsKey(idea.Key))
				{
					callback(vic2World.Effects.Sublists["ideas"].Sublists[idea.Key].KeyValuePairs.ToDictionary(effect => effect.Key, effect => idea.Value * float.Parse(effect.Value)));
				}
			}

			// country flags
			foreach (var flag in Eu4Country.Flags)
			{
				if (vic2World.Effects.Sublists["country_flags"].Sublists.ContainsKey(flag))
				{
					callback(vic2World.Effects.Sublists["country_flags"].Sublists[flag].KeyValuePairs.ToDictionary(effect => effect.Key, effect => float.Parse(effect.Value)));
				}
			}
			// religion
			if (vic2World.Effects.Sublists["religion"].Sublists.ContainsKey(Eu4Country.Religion))
			{
				callback(vic2World.Effects.Sublists["religion"].Sublists[Eu4Country.Religion].KeyValuePairs.ToDictionary(effect => effect.Key, effect => float.Parse(effect.Value)));
			}

			// government
			if (vic2World.Effects.Sublists["government"].Sublists.ContainsKey(Eu4Country.Government))
			{
				callback(vic2World.Effects.Sublists["government"].Sublists[Eu4Country.Government].KeyValuePairs.ToDictionary(effect => effect.Key, effect => float.Parse(effect.Value)));
			}

			// policies
			foreach (var policy in Eu4Country.Policies)
			{
				if (vic2World.Effects.Sublists["policies"].Sublists.ContainsKey(policy))
				{
					callback(vic2World.Effects.Sublists["policies"].Sublists[policy].KeyValuePairs.ToDictionary(effect => effect.Key, effect => float.Parse(effect.Value)));
				}
			}

			// values
			//mercantilism
			ValueEffect(vic2World, callback, "mercantilism", Eu4Country.Mercantilism);
			ValueEffect(vic2World, callback, "legitimacy", Eu4Country.Legitimacy);// - 50);
			ValueEffect(vic2World, callback, "republican_tradition", Eu4Country.RepublicanTradition);// - 50);
			ValueEffect(vic2World, callback, "stability", Eu4Country.Stability);
			ValueEffect(vic2World, callback, "absolutism", Eu4Country.Absolutism);

		}

		private void ValueEffect(Vic2World vic2World, Action<Dictionary<string, float>> callback, string key, float value)
		{
			if (vic2World.Effects.Sublists["values"].Sublists.ContainsKey(key))
			{
				var average = 0f;
				if (vic2World.Effects.Sublists["values"].Sublists[key].KeyValuePairs.ContainsKey("average"))
				{
					average = float.Parse(vic2World.Effects.Sublists["values"].Sublists[key].KeyValuePairs["average"]);
				}


				callback(vic2World.Effects.Sublists["values"].Sublists[key].KeyValuePairs.ToDictionary(effect => effect.Key, effect => (value - average) * float.Parse(effect.Value)));
			}
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

		public PoliticalParty(Ideology ideology)
		{
			Ideology = ideology;
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
			if(conservative > 33)
			{
				return Ideology.conservative;
			} else if(liberal > 33)
			{
				return Ideology.liberal;
			} else
			{
				return Ideology.reactionary;
			}
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
	}



	public class NV
	{
		public float Order { get; set; }
		public float Liberty { get; set; }
		public float Equality { get; set; }

		public NV()
		{
			Order = 0;
			Liberty = 0;
			Equality = 0;
		}

	}
}