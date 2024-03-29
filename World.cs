﻿using System;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace Econ {
    public static class World {

		public enum Terrain { Sea, Low, Mid, High, Hill, Mountain };
		public enum Jobs { A, B, C, D, E, F, G };

		public static int width { get; private set; } = Program.width;
		public static int height { get; private set; } = Program.height;

		public static List<Culture> religion { get; set; } = new List<Culture>();
		public static List<Culture> ethnicity { get; set; } = new List<Culture>();
		public static List<Country> countries { get; set; } = new List<Country>();
		public static Tile[,] map { get; set; } // = World.generateMap(width, height);
        public static DateTime date { get; set; } = new DateTime(1, 1, 1);
		
        public enum Week { Monday, Tuesday, Wedsday, Thursday, Friday, Saturday, Sunday };

        public static Week day { get; set; } = Week.Saturday;

        static World() {

			for (int i = 0; i < 2; i++) { // religion innit
				religion.Add(new Culture(i, "religion " + i));
				ethnicity.Add(new Culture(i, "ethnicity " + i));
			}

			map = World.generateMap(width, height);

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

			// Factories
			foreach (Country country in countries) {
				if (Program.debug) Console.WriteLine(country.name + " has " + country.tradeDeals.Count + " tradeals(s)");
				foreach (Tile tile in country.tiles) {
					map[tile.x, tile.y].owner = country;

					if (Program.rand.Next(100) < 50) {
						int factories = Program.rand.Next(1, 10);
						for (int i = 0; i < factories; i++) {
							switch (Program.rand.Next(7)) {
								case 0:
									tile.factories.Add(new Factory(tile, Market.products.A, null, 1, 10, Jobs.A));
									break;

								case 1:
									tile.factories.Add(new Factory(tile, Market.products.B, null, 1, 10, Jobs.B));
									break;

								case 2:
									tile.factories.Add(new Factory(tile, Market.products.C, null, 1, 10, Jobs.C));
									break;

								case 3:
									tile.factories.Add(new Factory(tile, Market.products.D, null, 1, 10, Jobs.D));
									break;

								case 4:
									tile.factories.Add(new Factory(tile, Market.products.E, new Dictionary<Market.products, double>() { { Market.products.A, 2 }, { Market.products.B, 2 } }, 1, 10, Jobs.E));
									break;

								case 5:
									tile.factories.Add(new Factory(tile, Market.products.F, new Dictionary<Market.products, double>() { { Market.products.C, 2 }, { Market.products.D, 2 } }, 1, 10, Jobs.F));
									break;
								case 6:
									tile.factories.Add(new Factory(tile, Market.products.G, new Dictionary<Market.products, double>() { { Market.products.E, 2 }, { Market.products.F, 2 } }, 1, 10, Jobs.G));
									break;
							}
						}
					}

				}

			}

			// sum of coutnry tiles
			int counter = 0;
			foreach (Country country in countries) {
				counter += country.tiles.Count;
			}
			Console.WriteLine($"Total Tiles: {counter} / 2702");
			//Console.WriteLine(map[0, 0].Serialize());
			//Thread.Sleep(5000);

		}

		public static void Size(int width, int height) {
			World.width = width;
			World.height = height;
		}
		public static void tick() {
			//World.date = World.date.AddDays(1);
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
		/*
		private static Tile[,] generateMap(string path) {
			if (path.EndsWith(".json")) {
				Tile[,] map = new Tile[width, height];
				countries = JsonSerializer<List<Country>>.Deserialize(path);
				return map;
			}
			else throw new FileNotFoundException();
			
		}
		*/
		private static Tile[,] generateMapVicky2() {

			Tile[,] grid = new Tile[width, height];

			int x = width / 2;
			int y = height / 2;

			//int vickyProvinceCounter = 0;

			for (int i = 0; i < 2702;) { // main map build loop (amount of provinces in vicky 2)

				Console.WriteLine(i + " / 2702");

				if (grid[x, y] == null) {
					grid[x, y] = new Tile(x, y, 0);
					i++;
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
						grid[x, y] = new Tile(x, y, 0);
						//grid[x, y].type = Tile.Terrain.Sea;
					}
				}
			}
			/*
			foreach (Tile tile in grid) {
				if (tile.terrain <= 1) tile.type = Tile.Terrain.Low;
				else if (tile.terrain <= 2) tile.type = Tile.Terrain.Mid;
				else if (tile.terrain <= 3) tile.type = Tile.Terrain.High;
				else if (tile.terrain <= 4) tile.type = Tile.Terrain.Hill;
				else tile.type = Tile.Terrain.Mountain;
			}
			
			foreach (Tile tile in grid) {
				if (tile.terrain == 0) {
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
			*/
			return grid;
		}

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

		public static void Save() {
			//JsonSerializerSettings settings = new JsonSerializerSettings();

			JsonSerializerSettings settings = new JsonSerializerSettings {
				PreserveReferencesHandling = PreserveReferencesHandling.Objects,
				Formatting = Formatting.Indented
			};
			try {
				StreamWriter sw = File.CreateText("save.json");
				sw.Write(JsonConvert.SerializeObject(new Save(), settings));
			}
			catch (System.IO.IOException) { }
		}

	}
}
