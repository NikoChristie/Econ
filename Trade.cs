using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public abstract class Trade {
		public readonly Market.products product;
		public double amount;
		public readonly MarketEntity target;

		public Trade(Market.products product, double amount, MarketEntity target) {
			this.product = product;
			this.amount = amount;
			this.target = target;
		}

		public abstract Trade partner();

		protected double value(Trade trade, MarketEntity buyer, MarketEntity supplier) { // always equals zero
			return (trade.amount / supplier.cost(trade.product)) * (Math.Abs(buyer.location.x - supplier.location.x) + Math.Abs(buyer.location.y - supplier.location.y));
		}

		public string ToString(bool partner = false) {
			return "(" + this.product.ToString() + " : " + this.amount + " " + this.GetHashCode() + ") " + (partner == true ? "(Match: " + this.partner().ToString() + ")\n" : "");
		}
	}
}
