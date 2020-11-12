using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public class Factory : MarketEntity {

		public readonly Market.products output;
		public Dictionary<Market.products, double> input;

		public double complexity;

		private float price = 1.00f;

		public float wages;

		public int capacity;
		public int population;

		public Factory(Tile location, Market.products output, Dictionary<Market.products, double> input, double complexity, int capacity) {
			this.complexity = complexity;
			this.location = location;
			this.output = output;
			this.input = input;
			this.capacity = capacity;
			this.population = capacity; // ! TODO: fix later

			this.pool.Add(this.output, 0);
			if (this.input != null) {
				foreach (KeyValuePair<Market.products, double> i in this.input) {
					this.pool.Add(i.Key, 0);
					this.orders.Add(i.Key, 0);
				}
			}
		}

		public override float cost(Market.products products) {
			return this.price;
		}

		public void demand() {

			if (this.input != null) {

				double amount = this.location.owner.workhours * (this.population * this.complexity);

				foreach (KeyValuePair<Market.products, double> i in this.input) {

					double amount_needed = (this.input[i.Key] * amount) - (this.pool[i.Key] + this.orders[i.Key]);

					if (amount_needed > 0) { // max_amount - (current_amount + ordered_amount)
						this.location.owner.tradeDemand[i.Key].Add(new Buy(i.Key, amount_needed, this));
					}

					if (this.pool[i.Key] + this.orders[i.Key] > this.input[i.Key] * (this.location.owner.workhours * (this.population * this.complexity))) {
						throw new Exception("Error: Amount of (" + i.Key.ToString() + ") Owned " + (this.pool[i.Key] + this.orders[i.Key]) + ", is greater than amount needed (" + this.input[i.Key] * (this.location.owner.workhours * (this.population * this.complexity)) + ")");
					}

				}
			}
		}

		public void supply() {
			if (this.pool[this.output] > 0) this.location.owner.tradeSupply[this.output].Add(new Sell(this.output, this.pool[this.output], this));
		}

		public void produce() {

			// # get price
			this.price += (this.population * this.location.owner.workhours) + this.operation_cost; // price = (workers * workhours) + spending cost

			// Production Amount
			double production = this.location.owner.workhours * (this.population * this.complexity); // production = max

			if (this.input != null) {

				foreach (KeyValuePair<Market.products, double> i in this.input) { // find limiting mat

					if (this.pool[i.Key] / this.input[i.Key] < production) { // adjust production cap (lower)
						production = this.pool[i.Key] / this.input[i.Key] < production ? this.pool[i.Key] / this.input[i.Key] : production; // is smaller?
					}
				}
			}

			if (this.input != null) {
				foreach (KeyValuePair<Market.products, double> i in this.input) { // find limiting mat

					this.pool[i.Key] -= production * this.input[i.Key];
				}
			}

			this.price /= (float)(production > 0 ? production : 1); // price = price / amount produced

			// this.price = this.price > 0.00f ? this.price : 0.01f;

			this.pool[this.output] += production;

			this.operation_cost = 0.00f; // # reset operation_cost
		}
	}
}
