using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public class Country {
		public string name;
		public List<Tile> tiles = new List<Tile>();
		public List<TradeDeal> tradeDeals = new List<TradeDeal>();
		public List<Trader> traders = new List<Trader>();
		public readonly Color color;
		public int workhours;// = 40; // ! workhours a week
		public float minimum_wage = (float)Program.rand.Next(1, 15);
		public Pathfind.node[,] tradeGrid;

		public Dictionary<Market.products, List<Sell>> tradeSupply = new Dictionary<Market.products, List<Sell>>();
		public Dictionary<Market.products, List<Buy>> tradeDemand = new Dictionary<Market.products, List<Buy>>();

		public Country(string name, Color color) {
			this.name = name;
			this.color = color;
			this.workhours = Program.rand.Next(20, 84);

			foreach (Market.products i in Enum.GetValues(typeof(Market.products))) {
				this.tradeDemand.Add(i, new List<Buy>());
				this.tradeSupply.Add(i, new List<Sell>());
			}

			// Get Trade Grid
			tradeGrid = new Pathfind.node[World.map.GetUpperBound(1) + 1, World.map.GetUpperBound(0) + 1];

			int width = World.map.GetUpperBound(0) + 1;
			int heigth = World.map.GetUpperBound(1) + 1;

			for (int y = 0; y < heigth; y++) {
				for (int x = 0; x < width; x++) {
					if (World.map[x, y].owner != null) {
						if (World.map[x, y].owner.Equals(this)) {
							tradeGrid[x, y] = new Pathfind.node(x, y, 1);
						}
						else {

							foreach (TradeDeal tradeDeal in this.tradeDeals) {
								if (World.map[x, y].owner != null) {

									if (World.map[x, y].owner.Equals(tradeDeal.recipient)) {
										tradeGrid[x, y] = new Pathfind.node(x, y, World.map[x, y].terrain);
									}
								}
							}
							if (tradeGrid[x, y] == null) {
								if (World.map[x, y].owner != null) {
									tradeGrid[x, y] = new Pathfind.node(x, y, 0);
								}
							}
						}
					}
					else {
						if (World.map[x, y].terrain > 0) {
							tradeGrid[x, y] = new Pathfind.node(x, y, World.map[x, y].terrain);
						}
						else {
							tradeGrid[x, y] = new Pathfind.node(x, y, 0);
						}

					}
				}
			}
		}

		public override string ToString() {
			return this.name;
		}

		public void tick() {

			if (World.day == World.Week.Tuesday) {
				foreach (Market.products i in Enum.GetValues(typeof(Market.products))) {
					this.tradeDemand[i].Clear();
					this.tradeSupply[i].Clear();
				}
			}
			else if (World.day == World.Week.Saturday) {
				updateTradeGrid();
			}

			foreach (Tile tile in this.tiles) {
				tile.tick();
			}

			// prevent error with trader removing self
			List<Trader> copy = new List<Trader>();
			foreach (Trader i in this.traders) {
				copy.Add(i);
			}

			foreach (Trader trader in copy) {
				trader.tick();
			}

		}

		private void updateTradeGrid() {

			for (int y = 0; y < World.height; y++) {
				for (int x = 0; x < World.width; x++) {

					while (true) {
						if (World.map[x, y].owner != null) {
							if (World.map[x, y].owner.Equals(this)) {
								tradeGrid[x, y].cost = World.map[x, y].terrain;
								break;

							}
							else {
								bool end = false;
								foreach (TradeDeal tradeDeal in this.tradeDeals) {
									// removed World.map[x, y].owner != null, don't think I need it
									if (World.map[x, y].owner.Equals(tradeDeal.recipient)) {
										tradeGrid[x, y].cost = World.map[x, y].terrain;
										end = true;
										break;
									}
									
								}
								if (end) break;
							}
						}
						else {
							if (World.map[x, y].terrain > 0) {
								tradeGrid[x, y].cost = World.map[x, y].terrain; // no-mans land
								break;
							}
							else {
								tradeGrid[x, y].cost = 0; // ocean
								break;
							}

						}
					}
				}
			}
		}

		#region Pop

		public int this[List<Tile> tiles = null, Culture religion = null, Culture ethnicity = null, bool? gender = null, int? age = null, World.Jobs? job = null] {
			get {
				return this.sum(tiles == null ? this.tiles : tiles, religion, ethnicity, gender, age, job);
			}
			set {
				if (value < 0) {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.remove(tiles == null ? this.tiles : tiles, religion, ethnicity, gender, age, job);
					}
				}
				else {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.append(tiles == null ? this.tiles : tiles, religion, ethnicity, gender, age, job);
					}
				}
			}
		}

		public int this[List<Tile> tiles = null, Culture religion = null, Culture ethnicity = null, int? gender = null, int? age = null, World.Jobs? job = null] {
			get {
				return this.sum(tiles == null ? this.tiles : tiles, religion, ethnicity, gender != 0, age, job);
			}
			set {
				if (value < 0) {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.remove(tiles == null ? this.tiles : tiles, religion, ethnicity, gender != 0, age, job);
					}
				}
				else {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.append(tiles == null ? this.tiles : tiles, religion, ethnicity, gender != 0, age, job);
					}
				}
			}
		}

		public int this[List<Group> groups] {
			get {
				int sum = 0;

				foreach (Group group in groups) {
					sum += this[group];
				}

				return sum;
			}
			set {
				foreach (Group group in groups) { // ! fix
					if (value < 0) {
						for (int i = 0; i < Math.Abs(value); i++) {
							this.remove(new List<Tile>() { group.tile }, group.religion, group.ethnicity, group.gender, group.age, group.job);
						}
					}
					else {
						for (int i = 0; i < Math.Abs(value); i++) {
							this.append(new List<Tile>() { group.tile }, group.religion, group.ethnicity, group.gender, group.age, group.job);
						}
					}
				}
			}
		}

		public int this[Group group] {
			get {

				return this.sum(new List<Tile>() { group.tile }, group.religion, group.ethnicity, group.gender, group.age, group.job);

			}
			set {

				if (value < 0) {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.remove(new List<Tile>() { group.tile }, group.religion, group.ethnicity, group.gender, group.age, group.job);
					}
				}
				else {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.append(new List<Tile>() { group.tile }, group.religion, group.ethnicity, group.gender, group.age, group.job);
					}
				}

			}
		}

		private void remove(List<Tile> tile, Culture religion = null, Culture ethnicity = null, bool? gender = null, int? age = null, World.Jobs? job = null) {


			Dictionary<Tile, int> array = new Dictionary<Tile, int>();
			foreach (Tile i in tile) {
				array.Add(i, i[religion, ethnicity, gender, age, job]);
			}

			Program.WeightedRandom<Tile>(array)[religion, ethnicity, gender, age, job] = -1;

		}

		private void append(List<Tile> tile, Culture religion = null, Culture ethnicity = null, bool? gender = null, int? age = null, World.Jobs? job = null) {
			List<Tile> list = new List<Tile>();

			foreach (Tile i in tile) {
				for (int j = 0; j < i[religion, ethnicity, gender, age, job]; j++) {
					list.Add(i);
				}
			}

			list[Program.rand.Next(list.Count)][religion, ethnicity, gender, age, job] = 1;
		}

		public void birthday() {
			foreach (Tile i in this.tiles) {
				i.birthday();
			}
		}

		public void reproduce() {
			foreach (Tile i in this.tiles) {
				i.reproduce();
			}
		}

		private int sum(List<Tile> tile, Culture religion = null, Culture ethnicity = null, bool? gender = null, int? age = null, World.Jobs? job = null) {
			int sum = 0;

			foreach (Tile i in tile) {
				sum += i[religion, ethnicity, gender, age, job];
			}

			return sum;
		}

		#endregion Pop

	}
}
