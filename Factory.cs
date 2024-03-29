﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public class Factory : MarketEntity {

		public Market.products output { get; }
		public Dictionary<Market.products, double> input;
		public World.Jobs job { get; }
		// TODO, add greed factor, which changes amount of surplus value taken by the factory after operation_cost and wages, it will change with the times

		public double complexity { get; set; }

		private float price { get; set; } = 1.00f;

		public float wages { get; set; }

		public int capacity { get; set; }
		public int population { get; set; }

		public Tile tile;

		public Factory(Tile location, Market.products output, Dictionary<Market.products, double> input, double complexity, int capacity, World.Jobs job) {
			this.tile = location;
			this.complexity = complexity;
			this.output = output;
			this.input = input;
			this.capacity = capacity;
			this.population = 0;
			this.job = job;

			this.wages = location.owner.minimum_wage;

			this.pool.Add(this.output, 0);
			if (this.input != null) {
				foreach (KeyValuePair<Market.products, double> i in this.input) {
					this.pool.Add(i.Key, 0);
					this.orders.Add(i.Key, 0);
				}
			}
		}

        public override Tile location() {
			return tile;

		}

        public override float cost(Market.products products) {
			return this.price;
		}

		public void demand() {

			if (this.input != null) {

				double amount = this.location().owner.workhours * (this.population * this.complexity);

				foreach (KeyValuePair<Market.products, double> i in this.input) {

					double amount_needed = (this.input[i.Key] * amount) - (this.pool[i.Key] + this.orders[i.Key]);

					if (amount_needed > 0) { // max_amount - (current_amount + ordered_amount)
						this.location().owner.tradeDemand[i.Key].Add(new Buy(i.Key, amount_needed, this));
						if (this.pool[i.Key] + this.orders[i.Key] > this.input[i.Key] * (this.location().owner.workhours * (this.population * this.complexity))) 
							throw new Exception($"Error: Amount of ({i.Key}) Owned {(this.pool[i.Key] + this.orders[i.Key])}, is greater than amount needed ({this.input[i.Key] * (this.location().owner.workhours * (this.population * this.complexity))})");
					}
					/*
					if (this.pool[i.Key] + this.orders[i.Key] > this.input[i.Key] * (this.location.owner.workhours * (this.population * this.complexity))) { // we have an error caused here because workers change jobs
						// return goods that aren't needed
						//throw new Exception("Error: Amount of (" + i.Key.ToString() + ") Owned " + (this.pool[i.Key] + this.orders[i.Key]) + ", is greater than amount needed (" + this.input[i.Key] * (this.location.owner.workhours * (this.population * this.complexity)) + ")");
					}
					*/

				}
			}
		}

		public void supply() {
			if (this.pool[this.output] > 0) this.location().owner.tradeSupply[this.output].Add(new Sell(this.output, this.pool[this.output], this));
		}

		public void produce() {
			if (this.population == 0) return;

			this.adjust_wages();

			this.operation_cost += this.population * this.wages;
			// # get price
			this.price = (this.population * this.location().owner.workhours) + this.operation_cost; // total_price = (workers * workhours) + spending cost

			// Production Amount
			double production = this.location().owner.workhours * (this.population * this.complexity); // production = max

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

			this.price /= (float)(Math.Max(production, 1)); // price = price / amount produced

			// this.price = this.price > 0.00f ? this.price : 0.01f;

			this.pool[this.output] += production;

			this.wages = Math.Max(this.wages, this.location().owner.minimum_wage);

			this.capital -= operation_cost;

			this.operation_cost = 0.00f; // # reset operation_cost
		}
		
		private void adjust_wages() {

			// Problem, you are your own worst enemey

			if (this.population < this.capacity) { // if factory not at capacity

				if (this.location().unemployment(this.job) > 1.00f) { // surplus of jobs
					// Overbid you closest competitor
					Factory competitor = null;

					foreach (Factory factory in this.location().factories) {
						if (!factory.Equals(this)) {
							if (competitor == null) competitor = factory.wages >= this.wages ? factory : null;
							else {
								if (factory.wages >= this.wages) { // greater wages means they are a competition
									competitor = (factory.wages - this.wages) <= (competitor.wages - this.wages) ? factory : competitor;
								}
							}
						}
					}
					if (competitor != null) {
						this.wages = Math.Max(this.wages, Math.Min(competitor.wages + (float)(competitor.wages * 0.01), (float)(((this.location().owner.workhours * (this.capacity * this.complexity)) * this.price) / this.capacity) - this.operation_cost));
						// new_wages = (value_produces_per_day / full_capacity) - cost
					}
					// else you pay your workers the most, good job!


				}
			}
			else { // factory at capacity
				if (this.location().unemployment(this.job) < 1.00f) { // surplus of workers
					// Stoop to the level of your competitor
					Factory competitor = null;

					foreach (Factory factory in this.location().factories) {
						if (!factory.Equals(this)) { 
							if (competitor == null) competitor = factory.wages <= this.wages ? factory : null;
							else {
								if (factory.wages <= this.wages) { // greater wages means they are a competition
									competitor = (factory.wages - this.wages) >= (competitor.wages - this.wages) ? factory : competitor;
								}
							}
						}
					}
					if (competitor != null) {
						this.wages = Math.Min(this.wages, Math.Min(competitor.wages - (float)(competitor.wages * 0.01), (float)(((this.location().owner.workhours * (this.capacity * this.complexity)) * this.price) / this.capacity) - this.operation_cost));
						// new_wages = (value_produces_per_day / full_capacity) - cost
					}
					// you are one rat bastard, noone pays their workers less


				}

			}

			this.wages = Math.Max(this.wages, this.location().owner.minimum_wage);
			this.wages = (float)Math.Round(this.wages, 2);
		}
	}
}
