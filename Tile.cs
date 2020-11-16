using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public class Tile : MarketEntity {

		public readonly int x;
		public readonly int y;
		public double terrain;
		public Country owner;

		public Dictionary<Market.products, float> supply = new Dictionary<Market.products, float>();
		public Dictionary<Market.products, float> demand = new Dictionary<Market.products, float>();
		public Dictionary<Market.products, float> price = new Dictionary<Market.products, float>();

		public Dictionary<Country, Dictionary<Market.products, double>> mod = new Dictionary<Country, Dictionary<Market.products, double>>();
		public List<Factory> factories = new List<Factory>();

		public Tile(int x, int y, double terrain) {

			this.location = this;

			this.x = x;
			this.y = y;
			this.terrain = terrain;

			// dict setup
			foreach (Market.products i in Enum.GetValues(typeof(Market.products))) {
				this.pool.Add(i, 0);
				this.supply.Add(i, 0);
				this.demand.Add(i, 0);
				this.price.Add(i, 1.00f);
			}

			// mod setup
			foreach (Country country in World.countries) {
				this.mod.Add(country, new Dictionary<Market.products, double>());
				foreach (Market.products i in Enum.GetValues(typeof(Market.products))) {
					this.mod[country].Add(i, 1);
				}
			}

		}

		public override float cost(Market.products products) {
			return this.price[products];
		}

		public void tick() {
			if (World.day == World.Week.Monday) {

				this.market_tick();

			}
			else if (World.day == World.Week.Friday) {
				foreach (Factory factory in this.factories) {
					factory.produce();
				}
			}
		}

		private void hire_tick() {

			Dictionary<World.Jobs, int> jobs = new Dictionary<World.Jobs, int>();

			foreach (World.Jobs i in Enum.GetValues(typeof(World.Jobs))) {
				jobs.Add(i, 0/* this.population[i] */);
			}

			// create sorted list of factories by highest wages
			List<Factory> postings = new List<Factory>();
			foreach (Factory factory in this.factories) {
				for (int i = 0; i < postings.Count; i++) {
					if (factory.wages >= factories[i].wages) { // find factory with wages more than him
						postings.Insert(i, factory);
					}
					else if (i == postings.Count - 1) { // otherwise, pay your workers more you clown
						postings.Add(factory);
					}
				}
			}
			
			while (postings.Count > 0) { // give pops jobs, while jobs exist

				List<Factory> remove = new List<Factory>(); // C# forces my hand here

				foreach (Factory factory in postings) {
					//int hires = Math.Min(jobs[factory.jobs], factory.population);
					//jobs[factory.job] -= hires;
					//factory.population += hires;
					remove.Add(factory); // remove factory

				}

				// Remove full factories - C# forces my hand here
				foreach (Factory i in remove) {
					postings.Remove(i);
				}
				
			}
		}

		private void market_tick() {

			foreach (Factory factory in this.factories) {
				factory.supply();
				factory.demand();
			}

			foreach (KeyValuePair<Market.products, double> i in this.pool) {
				double amount = this.supply[i.Key] - this.demand[i.Key];
				if (Math.Sign(amount) > 0) {
					this.owner.tradeSupply[i.Key].Add(new Sell(i.Key, amount, this));
				}
				else if (Math.Sign(amount) < 0) {
					this.owner.tradeDemand[i.Key].Add(new Buy(i.Key, Math.Abs(amount), this));
				}
			}
		}

		public Pathfind.node toNode() {
			return new Pathfind.node(this.x, this.y, this.terrain);
		}
	}
}
