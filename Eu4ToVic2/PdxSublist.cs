using System;
using System.Collections;
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
			keyValuePairs = new Dictionary<string, List<string>>();
			KeyValuePairs = new PseudoDictionary<string, string>(keyValuePairs);
			BoolValues = new Dictionary<string, List<bool>>();
			FloatValues = new Dictionary<string, List<float>>();
			sublists = new Dictionary<string, List<PdxSublist>>();
			Sublists = new PseudoDictionary<string, PdxSublist>(sublists);
			KeylessSublists = new List<PdxSublist>();
		}

		public void ForEachSublist(Action<KeyValuePair<string, PdxSublist>> callback)
		{
			foreach (var sub in sublists)
			{
				sub.Value.ForEach(v => callback(new KeyValuePair<string, PdxSublist>(sub.Key, v)));
			}
		}
		public void ForEachString(Action<KeyValuePair<string, string>> callback)
		{
			foreach (var sub in keyValuePairs)
			{
				sub.Value.ForEach(v => callback(new KeyValuePair<string, string>(sub.Key, v)));
			}
		}
		public string GetString(string key)
		{
			return keyValuePairs[key].Single();
		}
		public bool GetBool(string key)
		{
			return BoolValues[key].Single();
		}

		//public int GetInt(string key)
		//{
		//	return IntValues[key].Single();
		//}

		public float GetFloat(string key)
		{
			return FloatValues[key].Single();
		}

		public PdxSublist GetSublist(string key)
		{
			return sublists[key].Single();
		}

		public void AddValue(string key, string value)
		{
			//if(Key == "active_idea_groups" && Parent.Key == "LVA")
			//{
			//	Console.WriteLine("yes");
			//}
			if (value == "yes" || value == "no")
			{
				AddValue(BoolValues, key, value == "yes");
				return;
			}
			//int i;
			//if(int.TryParse(value, out i))
			//{
			//	if(IntValues == null)
			//	{
			//		IntValues = new Dictionary<string, List<int>>();
			//	}
			//	AddValue(IntValues, key, i);
			//}
			float f;
			if (float.TryParse(value, out f))
			{
				AddValue(FloatValues, key, f);

			}

			AddValue(keyValuePairs, key, value);

		}


		private void AddValue<T>(Dictionary<string, List<T>> to, string key, T value)
		{
			if (!to.ContainsKey(key))
			{
				to[key] = new List<T>();
			}
			to[key].Add(value);
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
				if (!Sublists.ContainsKey(key))
				{
					sublists[key] = new List<PdxSublist>();
				}
				sublists[key].Add(value);
			}
			value.Parent = this;
		}

		/** Calls back on each value matching the key when there are multiple keys */
		[System.Obsolete]
		public void GetAllMatchingKVPs(string key, Action<string> callback)
		{
			GetAllMatching(key, callback, keyValuePairs);
		}
		[System.Obsolete]
		public void GetAllMatchingSublists(string key, Action<PdxSublist> callback)
		{
			GetAllMatching(key, callback, sublists);
		}
		[System.Obsolete]
		private void GetAllMatching<T>(string key, Action<T> callback, Dictionary<string, List<T>> diction)
		{
			if (diction.ContainsKey(key))
			{
				diction[key].ForEach(v => callback(v));
			}

		}

		internal void WriteToFile(StreamWriter file, int indentation = 0)
		{
			if (keyValuePairs.Count != 0 || sublists.Count != 0 || KeylessSublists.Count != 0)
			{
				file.WriteLine();
			}

			//using (var file = File.CreateText(path))
			//{

			// TODO: make KeyValueParis a Dictionary<string, List<string>> to properly solve the duplicate key issue
			var rgx = new Regex(@"\d+$");
			foreach (var kvp in keyValuePairs)
			{
				//var newKey = rgx.Replace(kvp.Key, string.Empty);
				Write(file, indentation, kvp);

			}
			foreach (var kvp in BoolValues)
			{
				Write(file, indentation, kvp);
			}
			foreach (var kvp in FloatValues)
			{
				Write(file, indentation, kvp);
			}
			foreach (var sub in sublists)
			{
				//	var newKey = rgx.Replace(sub.Key, string.Empty);
				//if (!Sublists.ContainsKey(newKey))
				//{
				//	newKey = sub.Key;
				//}
				if (sub.Key == string.Empty)
				{
					continue;
				}
				sub.Value.ForEach(s =>
				{
					file.Write($"{new String('\t', indentation)}{sub.Key} = {{");
					s.WriteToFile(file, indentation + 1);
					file.WriteLine(new String('\t', indentation) + "}");
				});

			}
			foreach (var sub in KeylessSublists)
			{
				file.Write(new String('\t', indentation) + "{");
				sub.WriteToFile(file, indentation + 1);
				file.WriteLine(new String('\t', indentation) + "}");
			}
			//}
		}

		private void Write<T>(StreamWriter file, int indentation, KeyValuePair<string, List<T>> kvp)
		{

			kvp.Value.ForEach(v =>
			{
				if (kvp.Key == string.Empty)
				{
					file.Write(kvp.Value.ToString() + " ");
				}
				else
				{
					file.WriteLine($"{new String('\t', indentation)}{kvp.Key} = {v.ToString()}");
				}
			});
		}



		//public void AddNumber(float num)
		//{
		//	NumericValues.Add(num);
		//}

		public string Key { get; set; }

		public PseudoDictionary<string, string> KeyValuePairs { get; set; }
		private Dictionary<string, List<string>> keyValuePairs;
		public Dictionary<string, List<float>> FloatValues { get; set; }
		//public Dictionary<string, List<int>> IntValues { get; set; }
		public Dictionary<string, List<bool>> BoolValues { get; set; }

		public PseudoDictionary<string, PdxSublist> Sublists { get; set; }
		private Dictionary<string, List<PdxSublist>> sublists;

		public List<string> Values { get { return keyValuePairs.ContainsKey(string.Empty) ? keyValuePairs[string.Empty] : new List<string>(); } }

		//public List<float> NumericValues { get; set; }


		public List<PdxSublist> KeylessSublists { get; set; }

		public PdxSublist Parent { get; set; }
		private static ReadState State { get; set; }
		private static string ReadKey { get; set; }

		public DateTime GetDate(string key)
		{
			return ParseDate(keyValuePairs[key].Single());

		}

		public static DateTime ParseDate(string dateStr)
		{

			var dateParts = dateStr.Split('.').Select((p) => int.Parse(p)).ToList();
			return new DateTime(dateParts[0], dateParts[1], dateParts[2]);
		}

		public static PdxSublist ReadFile(string filePath, string firstLine = null)
		{
			var rootList = new PdxSublist(null, filePath);
			var currentList = rootList;
			using (var file = new StreamReader(filePath, Encoding.Default))
			{

				if (firstLine != null)
				{
					var line = file.ReadLine();
					if (line != firstLine)
					{
						throw new Exception("Not a valid file");
					}

				}
				char ch;
				State = ReadState.preKey;

				var inQuotes = false;
				var key = new StringBuilder();
				var value = new StringBuilder();
				//var total = new StringBuilder();
				var prevState = State;
				while (!file.EndOfStream)
				{

					ch = Convert.ToChar(file.Read());
					//total.Append(ch);
					if (State == ReadState.comment)
					{
						if (Environment.NewLine.Contains(ch))
						{
							State = prevState;
						}
						continue;
					}
					if (!inQuotes)
					{
						if (ch == '#')
						{
							prevState = State;
							State = ReadState.comment;
							continue;
						}
						if (ch == '{')
						{
							Terminate(currentList, key, value, ch);
							// open sublist

							var sub = new PdxSublist(currentList, State == ReadState.preValue ? key.ToString() : null);
							key = new StringBuilder();
							value = new StringBuilder();
							currentList = sub;
							State = ReadState.preKey;
							continue;
						}
						if (ch == '=')
						{
							// expecting next non-whitespace character to be the start of the key
							State = ReadState.preValue;
							continue;
						}
						if (ch == '}')
						{
							// end of sublist, go up a level
							Terminate(currentList, key, value, ch);

							key = new StringBuilder();
							value = new StringBuilder();
							currentList.Parent.AddSublist(currentList.Key, currentList);
							currentList = currentList.Parent;
							State = ReadState.preKey;
							continue;
						}

						if (char.IsWhiteSpace(ch))
						{
							if (State == ReadState.value)
							{
								//append to list
								currentList.AddValue(key.ToString(), value.ToString());
								key = new StringBuilder();
								value = new StringBuilder();
								State = ReadState.preKey;

							}
							else if (State == ReadState.key)
							{
								// no longer reading the key, expecting = or a new value if list
								State = ReadState.postKey;
							}
							continue;
						}
						else if (State == ReadState.preValue)
						{

							State = ReadState.value;
						}
						else if (State == ReadState.postKey)
						{
							//this must be a list
							currentList.AddValue(string.Empty, key.ToString());
							key = new StringBuilder();
							State = ReadState.key;
						}
						else if (State == ReadState.preKey)
						{
							State = ReadState.key;
						}


					}
					if (ch == '"')
					{
						inQuotes = !inQuotes;
						continue;
					}
					if (State == ReadState.key)
					{
						key.Append(ch);
					}
					if (State == ReadState.value)
					{
						value.Append(ch);
					}
				}
				Terminate(currentList, key, value);
			}
			if (currentList != rootList)
			{
				throw new Exception("An unknown error occurred.");
			}
			return rootList;
		}

		private static void Terminate(PdxSublist currentList, StringBuilder key, StringBuilder value, char? ch = null)
		{
			switch (State)
			{
				//determine what to do about current thing
				case ReadState.key:
				case ReadState.postKey:
					currentList.AddValue(string.Empty, key.ToString());
					break;
				case ReadState.value:
					currentList.AddValue(key.ToString(), value.ToString());
					break;
				case ReadState.preValue:
				case ReadState.preKey:
				case ReadState.comment:
					break;
				default:
					var unexpected = ch.HasValue ? ch.Value.ToString() : "EoF";
					throw new Exception($"Syntax error:  Unexcepted '{unexpected}'");
			}
		}

		public static PdxSublist ReadFileOld(string filePath, string firstLine = null)
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
				if (lineNumber == 1513567)
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
			AddValue(key, $"{date.Year}.{date.Month}.{date.Day}");
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
				currentList.AddValue(key, value);
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
							currentList.AddValue(k, v);
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
				currentList.AddValue(k, v);
			}
		}

		private static void SingleLineArray(string key, string value, PdxSublist currentList)
		{
			var numValues = new List<string>();
			var inQuotes = false;
			var nextVal = new StringBuilder();
			foreach (var ch in value)
			{
				if (ch == '}')
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
				if (ch == '"')
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
				currentList.AddValue(null, val);
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
			normal, key, value, preValue, preKey,
			postKey, comment
		}
	}


	/// <summary>
	/// A wrapper for a dictionary of type T1, List of T2 that will assume there list contains one item when you try to access from it
	/// This exists purely to stop old code from breaking with the new system of lists.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class PseudoDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
	{
		Dictionary<TKey, List<TValue>> dictionary;
		public PseudoDictionary(Dictionary<TKey, List<TValue>> dictionary)
		{
			this.dictionary = dictionary;
		}

		public TValue this[TKey key]
		{
			get { return dictionary[key].First(); }
			set
			{
				dictionary[key] = new List<TValue>();
				dictionary[key].Add(value);
			}
		}

		public Dictionary<TKey, List<TValue>>.KeyCollection Keys { get { return dictionary.Keys; } }

		public bool ContainsKey(TKey key)
		{
			return dictionary.ContainsKey(key);
		}

		public void ForEach(TKey key, Action<TValue> callback)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key].ForEach(callback);
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			foreach (var entry in dictionary)
			{
				foreach (var subEntry in entry.Value)
				{
					yield return new KeyValuePair<TKey, TValue>(entry.Key, subEntry);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}

