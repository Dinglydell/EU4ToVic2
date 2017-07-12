using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	public enum V2Relation
	{
		vassal, alliance, invalid
	}
	public class Vic2DiploRelation
	{
		public V2Relation Type { get; set; }
		public Vic2Country First { get; set; }
		public Vic2Country Second { get; set; }

		public Vic2DiploRelation(Eu4DiploRelation eu4, Vic2World world)
		{
			Type = V2Relation.invalid;
			switch (eu4.Type)
			{
				case Relation.alliance:
					Type = V2Relation.alliance;
					break;
				case Relation.dependency:
					if(eu4.SubjectType == "vassal")
					{
						Type = V2Relation.vassal;
					}
					break;
					
			}
			First = world.GetCountry(eu4.First);
			Second = world.GetCountry(eu4.Second);
		}

		public PdxSublist GetData()
		{
			var data = new PdxSublist(null, Enum.GetName(typeof(V2Relation), Type));

			data.AddValue("first", First.CountryTag);
			data.AddValue("second", Second.CountryTag);
			data.AddValue("start_date", "1836.1.1");
			data.AddValue("end_date", "1936.1.1");

			return data;
		}
	}
}
