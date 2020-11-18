using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public class Pop {

		public readonly Tile location;
		public static List<World.Jobs> jobs_debug = Enum.GetValues(typeof(World.Jobs)).Cast<World.Jobs>().ToList(); // ???

		public readonly Culture religion;
		public readonly Culture ethnicity;
		public readonly Estate estate;

		private List<List<Dictionary<World.Jobs, int>>> population = new List<List<Dictionary<World.Jobs, int>>>();
		public Pop(Culture religion, Culture ethnicity, Tile location, int pop) {

			this.religion = religion;
			this.ethnicity = ethnicity;
			this.location = location;

			#region innit
			for (int i = 0; i < 2; i++) { // Gender
				population.Add(new List<Dictionary<World.Jobs, int>>());
				for (int j = 0; j < 10; j++) { // Age
					population[i].Add(new Dictionary<World.Jobs, int>());
					foreach (World.Jobs k in Enum.GetValues(typeof(World.Jobs))) { // Jobs
						population[i][j].Add(k, 0);
					}
				}
			}
			#endregion innit 
			this[null] = pop;

		}

		public double ideas(string value) {
			return this.religion.ideas[value] + this.ethnicity.ideas[value];
		}

		public int this[bool? gender = null, int? age = null, World.Jobs? job = null] {
			get {
				return this.sum(gender, age, job);
			}
			set {
				if (value < 0) {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.remove(gender, age, job);
					}
				}
				else if (value > 0) {
					for (int i = 0; i < Math.Abs(value); i++) {
						this.append(gender, age, job);
					}
				}
			}
		}

		public int this[int gender, int? age = null, World.Jobs? job = null] { // overload for int, useful for when in iteration

			get {
				return this[gender != 0, age, job];
			}
			set {
				this[gender != 0, age, job] = value;
			}
		}

		private void remove(bool? gender = null, int? age = null, World.Jobs? job = null) {

			if (gender == null) {

				gender = Program.WeightedRandom<bool>(new Dictionary<bool, int>() { { true, this[true, age, job] }, { false, this[false, age, job] } });
			}

			if (age == null) {

				Dictionary<int, int> array = new Dictionary<int, int>();
				for (int i = 0; i < this[gender]; i++) {
					array.Add(i, this[gender, i]);
				}

				age = Program.WeightedRandom<int>(array);
			}

			if (job == null) {

				Dictionary<World.Jobs, int> array = new Dictionary<World.Jobs, int>();

				foreach (KeyValuePair<World.Jobs, int> i in this.population[gender == false ? 0 : 1][(int)age]) {
					array.Add(i.Key, i.Value);
				}

				job = Program.WeightedRandom<World.Jobs>(array);

			}

			if (this[gender, age, job] - 1 < 0) {

				Console.ForegroundColor = ConsoleColor.Magenta;
				throw new Exception("Error! this.population[" + gender + "][" + age + "][" + job + "] = " + (this[gender, age, job] - 1));
			}

			this.population[(int)(gender == false ? 0 : 1)][(int)age][(World.Jobs)job]--;

		}

		private void append(bool? gender = null, int? age = null, World.Jobs? job = null) {

			if (gender == null) {
				gender = Program.rand.Next(2) != 0;
			}

			if (age == null) {
				age = Program.rand.Next(10);
			}

			if (job == null) {

				// ! rework to take all avaliable jobs from heirarchy and give job
				//List<World.Jobs> jobs = Enum.GetValues(typeof(World.Jobs)).Cast<World.Jobs>().ToList(); // mega slow down
				job = Pop.jobs_debug[Program.rand.Next(Pop.jobs_debug.Count)];

			}

			this.population[gender == false ? 0 : 1][(int)age][(World.Jobs)job]++;
		}

		public bool reproduce() {
			int female = this[false, 2] + this[false, 3];
			int male = this[true, 2] + this[true, 3];

			int min = male > female ? female : male;

			this[null, 0] = min; // Babies

			return min != female;
		}

		public void birthday() { // move to Estate

			// remove boomers

			while (this[null, 9] > 0) {
				//Console.WriteLine("Boomers: " + this[null, 9]);
				this[null, 9] = -1;

			}

			for (int i = 0; i < 2; i++) {

				// add babies
				population[i].Insert(0, new Dictionary<World.Jobs, int>());
				foreach (World.Jobs k in Enum.GetValues(typeof(World.Jobs))) { // Jobs
					population[i][0].Add(k, 0);
				}
			}
		}

		private int sum(bool? _gender = null, int? age = null, World.Jobs? job = null) {

			int gender;
			switch (_gender) {
				case false:
					gender = 0;
					break;
				case true:
					gender = 1;
					break;
				default:
					gender = -1;
					break;
			}

			if (gender == -1) { // All Gender
				if (age == null) { // All Age
					if (job == null) { // All Job
						int sum = 0;
						for (int i = 0; i < population.Count; i++) {
							for (int j = 0; j < population[i].Count; j++) {
								foreach (KeyValuePair<World.Jobs, int> k in population[i][j]) {
									sum += k.Value;
								}
							}
						}
						return sum;
					}
					else { // Job
						int sum = 0;
						for (int i = 0; i < population.Count; i++) {
							for (int j = 0; j < population[i].Count; j++) {
								sum += population[i][j][(World.Jobs)job];
							}
						}
						return sum;
					}
				}
				else { // Age
					if (job == null) { // All Job
						int sum = 0;
						for (int i = 0; i < population.Count; i++) {
							foreach (KeyValuePair<World.Jobs, int> j in population[i][(int)age]) {
								sum += j.Value;
							}
						}
						return sum;
					}
					else { // Job
						int sum = 0;
						for (int i = 0; i < population.Count; i++) {
							sum += population[i][(int)age][(World.Jobs)job];
						}
						return sum;
					}
				}
			}
			else { // Gender
				if (age == null) { // All Age
					if (job == null) { // All Job
						int sum = 0;
						for (int i = 0; i < population[gender].Count; i++) {
							foreach (KeyValuePair<World.Jobs, int> k in population[gender][i]) {
								sum += k.Value;
							}
						}
						return sum;
					}
					else { // Job
						int sum = 0;
						for (int i = 0; i < population[gender].Count; i++) {
							sum += population[gender][i][(World.Jobs)job]; // stackoverflow???
						}
						return sum;
					}
				}
				else { // Age
					if (job == null) { // All Job
						int sum = 0;
						foreach (KeyValuePair<World.Jobs, int> k in population[gender][(int)age]) {
							sum += k.Value;
						}
						return sum;
					}
					else { // Job
						return population[gender][(int)age][(World.Jobs)job];

					}
				}
			}
		}

		public int reproduce(ref bool gender) {

			int male = this[true, 2] + this[true, 3];
			int female = this[false, 2] + this[false, 3];

			gender = male > female;

			this[null, 0] = gender == true ? female : male; // append smaller value (men or women)

			return Math.Abs(male - female); // return difference of men / women pop (absolute so its not negative)
		}

	}
}
