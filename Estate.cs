using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public class Estate {

		public enum Class { Tile, Religion, Ethnicity, Gender, Age, Job };

		public string name;
		public Country owner;
		public List<Group> members;

		public Estate(Country owner, string name, List<Group> members) {
			this.name = name;
			this.owner = owner;
			this.members = members;
		}

		public override string ToString() {
			return this.name;
		}

		public int this[bool? x = null] {
			get {

				if (x != null) throw new Exception("You idiot, You absolute bafoon, You’ve made yourself a laughingstock, You uneducated monkey, Your stupidity is mindblowing to me, I cant believe you just passed " + x + " as a statement, You absolute moron, You have acted like a total fool, a total clown, HONK HONK, Yeah thats you");

				return this.sum();
			}
			set {

				if (x != null) throw new Exception("You idiot, You absolute bafoon, You’ve made yourself a laughingstock, You uneducated monkey, Your stupidity is mindblowing to me, I cant believe you just passed " + x + " as a statement, You absolute moron, You have acted like a total fool, a total clown, HONK HONK, Yeah thats you");

				if (value < 0) {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.remove();
					}
				}
				else if (value > 0) {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.append();
					}
				}
			}
		}

		private int sum() {
			return this.owner[this.members];
		}

		private void append() {
			this.owner[this.members[Program.rand.Next(this.members.Count)]] = 1;
		}

		private void remove() {
			Dictionary<Group, int> array = new Dictionary<Group, int>();

			foreach (Group group in this.members) {
				array.Add(group, this.owner[group]);
			}

			Program.WeightedRandom<Group>(array)[false] = -1;
		}

		public List<Tile> GetTiles() {
			List<Tile> list = new List<Tile>();
			foreach (Group group in this.members) {
				if (!list.Contains(group.tile)) list.Add(group.tile);
			}
			return list;
		}

		public List<Culture> GetReligions() {
			List<Culture> list = new List<Culture>();
			foreach (Group group in this.members) {
				if (!list.Contains(group.religion)) list.Add(group.religion);
			}
			return list;
		}

		public List<Culture> GetEthnicity() {
			List<Culture> list = new List<Culture>();
			foreach (Group group in this.members) {
				if (!list.Contains(group.ethnicity)) list.Add(group.ethnicity);
			}
			return list;
		}

		public List<int> GetAges() {
			List<int> list = new List<int>();
			foreach (Group group in this.members) {
				if (!list.Contains(group.age)) list.Add(group.age);
			}
			return list;
		}

		public List<World.Jobs> GetJobs() {
			List<World.Jobs> list = new List<World.Jobs>();
			foreach (Group group in this.members) {
				if (!list.Contains(group.job)) list.Add(group.job);
			}
			return list;
		}

		public List<bool> GetGender() {
			List<bool> list = new List<bool>();
			foreach (Group group in this.members) {
				if (!list.Contains(group.gender)) list.Add(group.gender);
			}
			return list;
		}

	}
}
