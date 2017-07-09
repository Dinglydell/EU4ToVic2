using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	public class PdxSublist
	{

		public PdxSublist(PdxSublist parent = null, string key = null)
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
				value.Key = key;
				Sublists.Add(Uniquify(key, Sublists), value);
			}
			value.Parent = this;
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

		internal void WriteToFile(StreamWriter file, int indentation = 0)
		{
			if(KeyValuePairs.Count != 0 || Sublists.Count != 0 || KeylessSublists.Count != 0)
			{
				file.WriteLine();
			}
			
			//using (var file = File.CreateText(path))
			//{

			// TODO: make KeyValueParis a Dictionary<string, List<string>> to properly solve the duplicate key issue
			var rgx = new Regex(@"\d+$");
			foreach (var kvp in KeyValuePairs)
			{
				var newKey =  rgx.Replace(kvp.Key, string.Empty);
				if (!KeyValuePairs.ContainsKey(newKey))
				{
					newKey = kvp.Key;
				}
				file.WriteLine($"{new String('\t', indentation)}{newKey} = {kvp.Value}");
			}
			foreach (var v in Values)
			{
				file.Write(v + " ");
			}
			foreach (var sub in Sublists)
			{
				var newKey = rgx.Replace(sub.Key, string.Empty);
				if (!Sublists.ContainsKey(newKey))
				{
					newKey = sub.Key;
				}
				file.Write($"{new String('\t', indentation)}{newKey} = {{");
				sub.Value.WriteToFile(file, indentation + 1);
				file.WriteLine(new String('\t', indentation) + "}");
			}
			foreach (var sub in KeylessSublists)
			{
				file.Write(new String('\t', indentation) + "{");
				sub.WriteToFile(file, indentation + 1);
				file.WriteLine(new String('\t', indentation) + "}");
			}
			//}
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

		public PdxSublist Parent { get; set; }
		private static ReadState State { get; set; }
		private static string ReadKey { get; set; }

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
			State = ReadState.normal;
			//TODO: write a much more sophisticated file reader
			var file = new StreamReader(filePath, Encoding.Default);
			string line;
			if (firstLine != null)
			{
				line = file.ReadLine();
				if (line != firstLine)
				{
					throw new Exception("Not a valid file");
				}

			}
			var rootList = new PdxSublist(null, filePath);
			var currentList = rootList;
			var lineNumber = 0;
			while ((line = file.ReadLine()) != null)
			{
				lineNumber++;
				if(lineNumber == 1513567)
				{
					Console.WriteLine("Oh");
				}
				currentList = RunLine(line, currentList);
			}
			if (currentList != rootList)
			{
				throw new Exception("An unknown error occurred.");
			}
			file.Close();
			return rootList;
		}

		internal void AddDate(string key, DateTime date)
		{
			AddString(key, $"{date.Year}.{date.Month}.{date.Day}");
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
			if (State == ReadState.value)
			{
				key = ReadKey;
			}
			var value = line.Substring(line.IndexOf('=') + 1).Trim();

			if (line.Contains('='))
			{
				key = RemoveWhitespace(line.Substring(0, line.IndexOf('=')));
			}
			else if (value == "}")
			{
				return currentList.Parent;

			}
			if (string.IsNullOrWhiteSpace(value))
			{
				State = ReadState.value;
				ReadKey = key;
			}
			var parent = 0;
			if (value.Contains('}'))
			{
				parent = value.Count(c => c == '}');
				
				
				value = value.Substring(0, value.IndexOf('}')).Trim();
				
			}

			if (value.FirstOrDefault() == '{')
			{
				var list = new PdxSublist(currentList, key);
				
				if (line.Contains('}'))
				{
					if (line.IndexOf('}') < line.IndexOf('{'))
					{
						currentList = currentList.Parent;
						key = key.Substring(key.IndexOf('}') + 1);
						list.Key = key;
						list.Parent = currentList;
					}
					else {
						parent = 1;
						var open = line.IndexOf('{');
						value = line.Substring(open + 1, line.IndexOf('}') - open - 1);
						if (value.Contains('='))
						{
							SingleLineKeyValuePairs(key, value, list);
						}
						else {
							SingleLineArray(key, value, list);
						}
					}
				}
				currentList.AddSublist(key, list);
				currentList = list;


			}
			else if (key == null)
			{
				// awkward single line array of numbers
				value = line.Substring(line.IndexOf('=') + 1).Trim();
				SingleLineArray(key, value, currentList);
			}
			else
			{
				currentList.AddString(key, value);
			}
			for (var i = 0; i < parent; i++)
			{
				currentList = currentList.Parent;
			}
			return currentList;
		}

		private static void SingleLineKeyValuePairs(string key, string value, PdxSublist currentList)
		{
			string k = string.Empty;
			string v = string.Empty;
			bool readingKey = true;
			bool inQuotes = false;
			foreach (var ch in value)
			{
				if (ch == '"')
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
			var numValues = new List<string>();
			var inQuotes = false;
			var nextVal = new StringBuilder();
			foreach (var ch in value)
			{
				if(ch == '}')
				{
					break;
				}
				if (!inQuotes && char.IsWhiteSpace(ch))
				{
					if (nextVal.Length > 0)
					{
						numValues.Add(nextVal.ToString());
					}
					nextVal = new StringBuilder();
					continue;
				}
				if(ch == '"')
				{
					inQuotes = !inQuotes;
					continue;
				}

				nextVal.Append(ch);
			}
			if (nextVal.Length > 0)
			{
				numValues.Add(nextVal.ToString());
			}
			foreach (var val in numValues)
			{
				currentList.AddString(null, val);
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

		enum ReadState
		{
			normal, value
		}
	}
}
