using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {

	public class Trader {
		public readonly MarketEntity home;
		public readonly MarketEntity goal;
		public int x;
		public int y;
		public List<Pathfind.node> path;// = new List<Pathfind.node>();
		public readonly Market.products product;
		public readonly double amount;

		public Trader(MarketEntity goal, Trade trade) {

			this.product = trade.product;
			this.amount = trade.amount;
			this.home = trade.target;
			this.goal = goal;

			this.x = home.location.x;
			this.y = home.location.y;



			// I think your going my way!
			foreach (Trader trader in this.home.location.owner.traders) {
				if (trader.path != null) { // traders has been initialised
					if (trader.x == this.x && trader.y == this.y) {
						if (trader.goal.location.x == goal.location.x && trader.goal.location.y == goal.location.y) {
							this.path = new List<Pathfind.node>();
							foreach (Pathfind.node node in trader.path) {
								this.path.Add(node);
							}
						}
					}
				}
			}

			if (this.path == null) this.path = Pathfind.pathfind(home.location.toNode(), goal.location.toNode(), this.home.location.owner.tradeGrid); // # pathfind
			if (this.path == null) { // still null ? Kill Your Self!
				this.home.location.owner.traders.Remove(this); // Kill Your Self!
				return;
			}

			home.pool[this.product] -= this.amount; // # remove amount from supplier
			goal.orders[this.product] += this.amount; // # add amount ordered to buyer

			// Catch bad Path

			this.x = path[0].x;
			this.y = path[0].y;
			this.path.RemoveAt(0);


			if (Program.debug) Console.WriteLine(this.home.location.owner.name + " Trader[" + this.amount + " " + this.product + "]");
		}

		public void tick() {
			if (path.Count > 0) {
				this.x = this.path[0].x;
				this.y = this.path[0].y;
				this.path.RemoveAt(0);
			}
			else {

				// # trader arrived at destination

				this.goal.pool[this.product] += this.amount; // # give factory product

				float price = this.home.cost(this.product) * (float)this.amount;

				this.home.capital += price; // # pay supplier (supplier gains money)
				this.goal.capital -= price; // # pay supplier (buyer loses money)

				this.goal.operation_cost += this.home.cost(this.product) * (float)this.amount; // # add amount payed to buyers operation cost
				this.goal.orders[this.product] -= this.amount; // # remove amount ordered from order

				//Console.WriteLine("{0} += {1} * {2}\n{3} -= {4} * {5}", this.home.capital, this.home.cost(this.product), this.amount, this.goal.capital, this.home.cost(this.product), this.amount);
				//Console.ReadLine();

				if (false) {
					Console.ForegroundColor = Program.ColorToConsoleColor(this.home.location.owner.color);
					Console.Write(this.amount + " " + this.product + "(s) have been delivered to ");
					Console.ForegroundColor = Program.ColorToConsoleColor(this.goal.location.owner.color);
					Console.Write("[" + this.x + "," + this.y + "] ");
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine(" for " + Math.Round(price, 2) + "$ (" + Math.Round(price/this.amount, 2) + "$ each )");
					Console.ResetColor();
				}

				this.home.location.owner.traders.Remove(this); // # kill self
			}
		}
	}
}
