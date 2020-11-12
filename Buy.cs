﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public class Buy : Trade {

		public Buy(Market.products product, double amount, MarketEntity marketEntity) : base(product, amount, marketEntity) {

		}

		public override Trade partner() {

			Sell match = null;

			foreach (TradeDeal tradeDeal in this.target.location.owner.tradeDeals) {
				foreach (Sell trade in tradeDeal.recipient.tradeSupply[this.product]) { // ! watch out, "recipient" doesnt make sense in this context
					if (match == null) match = trade;
					else if (value(trade, target, trade.target) < value(trade, target, match.target)) {
						match = trade;
					}
				}

			}

			return match;
		}
	}
}
