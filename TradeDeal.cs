using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public class TradeDeal {
		public List<Market.products> items;
		public Country recipient;

		public TradeDeal(Country recipient, Market.products[] args) {
			this.recipient = recipient;
			this.items = new List<Market.products>();
			if (args != null) {
				foreach (Market.products i in args) {
					this.items.Add(i);
				}
			}
		}
	}
}
