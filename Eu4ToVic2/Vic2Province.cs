using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu4ToVic2
{
	class Vic2Province
	{
		public List<Eu4Province> Eu4Provinces;

		public int ProvID { get; set; }
		public Vic2Country Owner { get; set; }
		public List<Vic2Country> Cores { get; set; }

		public string TradeGoods { get; set; }
		public int LifeRating { get; set; }

		public int FortLevel { get; set; }

		public PopPool Pops { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="provID">ID of the province</param>
		/// <param name="defaultProvince">The province's default history file</param>
		/// <param name="vic2World">The world object the province exists in</param>
		/// <param name="siblingProvinces">The number of other vic2 provinces that eu4Provinces has been mapped to</param>
		/// <param name="eu4Provinces">List of eu4 provinces mapped to this province</param>
		public Vic2Province(int provID, PdxSublist defaultProvince, Vic2World vic2World, int siblingProvinces, List<Eu4Province> eu4Provinces)
		{
			Eu4Provinces = eu4Provinces;
			ProvID = provID;
			TradeGoods = defaultProvince.KeyValuePairs["trade_goods"];
			LifeRating = int.Parse(defaultProvince.KeyValuePairs["life_rating"]);

			// most common owner
			Owner = vic2World.GetCountry(eu4Provinces.GroupBy(p => p.Owner).OrderByDescending(grp => grp.Count())
	  .Select(grp => grp.Key).First());
			// all countries that have cores in any of the eu4 counterparts gets cores here
			Cores = eu4Provinces.SelectMany(p => p.Cores).Select(c => vic2World.GetCountry(c)).ToList();

			FortLevel = eu4Provinces.Any(p => p.FortLevel > 6) ? 1 : 0;

			CalcEffects(vic2World, siblingProvinces, eu4Provinces);
		}

		private void CalcEffects(Vic2World vic2World, int siblingProvinces, List<Eu4Province> eu4Provinces)
		{
			IterateEffects(vic2World, siblingProvinces, eu4Provinces, (effects, fromProvince) =>
			{
				CalcPopEffects(effects, fromProvince);
			});
		}

		private void CalcPopEffects(Dictionary<string, float> effects, Eu4Province fromProvince)
		{
			foreach (PopType popType in Enum.GetValues(typeof(PopType)))
			{
				var name = Enum.GetName(typeof(PopType), popType);
				if (effects.ContainsKey(name))
				{
					Pops.AddPop(fromProvince, popType, (int)effects[name]);
				}
			}
		}

		private void IterateEffects(Vic2World vic2World, int siblingProvinces, List<Eu4Province> eu4Provinces, Action<Dictionary<string, float>, Eu4Province> callback)
		{


			foreach (var prov in eu4Provinces)
			{
				Action<Dictionary<string, float>> newCallback = (Dictionary<string, float> effects) =>
				{
					callback(effects, prov);
				};
				// development
				vic2World.ValueEffect(vic2World.ProvinceEffects, newCallback, "base_tax", prov.BaseTax / (float)siblingProvinces);
				vic2World.ValueEffect(vic2World.ProvinceEffects, newCallback, "base_producion", prov.BaseProduction / (float)siblingProvinces);
				vic2World.ValueEffect(vic2World.ProvinceEffects, newCallback, "base_manpower", prov.BaseManpower / (float)siblingProvinces);

				// estates
				if (prov.Estate != null && vic2World.ProvinceEffects.Sublists["estate"].Sublists.ContainsKey(prov.Estate))
				{
					newCallback(vic2World.ProvinceEffects.Sublists["estate"].Sublists[prov.Estate].KeyValuePairs.ToDictionary(effect => effect.Key, effect => float.Parse(effect.Value) / eu4Provinces.Count));
				}


				// province flags
				if (vic2World.ProvinceEffects.Sublists.ContainsKey("province_flags"))
				{
					foreach (var flag in prov.Flags)
					{
						if (vic2World.ProvinceEffects.Sublists["province_flags"].Sublists.ContainsKey(flag))
						{
							newCallback(vic2World.ProvinceEffects.Sublists["province_flags"].Sublists[flag].KeyValuePairs.ToDictionary(effect => effect.Key, effect => float.Parse(effect.Value) / eu4Provinces.Count));
						}
					}
				}



			}

			// from owner country 
			if (vic2World.ProvinceEffects.Sublists.ContainsKey("owner"))
			{
				Vic2Country.IterateCountryEffects(vic2World, Owner.Eu4Country, vic2World.ProvinceEffects.Sublists["owner"], (effects) => { callback(effects, null); });
			}
		}
	}

	public class PopPool
	{
		private Dictionary<string, Pop> pops;
		private Dictionary<Eu4Province, ValueSet<string>> culture;
		private Dictionary<Eu4Province, ValueSet<string>> religion;
		public List<Pop> Pops
		{
			get
			{
				return pops.Values.ToList();
			}
		}
		public PopPool(List<Eu4Province> eu4Provinces)
		{
			pops = new Dictionary<string, Pop>();
			culture = new Dictionary<Eu4Province, ValueSet<string>>();
			religion = new Dictionary<Eu4Province, ValueSet<string>>();
			foreach (var prov in eu4Provinces)
			{
				culture[prov] = HistoryEffect(prov.CulturalHistory);
				religion[prov] = HistoryEffect(prov.ReligiousHistory);
			}
		}
		// calculate what proportions of the population should be what demographic based on the history of the province
		private ValueSet<string> HistoryEffect(Dictionary<DateTime, string> history)
		{
			var orderedCulturalHistory = history.OrderBy(h => h.Key.Ticks);
			var set = new ValueSet<string>(1f, orderedCulturalHistory.First().Value);

			KeyValuePair<DateTime, string>? lastEntry = null;
			foreach (var entry in orderedCulturalHistory)
			{
				if (lastEntry.HasValue)
				{
					var since = lastEntry.Value.Key - entry.Key;
					// 100 years -> +50%
					set[lastEntry.Value.Value] += (since.Days * 1.369863013698630136986301369863e-5);

				}
				// new culture is set to 50%
				set[entry.Value] = 0.5f;
				lastEntry = entry;
			}
			return set;
		}
		class DemographicEvent
		{
			public string Culture { get; set; }
			public string Religion { get; set; }
			public DemographicEvent()
			{
			}

			public DemographicEvent SetReligion(string religion)
			{
				Religion = religion;
				return this;
			}
			public DemographicEvent SetCulture(string culture)
			{
				Culture = culture;
				return this;
			}
		}
		public void AddPop(Eu4Province fromProvince, PopType type, int quantity)
		{
			var history = new Dictionary<DateTime, DemographicEvent>();
			fromProvince.CulturalHistory.ToList().ForEach(ce => history.Add(ce.Key, new DemographicEvent().SetCulture(ce.Value)));
			fromProvince.ReligiousHistory.ToList().ForEach(re =>
			{
				if (history.ContainsKey(re.Key))
				{
					history[re.Key].SetReligion(re.Value);
				}
				else
				{
					history[re.Key] = new DemographicEvent().SetReligion(re.Value);
				}
			});
			var orderedHistory = history.OrderBy(he => he.Key.Ticks);
			var popsList = new List<Pop>();
			popsList.Add(new Pop(type, quantity, orderedHistory.First().Value.Culture, orderedHistory.First().Value.Religion));
			KeyValuePair<DateTime, DemographicEvent> lastEntry = orderedHistory.First();
			bool firstTime = true;
			var majorityReligion = lastEntry.Value.Religion;
			foreach (var entry in orderedHistory)
			{
				if (firstTime)
				{
					firstTime = false;
					var since = lastEntry.Key - entry.Key;
					// 100 years -> +50%
					//set[lastEntry.Value.Value] += (since.Days * 1.369863013698630136986301369863e-5);
					popsList.AddRange(SplitPops((int)(since.Days * 1.369863013698630136986301369863e-5), popsList, c => lastEntry.Value.Culture, r => lastEntry.Value.Religion));
				}

				if (entry.Value.Religion == null)
				{
					if (entry.Value.Culture == null)
					{
						// something is probably broken here
						Console.WriteLine($"Warning: Province {fromProvince.ProvinceID} has an invalid history entry for {entry.Key.ToShortDateString()}");

					}
					else
					{
						// flip 50% of population to new culture. only true faith will convert culture as in eu4
						popsList.AddRange(SplitPops(1 + quantity / 2, popsList.Where(p => p.Religion == majorityReligion).ToList(), c => entry.Value.Religion, r => r));
					}
				}
				else
				{
					majorityReligion = entry.Value.Religion;
					if (entry.Value.Culture == null)
					{
						// flip 50% of population to new religion
						popsList.AddRange(SplitPops(1 + quantity / 2, popsList, c => c, r => entry.Value.Religion));
					}
					else
					{
						// flip 50% to new religion + culture
						popsList.AddRange(SplitPops(1 + quantity / 2, popsList, c => entry.Value.Religion, r => entry.Value.Religion));
					}
				}

			}
		}
		/// <summary>
		/// Converts of number of people who follow a religion to a new religion and culture, evenly across all pops of the old religion
		/// </summary>
		/// <param name="oldReligion"></param>
		/// <param name="total"></param>
		/// <param name="culture"></param>
		/// <param name="religion"></param>
		private void SplitReligion(string oldReligion, int total, string culture, string religion)
		{
			SplitPops(total, pops.Values.Where(p => p.Religion == oldReligion).ToList(), c => culture, r => religion);
		}
		/// <summary>
		/// Converts a number of people to a new religion and culture, evenly across all pops
		/// </summary>
		/// <param name="total"></param>
		/// <param name="culture"></param>
		/// <param name="religion"></param>
		private void SplitAll(int total, string culture, string religion)
		{
			SplitPops(total, pops.Values.ToList(), c => culture, r => religion).ForEach(AddPop);
		}
		private static List<Pop> SplitPops(int total, List<Pop> popsList, Func<string, string> culture, Func<string, string> religion)
		{
			var eachSplit = total / popsList.Count;
			var splitSoFar = 0;
			var newPops = new List<Pop>();
			for (var i = 0; i < popsList.Count; i++)
			{
				var pop = popsList[i];
				//split off of this pop
				var np = pop.Split(eachSplit, culture(pop.Culture), religion(pop.Religion));
				// determine how many have been split off
				splitSoFar += np.Size;
				// determine how many pops are left to split
				var popsLeft = (popsList.Count - i - 1);

				// determine how much each remaining pop must be split
				if (popsLeft > 0)
				{
					eachSplit = (total - splitSoFar) / popsLeft;
				}
				// prepare new pop to be added
				newPops.Add(np);
			}
			return newPops;
		}
		private void AddPop(Pop pop)
		{
			AddPop(pop.Type, pop.Size, pop.Culture, pop.Religion);
		}
		private void AddPop(PopType type, int quantity, string culture, string religion)
		{
			var key = GetKey(type, culture, religion);
			if (pops.ContainsKey(key))
			{
				pops[key].Add(quantity);
			}
			else
			{
				pops.Add(key, new Pop(type, quantity, culture, religion));
			}
		}

		public void SetCultureRatio(Dictionary<string, float> cultures)
		{
			// TODO
		}
		private string GetKey(PopType type, string culture, string religion)
		{
			return $"{type}/{culture}/{religion}";
		}
	}
	/// <summary>
	/// A Dictionary of T, float that has a constant total value. Any time a value is modified it will adjust other values to make sure the sum is constant.
	///
	/// Values cannot be negative.
	/// </summary>
	internal class ValueSet<T> : IEnumerable<KeyValuePair<T, double>>
	{
		double total;
		Dictionary<T, double> values;
		public ValueSet(double total, T firstKey)
		{
			this.total = total;
			values = new Dictionary<T, double>();
			values[firstKey] = total;
		}

		public double this[T key]
		{
			get
			{
				return values[key];
			}
			set
			{
				if (!values.ContainsKey(key))
				{
					values[key] = 0;
				}
				if (values.Count == 1)
				{
					values[key] = total;
					return;
				}
				// works out how much it has changed - cannot go above total value
				var change = Math.Min(value, total) - values[key];
				// all others will decrease by the amount it is change by, shared between them
				var eachChange = -change / (values.Count - 1);
				foreach (var otherKey in values.Keys.ToList())
				{
					values[otherKey] += eachChange;
				}
				values[key] += change;
			}
		}

		public IEnumerator<KeyValuePair<T, double>> GetEnumerator()
		{
			foreach (var val in values)
			{
				yield return val;
			}

		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public enum PopType
	{
		aristocrats, clergymen, artisans, soldiers, labourers, farmers, slaves, capitalists, bureaucrats, craftsmen, officers
	}
	public class Pop
	{

		public PopType Type { get; set; }
		public int Size { get; set; }
		public string Culture { get; set; }
		public string Religion { get; set; }


		public Pop(PopType type, int size, string culture, string religion)
		{
			Type = type;
			Size = size;
			Culture = culture;
			Religion = religion;
		}
		public void Add(int quantity)
		{
			Size += quantity;
		}

		/// <summary>
		/// Splits off some of this pop into a new pop with a different culture and religion
		/// </summary>
		/// <param name="quantity"></param>
		/// <param name="culture"></param>
		/// <param name="religion"></param>
		/// <returns></returns>
		public Pop Split(int quantity, string culture, string religion)
		{
			var newPop = new Pop(Type, Math.Min(quantity, Size), culture, religion);
			quantity = Math.Max(0, quantity - Size);
			return newPop;
		}

	}

	public class Vic2State
	{
		public List<Factory> Factories { get; set; }
	}

	public class Factory
	{
		public string Name { get; set; }
		public int Level { get; set; }
	}
}
