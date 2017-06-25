using System;
using System.Collections.Generic;

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

		public bool IsCivilised { get; set; }

		public float Prestige { get; set; }

		public PReforms PoliticalReforms { get; set; }
		public SReforms SocialReforms { get; set; }

		public UHouse UpperHouse { get; set; }

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
			CalcNationalValues();
			CalcReforms();
			CalcUpperHouse();
		}

		private void CalcUpperHouse()
		{
			//throw new NotImplementedException();
			UpperHouse = new UHouse();
		}

		private void CalcReforms()
		{
			//throw new NotImplementedException();
			PoliticalReforms = new PReforms();
			SocialReforms = new SReforms();
		}

		private void CalcNationalValues()
		{
			//throw new NotImplementedException();
			NationalValues = new NV();
		}
	}

	public class UHouse
	{
		public int Liberal { get; set; }
		public int Conservative { get; set; }
		public int Reactionary { get; set; }
	}

	public class PReforms
	{
		public enum SlaveReform
		{
			no_slavery, yes_slavery
		}
		public enum UpperHouseReform
		{
			party_appointed, appointed, state_equal_weight, population_equal_weight
		}
		public enum VotingReform
		{
			none_voting, landed_voting, wealth_weighted_voting, wealth_voting, universal_weighted_voting, universal_voting
		}
		public enum VotingSystemReform
		{
			first_past_the_post, jefferson_method, proportional_representation
		}
		public enum PublicMeetingsReform
		{
			no_meeting, yes_meeting
		}
		public enum PressRightsReform
		{
			state_press, censored_press, free_press
		}
		public enum TradeUnionsReform
		{
			no_trade_unions, state_controlled, non_socialist, all_trade_unions
		}
		public enum PoliticalPartiesReform
		{
			underground_parties, harassment, gerrymandering, non_secret_ballots, secret_ballots
		}


		public SlaveReform Slavery { get; set; }
		public UpperHouseReform UpperHouse { get; set; }
		public VotingReform Voting { get; set; }
		public VotingSystemReform VotingSystem { get; set; }
		public PublicMeetingsReform PublicMeetings { get; set; }
		public PressRightsReform PressRights { get; set; }
		public TradeUnionsReform TradeUnions { get; set; }
		public PoliticalPartiesReform PoliticalParties { get; set; }
	}

	public class SReforms
	{
		public enum WageReform
		{
			no_minimum_wage, trinket_wage, low_minimum_wage, acceptable_minimum_wage, good_minimum_wage
		}
		public enum WorkHoursReform
		{
			no_work_hour_limit, fourteen_hours, twelve_hours, ten_hours, eight_hours
		}
		public enum SafetyRegulationReform
		{
			no_safety, trinket_safety, low_safety, acceptable_safety, good_safety
		}
		public enum UnemploymentSubsidiesReform
		{
			no_subsidies, trinket_subsidies, low_subsidies, acceptable_subsidies, good_subsidies
		}
		public enum PensionsReform
		{
			no_pensions, trinket_pensions, low_pensions, acceptable_pensions, good_pensions
		}
		public enum HealthCareReform
		{
			no_health_care, trinket_health_care, low_health_care, acceptable_health_care, good_health_care
		}
		public enum SchoolReform
		{
			no_schools, low_schools, acceptable_schools, good_schools
		}

		public WageReform Wage { get; set; }
		public WorkHoursReform WorkHours { get; set; }
		public SafetyRegulationReform SafetyRegulation { get; set; }
		public UnemploymentSubsidiesReform UnemploymentSubsidies { get; set; }
		public PensionsReform Pensions { get; set; }
		public HealthCareReform HealthCare { get; set; }
		public SchoolReform School { get; set; }

	}


	public class NV
	{
		public int Order { get; set; }
		public int Liberty { get; set; }
		public int Equality { get; set; }

		public NV()
		{
			Order = 0;
			Liberty = 0;
			Equality = 0;
		}
	}
}