using Eu4Helper;
using PdxFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	public class Eu4DiploRelation: IEu4DiploRelation
	{
		public Relation Type { get; set; }
		public Eu4CountryBase First { get; set; }
		public Eu4CountryBase Second { get; set; }

		public string SubjectType { get; set; }
		public Eu4DiploRelation(PdxSublist data, Eu4Save save)
		{
			Type = (Relation)Enum.Parse(typeof(Relation), data.Key);
			First = save.Countries[data.GetString("first")];
			Second = save.Countries[data.GetString("second")];

			if(Type == Relation.dependency)
			{
				SubjectType = data.GetString("subject_type");
			}
		}
	}
}
