﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	class ProvinceMapper
	{
		//public static ProvinceMapper instance;
		public PdxSublist Mappings { get; set; }
		public ProvinceMapper(string mappingPath)
		{
			//instance = this;
			Console.WriteLine("Loading province mappings...");
			Mappings = PdxSublist.ReadFile(mappingPath);
		}
	}
}