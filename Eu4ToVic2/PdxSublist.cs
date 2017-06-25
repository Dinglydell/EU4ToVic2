using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	class PdxSublist
	{

		public PdxSublist(PdxSublist parent, string key = null)
		{
			this.Key = key;
			this.Parent = parent;
			KeyValuePairs = new Dictionary<string, string>();
			Values = new List<string>();
			Sublists = new Dictionary<string, PdxSublist>();
			KeylessSublists = new List<PdxSublist>();
		}

		public string GetStringValue(string key)
		{
			return DeQuote(KeyValuePairs[key]);
		}

		private string DeQuote(string value)
		{
			if (value.FirstOrDefault() == '"' && value.LastOrDefault() == '"')
			{
				value = value.Substring(1, value.Length - 2);
			}
			return value;
		}

		public void AddString(string key, string value)
		{
			if (key == null)
			{
				Values.Add(value);
			}
			else
			{
				KeyValuePairs.Add(Uniquify(key, KeyValuePairs), value);
			}
		}

		private string Uniquify<T>(string key, Dictionary<string, T> diction)
		{
			var nkey = key;
			for (var i = 1; diction.ContainsKey(nkey); i++)
			{
				nkey = key + i;
			}
			return nkey;
		}

		public void AddSublist(string key, PdxSublist value)
		{
			if (key == null)
			{
				KeylessSublists.Add(value);
			}
			else
			{

				Sublists.Add(Uniquify(key, Sublists), value);
			}
		}

		/** Calls back on each value matching the key when there are multiple keys */
		public void GetAllMatchingKVPs(string key, Action<string> callback)
		{
			GetAllMatching(key, (v) => callback(DeQuote(v)), KeyValuePairs);
		}

		public void GetAllMatchingSublists(string key, Action<PdxSublist> callback)
		{
			GetAllMatching(key, callback, Sublists);
		}

		private void GetAllMatching<T>(string key, Action<T> callback, Dictionary<string, T> diction)
		{
			var nkey = key;
			for (var i = 0; diction.ContainsKey(nkey); i++, nkey = key + (i == 0 ? string.Empty : i.ToString()))
			{
				callback(diction[nkey]);
			}
		}

		//public void AddNumber(float num)
		//{
		//	NumericValues.Add(num);
		//}

		public string Key { get; set; }

		public Dictionary<string, string> KeyValuePairs { get; set; }

		public Dictionary<string, PdxSublist> Sublists { get; set; }

		public List<string> Values { get; set; }

		//public List<float> NumericValues { get; set; }


		public List<PdxSublist> KeylessSublists { get; set; }

		public PdxSublist Parent { get; private set; }

		public DateTime GetDate(string key)
		{
			return ParseDate(KeyValuePairs[key]);

		}

		public static DateTime ParseDate(string dateStr)
		{

			var dateParts = dateStr.Split('.').Select((p) => int.Parse(p)).ToList();
			return new DateTime(dateParts[0], dateParts[1], dateParts[2]);
		}


		public static PdxSublist ReadFile(string filePath, string firstLine = null)
		{
			//TODO: write a much more sophisticated file reader
			var file = new StreamReader(filePath);
			string line;
			if (firstLine != null)
			{
				line = file.ReadLine();
				if (line != firstLine)
				{
					throw new Exception("Not a valid file");
				}
			}
			var rootList = new PdxSublist(null);
			var currentList = rootList;
			//var lineNumber = 0;
			while ((line = file.ReadLine()) != null)
			{
				//lineNumber++;
				currentList = RunLine(line, currentList);
			}
			if (currentList != rootList)
			{
				throw new Exception("An unknown error occurred.");
			}
			return rootList;
		}


		public static PdxSublist RunLine(string line, PdxSublist currentList)
		{
			
			if (line.Contains('#'))
			{
				//filter out comment
				line = line.Substring(0, line.IndexOf('#'));
			}
			if (string.IsNullOrWhiteSpace(line))
			{
				return currentList;
			}
			string key = null;
			var value = RemoveWhitespace(line.Substring(line.IndexOf('=') + 1));

			if (line.Contains('='))
			{
				key = RemoveWhitespace(line.Substring(0, line.IndexOf('=')));
			}
			else if (value == "}")
			{
				return currentList.Parent;

			}
			var parent = false;
			if (value.Contains('}'))
			{
				value = RemoveWhitespace(value.Substring(0, value.IndexOf('}')));
				parent = true;
			}

			if (value.FirstOrDefault() == '{')
			{
				var list = new PdxSublist(currentList, key);
				currentList.AddSublist(key, list);

				if (line.Contains('}'))
				{
					parent = false;
					var open = line.IndexOf('{');
					value = line.Substring(open + 1 , line.IndexOf('}') - open - 1);
					if (value.Contains('='))
					{
						SingleLineKeyValuePairs(key, value, list);
					}
					else {
						SingleLineArray(key, value, list);
					}
				}
				else {
					currentList = list;
				}


			}
			else if (key == null && !value.Contains('"'))
			{
				// awkward single line array of numbers
				value = line.Substring(line.IndexOf('=') + 1).Trim();
				SingleLineArray(key, value, currentList);
			}
			else
			{
				currentList.AddString(key, value);
			}
			return parent ? currentList.Parent : currentList;
		}

		private static void SingleLineKeyValuePairs(string key, string value, PdxSublist currentList)
		{
			string k = string.Empty;
			string v = string.Empty;
			bool readingKey = true;
			bool inQuotes = false;
			foreach (var ch in value)
			{
				if(ch == '"')
				{
					//toggle whether we're currently reading inside quotes
					inQuotes = !inQuotes;
					continue;
				}
				//if we're not in quotes, special characters apply
				if (!inQuotes)
				{
					if (char.IsWhiteSpace(ch))
					{
						//if we're not in quotes, are currently reading the value, have a whitespace and value is not empty then this indicates the value is finished
						if (!readingKey && !string.IsNullOrEmpty(v))
						{
							readingKey = true;
							currentList.AddString(k, v);
							k = string.Empty;
							v = string.Empty;
						}
						//whitespace not added to the value when not in quotes
						continue;
					}
					//= sign indicates we've finished reading the key and are now reading the value
					if (ch == '=')
					{
						readingKey = false;

						continue;
					}

					
				}
				//if we're reading the key, add the character to the key else add it to the value
				if (readingKey)
				{
					k += ch;
				}
				else {
					v += ch;
				}
				//currentList.AddString(key, val);
			}
			//last entry leftover
			if (!readingKey && !string.IsNullOrEmpty(v))
			{
				currentList.AddString(k, v);
			}
		}

		private static void SingleLineArray(string key, string value, PdxSublist currentList)
		{
			var numValues = value.Split(' ');
			foreach (var val in numValues)
			{
				currentList.AddString(key, val);
			}
		}

		private static string RemoveWhitespace(string str)
		{
			var inQuotes = false;
			var newStr = new StringBuilder();
			foreach (var ch in str)
			{
				if (ch == '"')
				{
					inQuotes = !inQuotes;

				}
				//if it's whitespace outside of quotes, skip it
				if ((ch == ' ' || ch == '\t') && !inQuotes && !(ch == '"'))
				{
					continue;
				}
				newStr.Append(ch);
			}
			//return Regex.Replace(str, @"\s+", String.Empty);
			return newStr.ToString();
		}
	}
}
