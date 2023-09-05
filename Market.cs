using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public static class Market {

		public enum products { A, B, C, D, E, F, G };
		private static Dictionary<products, List<float>> history = new Dictionary<products, List<float>>(); // product / price, updates each week

		static Market() {
			foreach (Market.products i in Enum.GetValues(typeof(Market.products))) {
				history.Add(i, new List<float>());
			}
		}

		public static void tick() {

			foreach (Country country in World.countries) {
				country.tick();
			}
			if (World.day == World.Week.Monday) {
				Market.trade_tick();
			}
		}

		private static void trade_tick() { // problem coulb be that no-one can find a pair, their is TODO: Fix

			Dictionary<products, float[]> list = new Dictionary<products, float[]>();
			foreach (Market.products i in Enum.GetValues(typeof(Market.products))) {
				list.Add(i, new float[] { 0, 0});
			}

			// Update Info
			foreach (Country country in World.countries) {
				foreach (Tile tile in country.tiles) {
					foreach (Factory factory in tile.factories) {
						list[factory.output][0] += (float)(factory.cost(factory.output) * factory.pool[factory.output]);
						list[factory.output][1] += (float)factory.pool[factory.output];
					}
				}
			}

			foreach (KeyValuePair<products, List<float>> i in history) {
				if(list[i.Key][1] > 0) history[i.Key].Add(list[i.Key][0] / list[i.Key][1]);
			}

			for (bool end = false; !end;) {
				foreach (Country country in World.countries) {
					foreach (KeyValuePair<Market.products, List<Buy>> trade_list in country.tradeDemand) {

						List<Sell> supply_remove = new List<Sell>();
						List<Buy> demand_remove = new List<Buy>();

						end = true;

						foreach (Buy trade_demand in country.tradeDemand[trade_list.Key]) {
							Sell supply_match = (Sell)trade_demand.partner();
							if (supply_match != null) {

								end = false; // end = false until all trades.partners == null

								// if your partner's partner is you
								if (supply_match.partner().Equals(trade_demand)) { // partners still exist

									double amount = Math.Min(supply_match.amount, trade_demand.amount);

									//supply_match.amount = amount;

									if (supply_match.amount - amount <= 0) {
										supply_remove.Add(supply_match);
									}
									else {
										supply_match.amount -= amount;
									}

									if (trade_demand.amount - amount <= 0) {
										demand_remove.Add(trade_demand);
									}

									supply_match.target.location().owner.traders.Add(new Trader(trade_demand.target, supply_match)); // add trader
								}
							}
						}

						// Remove Spent Trade Deals, C# funny buisness

						foreach (Sell sell in supply_remove) {
							sell.target.location().owner.tradeSupply[sell.product].Remove(sell);
						}

						foreach (Buy buy in demand_remove) {
							buy.target.location().owner.tradeDemand[buy.product].Remove(buy);
						}
					}

				}
			}

			//info();

		}

		public static void info() {
			Console.Clear();
			foreach (KeyValuePair<products, List<float>> i in history) {
				Console.Write(i.Key.ToString() + " ");
				// ↑↓→
				if (history[i.Key].Count > 1) {
					if (history[i.Key][history[i.Key].Count - 1] == history[i.Key][history[i.Key].Count - 2]) {
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.Write('=');
					}
					else if (history[i.Key][history[i.Key].Count - 1] > history[i.Key][history[i.Key].Count - 2]) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.Write('-');
					}
					else {
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write('+');
					}
					Console.ResetColor();
				}
				Console.Write(" : ");

				Console.ForegroundColor = ConsoleColor.DarkGreen;
				if (history[i.Key].Count > 0) {

					List<float> prices = history[i.Key].GetRange(Math.Max(history[i.Key].Count - 10, 0), Math.Min(10, history[i.Key].Count - 1));
					if (prices.Count > 0) {
						for (int j = 0; j < prices.Count; j++) {

							if (j > 0) {
								if (prices[j] == prices[j - 1]) {
									if (j < prices.Count - 1) Console.ForegroundColor = ConsoleColor.DarkYellow;
									else Console.ForegroundColor = ConsoleColor.Yellow;
								}
								else if (prices[j] > prices[j - 1]) {
									if (j < prices.Count - 1) Console.ForegroundColor = ConsoleColor.DarkGreen;
									else Console.ForegroundColor = ConsoleColor.Green;
								}
								else {
									if (j < prices.Count - 1) Console.ForegroundColor = ConsoleColor.DarkRed;
									else Console.ForegroundColor = ConsoleColor.Red;
								}
							}

							Console.Write($"{Math.Round(prices[j], 2)}$ ");
						}
						Console.ResetColor();
						Console.ForegroundColor = Program.ComparisonColor(prices[prices.Count - 1], prices.Sum() / prices.Count);
						Console.Write(" avrg: " + (Double.IsNaN(Math.Round(prices.Sum() / prices.Count, 2)) ? '~' : Math.Round(prices.Sum() / prices.Count, 2)) + "$");
					}
				}
				else {
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write("- ");
				}
				Console.WriteLine();
				Console.ResetColor();
			}
		}
	}
}
