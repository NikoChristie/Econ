using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public static class Market {

		private static Task<bool> trade;
		public enum products { A, B, C, D, E, F, G };

		public static void tick() {

			foreach (Country country in World.countries) {
				country.tick();
			}
			if (World.day == World.Week.Monday) {
				Market.trade_tick();
			}
		}

		private static void trade_tick() { // problem coulb be that no-one can find a pair, their is TODO: Fix

			bool end = false;

			while (!end) {
				foreach (Country country in World.countries) {
					foreach (KeyValuePair<Market.products, List<Buy>> trade_list in country.tradeDemand) {

						List<Sell> supply_remove = new List<Sell>();
						List<Buy> demand_remove = new List<Buy>();

						end = true;

						foreach (Buy trade_demand in country.tradeDemand[trade_list.Key]) { // TODO: add case for nulls
							Sell supply_match = (Sell)trade_demand.partner();
							if (supply_match != null) {

								end = false; // end = false until all trades.partners == null

								if (supply_match.partner().Equals(trade_demand)) {

									double amount = trade_demand.amount > supply_match.amount ? supply_match.amount : trade_demand.amount;

									supply_match.amount = amount;

									supply_match.target.location.owner.traders.Add(new Trader(trade_demand.target, supply_match));

									if (supply_match.amount - amount <= 0) {
										supply_remove.Add(supply_match);
									}
									else {
										supply_match.amount -= amount;
									}

									if (trade_demand.amount - amount <= 0) {
										demand_remove.Add(trade_demand);
									}

								}
							}
						}

						// Remove Spent Trade Deals

						foreach (Sell sell in supply_remove) {
							sell.target.location.owner.tradeSupply[sell.product].Remove(sell);
						}

						foreach (Buy buy in demand_remove) {
							buy.target.location.owner.tradeDemand[buy.product].Remove(buy);
						}
					}

				}
			}

		}


	}
}
