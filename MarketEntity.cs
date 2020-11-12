using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public abstract class MarketEntity {

		public Dictionary<Market.products, double> pool = new Dictionary<Market.products, double>();
		public Dictionary<Market.products, double> orders = new Dictionary<Market.products, double>();
		//public Dictionary<Market.products, float> supply = new Dictionary<Market.products, float>();
		//public Dictionary<Market.products, float> demand = new Dictionary<Market.products, float>();


		public Tile location;
		public float capital = 0.00f;
		public float operation_cost = 0.00f;

		public abstract float cost(Market.products products);
	}
}
