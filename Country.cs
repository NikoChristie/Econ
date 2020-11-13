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
		public float minimum_wage = 15.00f;
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
								/*
								if (tradeGrid[x, y] == null) {
									if (World.map[x, y].owner != null) {
										tradeGrid[x, y].cost = 0;
									}
								}
								*/
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

	}
}
