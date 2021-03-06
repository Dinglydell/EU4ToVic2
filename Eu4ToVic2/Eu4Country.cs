﻿using Eu4Helper;
using Eu4ToVic2;
using PdxFile;
using PdxUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{


	public class Eu4Country : Eu4CountryBase
	{
	
		public Eu4Country(PdxSublist country, Eu4Save save)
		{
			World = save;
			CountryTag = country.Key;
			Opinions = country.GetSublist("opinion_cache").Values.Select(int.Parse).ToList();
			//Console.WriteLine($"Loading {CountryTag}...");
			if (country.KeyValuePairs.ContainsKey("name"))
			{
				DisplayNoun = country.GetString("name").Replace("\"", string.Empty);
			} else { 
				DisplayNoun = save.Localisation[CountryTag];
			}
			if (country.KeyValuePairs.ContainsKey("adjective"))
			{
				DisplayAdj = country.GetString("adjective").Replace("\"", string.Empty);
			}
			else {
				DisplayAdj = save.Localisation[$"{CountryTag}_ADJ"];
			}

			Exists = country.Sublists.ContainsKey("owned_provinces");

			if (country.KeyValuePairs.ContainsKey("overlord"))
			{
				Overlord = country.GetString("overlord").Replace("\"", string.Empty);
			}
			Subjects = new List<string>();
			if (country.Sublists.ContainsKey("subjects"))
			{
				country.Sublists["subjects"].Values.ForEach(s =>
				{
					Subjects.Add(s);
				});
			}
			if (country.KeyValuePairs.ContainsKey("liberty_desire"))
			{
				LibertyDesire = float.Parse(country.GetString("liberty_desire"));
			}
			if (country.KeyValuePairs.ContainsKey("colonial_parent"))
			{
				IsColonialNation = true;
			}
			States = new HashSet<Eu4Area>();
			if (country.Sublists.ContainsKey("state"))
			{
				country.Sublists.ForEach("state", stData =>
				{
					var area = save.Areas[stData.KeyValuePairs["area"]];
					States.Add(area);
					area.Prosperity =  stData.GetFloat("prosperity");
					//area.Owner = this;
				});
			}

			var institutions = country.GetSublist("institutions");
			var listInstitutions = institutions.FloatValues[string.Empty].Select(ins => ins == 1).ToList();
			Institutions = new Dictionary<string, bool>();
			for(var i = 0; i < listInstitutions.Count; i++)
			{
				Institutions.Add(INSTITUTION_NAMES[i], listInstitutions[i]);
			}
			Capital = (int)country.GetFloat("capital");
			var colours = country.GetSublist("colors");
			var mapColour = colours.GetSublist("map_color");
			MapColour = new Colour(mapColour.FloatValues[string.Empty]);

			PrimaryCulture = country.GetString("primary_culture");

			AcceptedCultures = new List<string>();

			country.KeyValuePairs.ForEach("accepted_culture", (value) =>
			{
				AcceptedCultures.Add(value);
			});
			if (country.KeyValuePairs.ContainsKey("religion"))
			{
				Religion = country.GetString("religion");
			} else
			{
				Religion = (country.Sublists["history"].Sublists.Where(s => s.Value.KeyValuePairs.ContainsKey("religion")).OrderByDescending(s => PdxSublist.ParseDate(s.Key).Ticks).FirstOrDefault().Value ?? country.Sublists["history"]).GetString("religion");
			}
			

			GovernmentRank = (byte)country.GetFloat("government_rank");

			var tech = country.GetSublist("technology");
			AdmTech = (byte)tech.GetFloat("adm_tech");
			DipTech = (byte)tech.GetFloat("dip_tech");
			MilTech = (byte)tech.GetFloat("adm_tech");

			Estates = new List<Estate>();
			country.Sublists.ForEach("estate", (est) =>
			{
				Estates.Add(new Estate(est));
			});


			PowerProjection = LoadFloat(country, "current_power_projection");

			LastElection = country.GetDate("last_election");

			Prestige = LoadFloat(country, "prestige");

			Stability = (sbyte)country.GetFloat("stability");
			Inflation = LoadFloat(country, "inflation");
			

			country.GetAllMatchingSublists("loan", (loan) =>
			{
				Debt += (int)loan.GetFloat("amount");
			});

			Absolutism = LoadFloat(country,"absolutism");
			Legitimacy = LoadFloat(country,"legitimacy", 50);
			RepublicanTradition = LoadFloat(country, "republican_tradition", 50);
			Corruption = LoadFloat(country,"corruption");
			Mercantilism = LoadFloat(country,"mercantilism");

			Ideas = new Dictionary<string, byte>();
			var ideas = country.GetSublist("active_idea_groups");
			foreach (var idp in ideas.FloatValues)
			{
				Ideas.Add(idp.Key, (byte)idp.Value.Single());
			}

			Flags = country.Sublists.ContainsKey("flags") ? country.GetSublist("flags").KeyValuePairs.Keys.ToList() : new List<string>() ;
			Policies = new List<string>();
			country.Sublists.ForEach("active_policy", (pol) =>
			{
				Policies.Add(pol.GetString("policy"));
			});

			Government = country.GetSublist("government").GetString("government");
			if (country.Key == "GBR")
			{
			//	Console.WriteLine(Institutions);
			}
		}

		public override void AddDiplomacy(PdxSublist diplomacy)
		{
			throw new NotImplementedException();
		}

		public override PdxSublist GetCountryFile()
		{
			throw new NotImplementedException();
		}

		public override PdxSublist GetHistoryFile()
		{
			throw new NotImplementedException();
		}

		private float LoadFloat(PdxSublist country, string key, float deflt = 0)
		{
			if (!country.FloatValues.ContainsKey(key))
			{
				return deflt;
			}
			return country.GetFloat(key);
		}


	}
}
