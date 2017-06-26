using System;
using System.Collections.Generic;

namespace Eu4ToVic2
{
	class Vic2World
	{

		public Eu4Save Eu4Save { get; set; }


		public ProvinceMapper ProvMapper { get; set; }
		public Mapper V2Mapper { get; set; }
		public List<Vic2Country> Vic2Countries { get; set; }
		public PdxSublist Effects { get; set; }
		public Dictionary<Ideology, IdeologyModifier> IdeologyModifiers { get; set; }
		public Vic2World(Eu4Save eu4Save)
		{

			Eu4Save = eu4Save;
			V2Mapper = new Mapper();
			ProvMapper = new ProvinceMapper("province_mappings.txt");
			LoadEffects();
			LoadPoliticalParties();
			Console.WriteLine("Constructing Vic2 world...");
			GenerateCountries();
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
			Console.WriteLine("Loading effects.txt...");
			Effects = PdxSublist.ReadFile("effects.txt");
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

	public static class Policies {
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

		public static IdeologyModifier operator +(IdeologyModifier a, IdeologyModifier b) {
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