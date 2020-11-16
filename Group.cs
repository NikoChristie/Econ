using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public class Group {
		// TODO add tile list instead of tile, more effiectne this way
		public readonly Country owner;
		public readonly Culture religion;
		public readonly Culture ethnicity;
		public readonly Tile tile;
		public readonly bool gender;
		public readonly int age;
		public readonly World.Jobs job;

		public static List<Group> Groups(Country country, List<Tile> tile = null, List<Culture> religion = null, List<Culture> ethnicity = null, List<bool> gender = null, List<int> age = null, List<World.Jobs> job = null) {
			List<Group> groups = new List<Group>();

			tile = tile == null ? tile = country.tiles : tile;
			religion = religion == null ? World.religion : religion;
			ethnicity = ethnicity == null ? World.ethnicity : ethnicity;
			gender = gender == null ? new List<bool>() { false, true } : gender;
			age = age == null ? new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 } : age;

			if (job == null) {
				job = new List<World.Jobs>();
				foreach (World.Jobs jobs in Enum.GetValues(typeof(World.Jobs))) {
					job.Add(jobs);
				}
			}

			foreach (Tile t in tile) {
				foreach (Culture r in religion) {
					foreach (Culture e in ethnicity) {
						foreach (bool g in gender) {
							foreach (int a in age) {
								foreach (World.Jobs j in job) {
									groups.Add(new Group(country, r, e, t, g, a, j));
								}
							}
						}
					}
				}
			}

			return groups;
		}

		public override bool Equals(object obj) {
			Group other = (Group)obj;
			return this.owner == other.owner && this.religion == other.religion && this.ethnicity == other.ethnicity && this.tile == other.tile && this.gender == other.gender && this.age == other.age && this.job == other.job;
		}

		#region Constructors
		public Group(Country country, Culture religion, Culture ethnicity, Tile tile, bool gender, int age, World.Jobs job) {
			this.owner = country;
			this.religion = religion;
			this.ethnicity = ethnicity;
			this.tile = tile;
			this.gender = gender;
			this.age = age;
			this.job = job;
		}

		public Group(Country country, Culture religion, Culture ethnicity, Tile tile, int gender, int age, World.Jobs job) {
			this.owner = country;
			this.religion = religion;
			this.ethnicity = ethnicity;
			this.tile = tile;
			this.gender = gender != 0;
			this.age = age;
			this.job = job;
		}
		public Group(Country country, Culture religion, Culture ethnicity, int x, int y, bool gender, int age, World.Jobs job) {
			this.owner = country;
			this.religion = religion;
			this.ethnicity = ethnicity;
			this.tile = World.map[x, y];
			this.gender = gender;
			this.age = age;
			this.job = job;
		}

		public Group(Country country, Culture religion, Culture ethnicity, int x, int y, int gender, int age, World.Jobs job) {
			this.owner = country;
			this.religion = religion;
			this.ethnicity = ethnicity;
			this.tile = World.map[x, y];
			this.gender = gender != 0;
			this.age = age;
			this.job = job;
		}
		#endregion Constructor

		public int this[bool? x = null] {
			get {
				return this.owner[this];
			}
			set {
				this.owner[this] = value;
			}
		}

		public override string ToString() {
			return $"{this.owner.name} [{this.tile.x}, {this.tile.y}] {religion.name} {ethnicity.name} " + (gender == false ? "female" : "male ") + $" {age * 10} {Enum.GetName(typeof(World.Jobs), job)}";
		}

	}
}
