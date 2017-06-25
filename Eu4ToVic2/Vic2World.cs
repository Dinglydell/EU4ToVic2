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

		public Vic2World(Eu4Save eu4Save)
		{
			
			Eu4Save = eu4Save;
			V2Mapper = new Mapper();
			ProvMapper = new ProvinceMapper("province_mappings.txt");
			Console.WriteLine("Constructing Vic2 world...");
			GenerateCountries();
		}

		private void GenerateCountries()
		{
			Console.WriteLine("Creating Vic2 countries...");
			Vic2Countries = new List<Vic2Country>();
			foreach(var eu4Country in Eu4Save.Countries.Values)
			{
				var v2Country = new Vic2Country(this, eu4Country);
				Vic2Countries.Add(v2Country);
			}

			Console.WriteLine($"Created {Vic2Countries.Count} Vic2 countries...");
		}
	}
}