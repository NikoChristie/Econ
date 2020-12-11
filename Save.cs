using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Econ {
    public class Save {

		public int width { get; }
		public int height { get; }

		public List<Culture> religion { get; set; } = new List<Culture>();
		public List<Culture> ethnicity { get; set; } = new List<Culture>();
		public List<Country> countries { get; set; } = new List<Country>();
		public Tile[,] map { get; set; }
		public DateTime date { get; set; }

		public World.Week day { get; set; }

		public Save() {
			this.width = World.width;
			this.height = World.height;
			foreach (Culture culture in World.religion) {
				religion.Add(culture);
			}
			foreach (Culture culture in World.ethnicity) {
				ethnicity.Add(culture);
			}
			this.ethnicity = World.ethnicity;
			map = new Tile[width, height];
			for(int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					map[x, y] = World.map[x, y];
				}
			}

			this.date = World.date;
			this.day = World.day;
		}

		public void Load() {
			World.religion = religion;
			foreach (Culture culture in World.religion) {
				religion.Add(culture);
			}
			foreach (Culture culture in World.ethnicity) {
				ethnicity.Add(culture);
			}
			this.ethnicity = World.ethnicity;
			World.map = new Tile[width, height];
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					World.map[x, y] = map[x, y];
				}
			}
			World.date = date;
			World.day = day;
			World.Size(width, height);
		}
	}
}
