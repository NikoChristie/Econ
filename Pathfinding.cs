using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Econ {
	public static class Pathfind {

		public static List<node> pathfind(node start, node end, node[,] grid, bool debug = false) {

			debug = Program.debug;

			if (debug) Console.WriteLine("Pathfind [" + start.x + "," + start.y + "] to [" + end.x + "," + end.y + "]");

			List<node> open = new List<node>() { start };
			List<node> closed = new List<node>() { start }; // ?
			List<node> path = new List<node>();

			if (end.cost <= 0) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Path unreachable!");
				Console.ResetColor();
				return path;
			}

			while (open.Count > 0) {

				node current = open[0];
				if (debug) {

					Pathfind.printGrid(grid, start, end, current);

					Console.WriteLine("Current[" + current.x + "," + current.y + "]");


					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("open [");
					foreach (Pathfind.node node in open) {
						Console.Write(" [" + node.x + "," + node.y + "]");
					}
					Console.WriteLine("]");

					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write("closed [");
					foreach (Pathfind.node node in closed) {
						Console.Write(" [" + node.x + "," + node.y + "]");
					}
					Console.WriteLine("]");
					Console.ResetColor();

				}
				if (current.x == end.x && current.y == end.y) { // end found
					if (debug) Console.WriteLine("End has been found!");
					while (current.parent != null) {
						path.Add(current.parent);
						current = current.parent;
					}
					path.Reverse();
					path.Add(end);
					return path;
				}

				else {

					if (open.Count > (grid.GetUpperBound(0) + 1) * (grid.GetUpperBound(1) + 1)) {
						Console.ForegroundColor = ConsoleColor.Red;

						Console.WriteLine("uwu notices size of open list (" + open.Count + ")");
						Console.ReadLine();

						Console.ResetColor();
					}

					for (int i = 0; i < 8; i++) {

						int x = current.x;
						int y = current.y;

						switch (i) {
							case 0:
								x++;
								break;
							case 1:
								x--;
								break;
							case 2:
								y++;
								break;
							case 3:
								y--;
								break;
							case 4:
								x++;
								y++;
								break;
							case 5:
								x++;
								y--;
								break;
							case 6:
								x--;
								y++;
								break;
							case 7:
								x--;
								y--;
								break;

						}

						if (debug) Console.WriteLine("Neighbor [" + x + "," + y + "]"); //Console.ReadLine();

						if (x >= 0 && x <= grid.GetUpperBound(0) && y >= 0 && y <= grid.GetUpperBound(1)) { // Should prevent infinite openlist append loop in corners

							node neighbor = new node(x, y, grid[x, y].cost, current);

							if (!contains(neighbor, closed.ToArray())) { // Node not in closed list ? next
								if (neighbor.cost > 0) {
									if (debug) Console.WriteLine("Appending Node");

									open.Add(neighbor);
									for (int j = 0; j < open.Count; j++) {
										if (heuristic(neighbor, end) < heuristic(open[j], end)) {
											open.Insert(j, neighbor);
											open.RemoveAt(open.Count - 1); // ? wouldnt you want to pop front ??? 
											break;
										}
									}
								}
								else {
									if (debug) Console.WriteLine("Not appending node because it is in the cost is less than zero");

								}
								closed.Add(neighbor);
							}
							else { // Otherwise ? already in closed list;
								if (debug) Console.WriteLine("Not appending node because it is in the closed list");
							}
						}

					}

					open.Remove(current); // why not just pop front ? bc its in an already sorted list numbnuts
										  //closed.Add(current);
				}
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Error: Wait this shouldn't have happend");
			Console.ResetColor();
			Console.ReadLine();
			if (debug) return pathfind(start, end, grid, true);
			else return null;
		}

		private static void printGrid(node[,] grid, node start, node end, node current) {
			for (int y = 0; y < grid.GetUpperBound(1) + 1; y++) {
				Console.Write("[");
				for (int x = 0; x < grid.GetUpperBound(0) + 1; x++) {

					if (x == current.x && y == current.y) {
						Console.BackgroundColor = ConsoleColor.Yellow;
					}
					else if (x == start.x && y == start.y) {
						Console.BackgroundColor = ConsoleColor.DarkGreen;
					}
					else if (x == end.x && y == end.y) {
						Console.BackgroundColor = ConsoleColor.DarkRed;
					}

					if (grid[x, y].cost > 0) {
						Console.ForegroundColor = ConsoleColor.Green;
					}
					else {
						Console.ForegroundColor = ConsoleColor.Red;
					}

					Console.Write(" " + Math.Round(grid[x, y].cost, 0) + " ");

					Console.ResetColor();
				}
				Console.WriteLine("]");
			}
		}

		private static bool contains(node j, node[] list) {
			foreach (node i in list) {
				if (j.x == i.x && j.y == i.y) {
					return true;
				}
			}
			return false;
		}

		private static double heuristic(node current, node end) {
			return current.cost * (Math.Abs(current.x - end.x) + Math.Abs(current.y - end.y));
		}

		public class node {
			public readonly int x;
			public readonly int y;
			public double cost;
			public node parent = null;

			public node(int x, int y, double cost, node parent = null) {
				this.x = x;
				this.y = y;
				this.cost = cost;
				this.parent = parent;
			}
		}

	}

}
