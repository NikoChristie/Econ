using System;
using System.Collections.Generic;
using System.Drawing;

namespace Econ {
    public static class World {

		public enum Terrain { Sea, Low, Mid, High, Hill, Mountain };

		public const int width = Program.width;
		public const int height = Program.height;

		public static List<Country> countries = new List<Country>();
        public static Tile[,] map = World.generateMap(width, height);
        public static DateTime date = new DateTime(1, 1, 1);
		
        public enum Week { Monday, Tuesday, Wedsday, Thursday, Friday, Saturday, Sunday };

        public static Week day = Week.Saturday;

        static World() {
			for (int i = 0; i < 10; i++) {
				World.countries.Add(new Country("Country " + i, Color.FromArgb(Program.rand.Next(255), Program.rand.Next(255), Program.rand.Next(255))));
			}
			foreach (Country country in countries) {

				if (country.tiles.Count > 1) {
					country.tiles.Add(map[Program.rand.Next(map.GetUpperBound(0) + 1), Program.rand.Next(map.GetUpperBound(1) + 1)]);
					country.tiles[0].owner = country;
					country.tiles[0].terrain = 1;
				}

				foreach (Country other in countries) {
					if (!other.Equals(country)) {
						country.tradeDeals.Add(new TradeDeal(other, new Market.products[] { Market.products.A, Market.products.B, Market.products.C, Market.products.D, Market.products.E, Market.products.F, Market.products.G }));
					}
				}

				List<Tile> copy = new List<Tile>();
				foreach (Tile tile in country.tiles) {
					copy.Add(tile);
				}
			}

			landDistribute();

			foreach (Country country in countries) {
				if (Program.debug) Console.WriteLine(country.name + " has " + country.tradeDeals.Count + " tradeals(s)");
				foreach (Tile tile in country.tiles) {
					map[tile.x, tile.y].owner = country;

					if (Program.rand.Next(100) < 50) {
						int factories = Program.rand.Next(1, 10);
						for (int i = 0; i < factories; i++) {
							switch (Program.rand.Next(7)) {
								case 0:
									tile.factories.Add(new Factory(tile, Market.products.A, null, 1, 10));
									break;

								case 1:
									tile.factories.Add(new Factory(tile, Market.products.B, null, 1, 10));
									break;

								case 2:
									tile.factories.Add(new Factory(tile, Market.products.C, null, 1, 10));
									break;

								case 3:
									tile.factories.Add(new Factory(tile, Market.products.D, null, 1, 10));
									break;

								case 4:
									tile.factories.Add(new Factory(tile, Market.products.E, new Dictionary<Market.products, double>() { { Market.products.A, 2 }, { Market.products.B, 2 } }, 1, 10));
									break;

								case 5:
									tile.factories.Add(new Factory(tile, Market.products.F, new Dictionary<Market.products, double>() { { Market.products.C, 2 }, { Market.products.D, 2 } }, 1, 10));
									break;
								case 6:
									tile.factories.Add(new Factory(tile, Market.products.G, new Dictionary<Market.products, double>() { { Market.products.E, 2 }, { Market.products.F, 2 } }, 1, 10));
									break;
							}
						}
					}

				}

			}

		}

		public static void tick() {
			World.day = World.day >= World.Week.Sunday ? World.Week.Monday : World.day + 1;

			Market.tick();

		}

