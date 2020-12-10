using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public class Sell : Trade {

		public Sell(Market.products product, double amount, MarketEntity marketEntity) : base(product, amount, marketEntity) {

		}

		public override Trade partner() {

			Buy match = null;
			double score = 0;

			foreach (TradeDeal tradeDeal in this.target.location.owner.tradeDeals) {
				if (tradeDeal.items.Contains(this.product)) {
					foreach (Buy trade in tradeDeal.recipient.tradeDemand[this.product]) {
						double target_score = 0;
						if (match == null) {
							match = trade;
							score = value(trade, match.target, target);
						}
						else if ((target_score = value(trade, trade.target, target)) < score) {

							match = trade;
							score = target_score;
						}

					}
				}
			}

			/*
			foreach (TradeDeal tradeDeal in this.target.location.owner.tradeDeals) {
				foreach (Buy trade in tradeDeal.recipient.tradeDemand[this.product]) {
					if (match == null) match = trade;
					else if (value(trade, trade.target, target) < value(trade, match.target, target)) {
						
						match = trade;
					}

				}
			}
			*/

			return match;
		}
	}
}
