using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
    public class Culture {

		public readonly int index;
		public string name;
		public Dictionary<string, double> ideas = new Dictionary<string, double>() { { "gender", 0 } }; // use enum nerd

		public Culture(int index, string name) {
			this.name = name;
			this.index = index;

			Dictionary<string, double> copy = new Dictionary<string, double>();
			foreach (KeyValuePair<string, double> i in this.ideas) {
				copy.Add(i.Key, this.ideas[i.Key]);
			}

			foreach (KeyValuePair<string, double> i in copy) {
				this.ideas[i.Key] = Program.rand.Next(21) - 10;
			}

		}

		public override string ToString() {
			return this.name;
		}

	}
}
