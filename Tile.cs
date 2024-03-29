﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Econ {
	public class Tile : MarketEntity {

		public int x { get; }
		public int y { get; }
		public double terrain { get; set; }
		public Country owner { get; set; }

		public Dictionary<Market.products, float> supply = new Dictionary<Market.products, float>();
		public Dictionary<Market.products, float> demand = new Dictionary<Market.products, float>();
		public Dictionary<Market.products, float> price  = new Dictionary<Market.products, float>();

		public Dictionary<Country, Dictionary<Market.products, double>> mod = new Dictionary<Country, Dictionary<Market.products, double>>();
		public List<Factory> factories { get; set; } = new List<Factory>();

		public Pop[,] population { get; } = new Pop[World.religion.Count, World.ethnicity.Count];

		public Tile(int x, int y, double terrain) {

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

			foreach (Culture yy in World.ethnicity) {
				foreach (Culture xx in World.religion) {
					this.population[xx.index, yy.index] = new Pop(xx, yy, this, Program.rand.Next(100));
				}
			}

		}

		public Tile() { 
		
		}

		public Tile(int x, int y, double terrain, Country owner, List<Factory> factories, Pop[,] population) { 
		
		}

		public override Tile location() { 
			return this;
		}

		public override float cost(Market.products products) {
			return this.price[products];
		}

		public void tick() {

			switch (World.day) {
				case World.Week.Monday:
					market_tick();
					break;
				case World.Week.Friday:
					foreach (Factory factory in this.factories) {
						factory.produce();
					}
					break;
				default:
					break;
			}
		}

		private void hire_tick() {

			Dictionary<World.Jobs, int> jobs = new Dictionary<World.Jobs, int>();

			// create sorted list of factories by highest wages
			Dictionary<World.Jobs, List<Factory>> postings = new Dictionary<World.Jobs, List<Factory>>();

			foreach (World.Jobs i in Enum.GetValues(typeof(World.Jobs))) {
				jobs.Add(i, this[null, null, null, null, i]);
				postings.Add(i, new List<Factory>());
			}

			foreach (Factory factory in factories) { // clear factory
				factory.population = 0;
				postings[factory.job].Add(factory);
			}

			foreach (World.Jobs job in Enum.GetValues(typeof(World.Jobs))) {

				for (int i = 0; i < postings[job].Count; i++) {
					if (i < postings[job].Count - 1) {
						if (postings[job][i].wages < postings[job][i + 1].wages) {
							Factory temp = postings[job][i];
							postings[job][i] = postings[job][i + 1];
							postings[job][i + 1] = temp;
						}
					}
				}
				
			}

			foreach (World.Jobs job in Enum.GetValues(typeof(World.Jobs))) {

				foreach (Factory factory in postings[job]) {
					int hires = Math.Min(jobs[factory.job], factory.capacity);
					jobs[factory.job] -= hires;
					factory.population += hires;
					//remove.Add(factory); // remove factory

				}
			}
		}

		public float unemployment(World.Jobs job) { // avaliable jobs / workers
			float total = this[null, null, null, null, job];
			float unemployment = 0;
			foreach(Factory factory in factories) {
				if (factory.job.Equals(job)) {
					unemployment += factory.capacity;
				}

            }
			return total == 0 ? 1.00f : (float)Math.Round((unemployment / total), 2);
		}

		private void market_tick() {

			if (this.factories.Count > 0) hire_tick();

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

		#region Pop

		public int this[Culture religion = null, Culture ethnicity = null, bool? gender = null, int? age = null, World.Jobs? job = null] { // maybe it sthe valeu
			get {
				return this.sum(religion, ethnicity, gender, age, job);
			}
			set {


				if (value < 0) {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.remove(religion, ethnicity, gender, age, job);
					}
				}
				else if (value > 0) {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.append(religion, ethnicity, gender, age, job);
					}
				}
			}
		}

		public int this[Culture religion, Culture ethnicity, int gender, int? age = null, World.Jobs? job = null] {
			get {
				return this.sum(religion, ethnicity, gender != 0, age, job);
			}
			set {


				if (value < 0) {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.remove(religion, ethnicity, gender != 0, age, job);
					}
				}
				else {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.append(religion, ethnicity, gender != 0, age, job);
					}
				}
			}
		}

		private void remove(Culture religion = null, Culture ethnicity = null, bool? gender = null, int? age = null, World.Jobs? job = null) {

			if (religion == null) {

				Dictionary<Culture, int> array = new Dictionary<Culture, int>();

				foreach (Culture y in World.ethnicity) {
					foreach (Culture x in World.religion) {
						array.Add(x, this.population[x.index, y.index][gender, age, job]);
					}
				}

				religion = Program.WeightedRandom<Culture>(array);

			}

			if (ethnicity == null) {

				Dictionary<Culture, int> array = new Dictionary<Culture, int>();

				foreach (Culture y in World.ethnicity) {
					foreach (Culture x in World.religion) {
						array.Add(y, this.population[x.index, y.index][gender, age, job]);
					}
				}

				ethnicity = Program.WeightedRandom<Culture>(array);

			}

			Console.WriteLine("Pop[" + religion + "," + ethnicity + "] : " + this.population[religion.index, ethnicity.index][gender, age, job]);

			this.population[religion.index, ethnicity.index][gender, age, job] = -1;
		}

		private void append(Culture religion = null, Culture ethnicity = null, bool? gender = null, int? age = null, World.Jobs? job = null) {

			if (religion == null) {
				religion = World.religion[Program.rand.Next(World.religion.Count)];
			}

			if (ethnicity == null) {
				ethnicity = World.ethnicity[Program.rand.Next(World.ethnicity.Count)];
			}

			this.population[religion.index, ethnicity.index][gender, age, job] = 1;
		}

		private int sum(Culture religion = null, Culture ethnicity = null, bool? gender = null, int? age = null, World.Jobs? job = null) {

			int sum = 0;

			if (religion == null) { // All Religion
				if (ethnicity == null) { // All Ethnicity
					for (int y = 0; y < World.ethnicity.Count; y++) {
						for (int x = 0; x < World.religion.Count; x++) {
							sum += this.population[x, y][gender, age, job];
						}
					}
					return sum;
				}
				else { // Ethnicity
					for (int i = 0; i < World.ethnicity.Count; i++) {
						sum += this.population[i, ethnicity.index][gender, age, job];
					}
					return sum;
				}
			}
			else { // Religion
				if (ethnicity == null) { // All Ethnicity
					for (int i = 0; i < World.religion.Count; i++) {
						sum += this.population[religion.index, i][gender, age, job];
					}
					return sum;
				}
				else {
					return this.population[religion.index, ethnicity.index][gender, age, job];
				}
			}
		}

		public void reproduce() { // will be later moved to Estate

			Dictionary<Pop, int>[] singles = new Dictionary<Pop, int>[] { new Dictionary<Pop, int>(), new Dictionary<Pop, int>() }; // singles = [gender][[religion, ethnicity], amount]

			foreach (Culture y in World.ethnicity) {
				foreach (Culture x in World.religion) {
					bool gender = false;
					int amount = this.population[x.index, y.index].reproduce(ref gender);
					singles[gender == false ? 0 : 1].Add(this.population[x.index, y.index], amount);

				}
			}

			int counter = 0;

			while (singles[0].Count > 0 && singles[1].Count > 0) {

				counter++;

				Pop mom = Program.WeightedRandom<Pop>(singles[0]);
				Pop dad = Program.WeightedRandom<Pop>(singles[1]);

				singles[0][mom]--; // remove mom
				singles[1][dad]--; // remove dad
				;
				//this[mom.ideas("gender") > dad.ideas("gender") ? mom.religion : dad.religion, mom.ideas("gender") > dad.ideas("gender") ? mom.ethnicity : dad.ethnicity, null, 0]++;
				Pop alpha = mom.ideas("gender") * -1 > dad.ideas("gender") ? mom : dad;
				this[alpha.religion, alpha.ethnicity, Program.rand.Next(2) != 0, 0] = 1; // 38 for some reason, the reason is you are dumb

				foreach (Dictionary<Pop, int> i in singles) {
					foreach (KeyValuePair<Pop, int> j in i) {
						if (j.Value == 0) {
							i.Remove(j.Key);
						}
					}
				}

			}
		}

		public void birthday() { // will be later moved to Estate
			foreach (Pop pop in this.population) {
				pop.birthday();
			}
		}

		#endregion Pop

		public string Serialize() {
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			return JsonConvert.SerializeObject(World.map[this.x, this.y], settings);
		}
	}
}
