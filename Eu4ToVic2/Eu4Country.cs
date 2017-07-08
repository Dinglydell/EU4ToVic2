using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	public class Colour
	{
		public byte Red { get; set; }
		public byte Green { get; set; }
		public byte Blue { get; set; }

		public Colour(byte r, byte g, byte b)
		{
			Red = r;
			Green = g;
			Blue = b;
		}


		public Colour(List<string> rgb) : this(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]))
		{
		}

		public Colour(List<string> rgb, byte multiplier): this((byte)(multiplier * float.Parse(rgb[0])), (byte)(multiplier * float.Parse(rgb[1])), (byte)(multiplier * float.Parse(rgb[2])))
		{

		}

	}

	public class Estate
	{
		public static readonly string[] EstateTypes = new string[] { null, "estate_church", "estate_nobles", "estate_burghers", "estate_cossacks", "estate_nomadic_tribes", "estate_dhimmi"};
		public string Type { get; set; }
		public float Loyalty { get; set; }
		public float Influence { get; set; }
		public float Territory { get; set; }

		public Estate(PdxSublist estate)
		{
			Type = estate.GetStringValue("type");
			Loyalty = float.Parse(estate.KeyValuePairs["loyalty"]);
			Influence = float.Parse(estate.KeyValuePairs["influence"]);
			Territory = float.Parse(estate.KeyValuePairs["territory"]);
		}
	}

	public class Eu4Country
	{

		public bool Exists { get; set; }


		public byte GovernmentRank { get; set; }

		public List<bool> Institutions { get; private set; }
		public string CountryTag { get; set; }
		public int Capital { get; set; }

		public Colour MapColour { get; set; }

		public string PrimaryCulture { get; set; }
		public List<string> AcceptedCultures { get; set; }
		public string Religion { get; set; }

		public byte AdmTech { get; set; }
		public byte DipTech { get; set; }
		public byte MilTech { get; set; }

		public List<Estate> Estates { get; set; }

		public float PowerProjection { get; set; }

		public DateTime LastElection { get; set; }

		public float Prestige { get; set; }
		public sbyte Stability { get; private set; }
		public float Inflation { get; private set; }


		public int Debt { get; set; }

		public float Absolutism { get; set; }
		public float Legitimacy { get; set; }
		public float RepublicanTradition { get; set; }
		public float Corruption { get; set; }
		public float Mercantilism { get; private set; }

		public Dictionary<string, byte> Ideas { get; set; }

		public string Government { get; private set; }

		public List<string> Flags { get; set; }

		public List<string> Policies { get; set; }

		public Eu4Country(PdxSublist country)
		{
			CountryTag = country.Key;
			//Console.WriteLine($"Loading {CountryTag}...");

			Exists = country.Sublists.ContainsKey("owned_provinces");

			var institutions = country.Sublists["institutions"];
			Institutions = institutions.Values.Select(ins => int.Parse(ins) == 1).ToList();
			Capital = int.Parse(country.KeyValuePairs["capital"]);
			var colours = country.Sublists["colors"];
			var mapColour = colours.Sublists["map_color"];
			MapColour = new Colour(mapColour.Values);

			PrimaryCulture = country.KeyValuePairs["primary_culture"];

			AcceptedCultures = new List<string>();

			country.GetAllMatchingKVPs("accepted_culture", (value) =>
			{
				AcceptedCultures.Add(value);
			});

			Religion = country.KeyValuePairs["religion"];

			GovernmentRank = byte.Parse(country.KeyValuePairs["government_rank"]);

			var tech = country.Sublists["technology"];
			AdmTech = byte.Parse(tech.KeyValuePairs["adm_tech"]);
			DipTech = byte.Parse(tech.KeyValuePairs["dip_tech"]);
			MilTech = byte.Parse(tech.KeyValuePairs["adm_tech"]);

			Estates = new List<Estate>();
			country.GetAllMatchingSublists("estate", (est) =>
			{
				Estates.Add(new Estate(est));
			});


			PowerProjection = LoadFloat(country, "current_power_projection");

			LastElection = country.GetDate("last_election");

			Prestige = LoadFloat(country, "prestige");

			Stability = (sbyte)float.Parse(country.KeyValuePairs["stability"]);
			Inflation = LoadFloat(country, "inflation");
			

			country.GetAllMatchingSublists("loan", (loan) =>
			{
				Debt += int.Parse(loan.KeyValuePairs["amount"]);
			});

			Absolutism = LoadFloat(country,"absolutism");
			Legitimacy = LoadFloat(country,"legitimacy", 50);
			RepublicanTradition = LoadFloat(country, "republican_tradition", 50);
			Corruption = LoadFloat(country,"corruption");
			Mercantilism = LoadFloat(country,"mercantilism");

			Ideas = new Dictionary<string, byte>();
			var ideas = country.Sublists["active_idea_groups"];
			foreach(var idp in ideas.KeyValuePairs)
			{
				Ideas.Add(idp.Key, byte.Parse(idp.Value));
			}

			Flags = country.Sublists["flags"].KeyValuePairs.Keys.ToList();
			Policies = new List<string>();
			country.GetAllMatchingSublists("active_policy", (pol) =>
			{
				Policies.Add(pol.KeyValuePairs["policy"]);
			});

			Government = country.KeyValuePairs["government"];
			if (country.Key == "GBR")
			{
			//	Console.WriteLine(Institutions);
			}
		}

		private float LoadFloat(PdxSublist country, string key, float deflt = 0)
		{
			if (!country.KeyValuePairs.ContainsKey(key))
			{
				return deflt;
			}
			return float.Parse(country.KeyValuePairs[key]);
		}
	}
}