		private static Tile[,] generateMap(int width, int height) {
			Tile[,] grid = new Tile[width, height];
			for (int yy = 0; yy < height; yy++) {
				for (int xx = 0; xx < width; xx++) {
					grid[xx, yy] = new Tile(xx, yy, 0);
				}
			}

			int x = width / 2;
			int y = height / 2;

			for (int i = width * height; i > 0; i--) {
				x += Program.rand.Next(3) - 1;
				y += Program.rand.Next(3) - 1;

				x = x >= width ? width - 1 : x;
				x = x < 0 ? 0 : x;

				y = y >= height ? height - 1 : y;
				y = y < 0 ? 0 : y;

				grid[x, y].terrain = Program.rand.NextDouble();
			}

			return grid;
		}
		/*
		 private static Tile[,] generateMap(int width, int height) {

			Tile[,] grid = new Tile[width, height];

			int x = width / 2;
			int y = height / 2;

			for (int i = 0; i < (width * height); i++) { // main map build loop
				if (grid[x, y] == null) {
					grid[x, y] = new Province(x, y);
				}
				grid[x, y].terrain += Program.rand.NextDouble();

				x += Program.rand.Next(3) - 1;
				y += Program.rand.Next(3) - 1;

				x = x >= width ? width - 1 : x < 0 ? 0 : x;
				y = y >= height ? height - 1 : y < 0 ? 0 : y;

			}

			for (y = 0; y < height; y++) { // create sea tiles
				for (x = 0; x < width; x++) {
					if (grid[x, y] == null) {
						grid[x, y] = new Tile(x, y);
						grid[x, y].type = Tile.Terrain.Sea;
					}
				}
			}

			foreach (Tile tile in grid) {
				if (tile.terrain <= 1) tile.type = Tile.Terrain.Low;
				else if (tile.terrain <= 2) tile.type = Tile.Terrain.Mid;
				else if (tile.terrain <= 3) tile.type = Tile.Terrain.High;
				else if (tile.terrain <= 4) tile.type = Tile.Terrain.Hill;
				else tile.type = Tile.Terrain.Mountain;
			}

			foreach (Tile tile in grid) {
				if (tile.type == Tile.Terrain.Sea) {
					for (y = -1; y < 2; y += 2) {
						for (x = -1; x < 2; x += 2) {

							try {
								if (grid[tile.x + x, tile.y + y].GetType() == typeof(Province)) {
									Province province = (Province)grid[tile.x + x, tile.y + y]; // cast to province
									province.costal = true; // make costal province
								}
							}
							catch (System.IndexOutOfRangeException) { }

						}
					}
				}
			}
			return grid;
		}
		 
		 */
		private static void landDistribute(bool debug = false/*Program.debug*/) {
			List<Tile> tiles = new List<Tile>();

			List<List<Tile>> open = new List<List<Tile>>();
			List<Tile> closed = new List<Tile>();

			foreach (Country country in countries) {
				open.Add(new List<Tile>());
			}

			foreach (Tile tile in World.map) {
				if (tile.terrain > 0) {
					tiles.Add(tile);
				}
			}

			for (int i = 0; i < countries.Count; i++) {
				int index = Program.rand.Next(tiles.Count);
				open[i].Add(tiles[index]);
				closed.Add(tiles[index]);
				tiles.RemoveAt(index);
			}

			while (true) {

				bool end = true;
				foreach (List<Tile> list in open) {
					if (list.Count > 0) {
						end = false;
					}
				}

				if (end == true) {
					break;
				}

				if (debug) {
					Console.WriteLine("\n");
					Console.ForegroundColor = ConsoleColor.Red;
					foreach (Tile tile in closed) {
						Console.Write("[" + tile.x + "," + tile.y + "]");
					}
					Console.WriteLine("\n");
					Console.ResetColor();
				}
				for (int i = 0; i < countries.Count; i++) {

					if (debug) Console.Write("\n" + countries[i].name + ": ");
					if (open[i].Count > 0) {
						if (debug) {
							Console.ForegroundColor = ConsoleColor.Green;
							foreach (Tile tile in open[i]) {
								Console.Write("[" + tile.x + "," + tile.y + "] ");
							}
							Console.ResetColor();
						}
						countries[i].tiles.Add(open[i][0]);

						for (int j = 0; j < 8; j++) { // Open Neighbors

							int x = open[i][0].x;
							int y = open[i][0].y;

							switch (j) {
								case 0:
									y++;
									break;
								case 1:
									y--;
									break;
								case 2:
									x++;
									break;
								case 3:
									x--;
									break;
								case 4:
									y++;
									x--;
									break;
								case 5:
									y++;
									x++;
									break;
								case 6:
									y--;
									x--;
									break;
								case 7:
									y--;
									x++;
									break;
							}

							x = x > map.GetUpperBound(0) ? map.GetUpperBound(0) : x;
							x = x < 0 ? 0 : x;

							y = y > map.GetUpperBound(1) ? map.GetUpperBound(1) : y;
							y = y < 0 ? 0 : y;



							bool ok = true;

							for (int k = 0; k < closed.Count; k++) { // In CLosed List -> Doesn't Work
								if (x == closed[k].x && y == closed[k].y) {
									ok = false;
									break;
								}
							}

							if (ok == true) {
								if (map[x, y].terrain > 0) {
									closed.Add(map[x, y]);
									open[i].Add(map[x, y]);
								}
								else {
									closed.Add(map[x, y]);
								}
							}




						}
						//closed.Add(open[i][0]);
						open[i].Remove(open[i][0]);
					}
				}
			}

			foreach (Country country in World.countries) {
				foreach (Tile tile in country.tiles) {
					tile.owner = country;
				}
			}
		}

	}
}
