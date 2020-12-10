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
		/*
		protected double value(Trade trade, MarketEntity buyer, MarketEntity supplier) { 
			return (trade.amount / supplier.cost(trade.product)) * (Math.Abs(buyer.location.x - supplier.location.x) + Math.Abs(buyer.location.y - supplier.location.y));
		}
		*/

		protected double value(Trade trade, MarketEntity buyer, MarketEntity supplier) {

			// DEBUG SHIT
			if (trade.amount == 0 || supplier.cost(trade.product) == 0) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Uh oh either trade.amount ({trade.amount}) == 0 ir supplier.cost({trade.product}) == 0");
				Console.ResetColor();
				Console.ReadLine();
			}
			if (buyer.location.x == supplier.location.x && buyer.location.y == supplier.location.y) return 0; 
			else return (trade.amount / supplier.cost(trade.product)) * (Math.Abs(buyer.location.x - supplier.location.x) + Math.Abs(buyer.location.y - supplier.location.y));
		}

		public string ToString(bool partner = false) {
			return "(" + this.product.ToString() + " : " + this.amount + " " + this.GetHashCode() + ") " + (partner == true ? "(Match: " + this.partner().ToString() + ")\n" : "");
		}

        public override bool Equals(object obj) {
			Trade other = (Trade)obj;
			if (this.product == other.product) {
				if (this.amount == other.amount) {
					return true;
				}
				else return false;
			}
			else return false;
        }
    }
}
