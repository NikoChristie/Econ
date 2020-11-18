using System;
using System.Collections.Generic;
using System.Drawing;
using SdlDotNet.Graphics;
using SdlDotNet.Core;
using SdlDotNet.Graphics.Primitives;
using SdlDotNet.Input;
using SdlDotNet.Graphics.Sprites;
using System.Collections;
using System.Drawing.Configuration;
using System.Runtime.InteropServices;

namespace Econ {
	public static class Program {

		private static int frame = 0;
		public const int width = 50;
		public const int height = 50;
		public const int wpixel_width = 16;
		public const int wpixel_height = 16;
		public const int wwidth = width * wpixel_width;
		public const int wheight = height * wpixel_height;
		public static Surface Screen;
		public static bool debug = false;
		public static Random rand = new Random();
		private static int counter = 0;
		private static readonly char[] loader = { '/', '-', '\\', '|' };

		#region Sprites
		public static Sprite beeg = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\beeg_yoshi.bmp");
		public static MaskedSprite factory = new MaskedSprite(@"C:\Users\nikol\source\repos\Econ\Econ\Factory.bmp", new Box[] { new Box(new Point(1, 2), new Size(4, 12)), new Box(new Point(5, 7), new Size(10, 8))});
		public static MaskedSprite caravan = new MaskedSprite(@"C:\Users\nikol\source\repos\Econ\Econ\CaravanLand.bmp", new Box[] { new Box(new Point(1, 3), new Size(10, 10)), new Box(new Point(11, 11), new Size(4, 1)), new Box(new Point(3, 12), new Size(3, 2)) });
		public static Sprite plus  = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\+.bmp");
		public static Sprite one   = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\1.bmp");
		public static Sprite two   = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\2.bmp");
		public static Sprite three = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\3.bmp");
		public static Sprite four  = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\4.bmp");
		public static Sprite five  = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\5.bmp");
		public static Sprite six   = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\6.bmp");
		public static Sprite seven = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\7.bmp");
		public static Sprite eight = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\8.bmp");
		public static Sprite nine  = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\9.bmp");
        #endregion Sprites

        public static class Camera {
			public static float x = 0.00f;
			public static float y = 0.00f;
			public static float size_x = Program.wwidth;
			public static float size_y = Program.wheight;
			public static float scale_x = 1.00f;
			public static float scale_y = 1.00f;
		}

		public static void Main(string[] args) {

			Mouse.ShowCursor = true;
			Screen = Video.SetVideoMode(wwidth, wheight, 32, false, false, false, true);
			
			Events.TargetFps = 60;
			
			Events.Quit += (QuitEventHandler);
			Events.Tick += (TickEventHandler);
			Events.KeyboardDown += (KeyBoardDownHandler);
			Events.MouseButtonDown += (MouseButtonDownHandler);
			Events.Run();
		}
		private static void QuitEventHandler(object sender, QuitEventArgs args) {
			Events.QuitApplication();
		}
		private static void TickEventHandler(object sender, TickEventArgs args) {
			if(frame % loader.Length == 0)counter = counter < loader.Length - 1 ? counter + 1 : 0;
			frame = (frame + 1) % Events.Fps; // every second
			if (World.map != null) {
				PrintMap();
				Screen.Update();
			}
			if(frame == 0) World.tick();
			Video.WindowCaption = $"{loader[counter]} {World.day} at {Events.Fps} fps { Math.Round(((float)Events.Fps / (float)Math.Max(Events.TargetFps, 1)) * 100, 0) }%";
			//Video.WindowCaption = $"{loader[counter]} {World.date.Year}-{World.date.Month}-{World.date.Day}-{World.date.DayOfWeek/*World.day*/} at {Events.Fps} fps";

		}

		private static void KeyBoardDownHandler(object sender, KeyboardEventArgs args) { // completely broken
			
			if (args.Key == Key.Escape) Screen.Close();
		}

		private static void MouseButtonDownHandler(object sender, MouseButtonEventArgs args) {
			if (args.Button == MouseButton.PrimaryButton) {
				int x = Mouse.MousePosition.X / wpixel_width;
				int y = Mouse.MousePosition.Y / wpixel_height;

				if (World.map[x, y].owner != null) {
					PrintEcon(World.map[x, y]);
				}
			}
			else if (args.Button == MouseButton.SecondaryButton) {
				int x = Mouse.MousePosition.X / wpixel_width;
				int y = Mouse.MousePosition.Y / wpixel_height;

				if (World.map[x, y].owner != null) {
					foreach (Pop pop in World.map[x, y].population) {
						PrintPop(pop);
					}
				}
			}
		}

		public static T WeightedRandom<T>(Dictionary<T, int> list) {
			int sum = 0;
			foreach (KeyValuePair<T, int> i in list) {
				sum += i.Value;
			}

			int index = Program.rand.Next(sum) + 1;

			foreach (KeyValuePair<T, int> i in list) {
				if (index > i.Value) {
					index -= i.Value;
				}
				else {
					return i.Key;
				}

			}
			throw new Exception("Error: random_index: " + index + " couldn't find a value and is returning null");

		}

		public static void PrintEcon(Tile tile) {
			Console.Clear();
			Console.ForegroundColor = ColorToConsoleColor(tile.owner.color);
			Console.WriteLine($"Tile[{tile.x}, {tile.y}] has {tile.factories.Count} factories and a population of {tile[null]}");

			foreach (Factory factory in tile.factories) {

				Console.ForegroundColor = ColorToConsoleColor(tile.owner.color);
				Console.BackgroundColor = ConsoleColor.Black;

				Console.Write("\t" + factory.output + " factory, pop : " + factory.population + " /" + factory.capacity + " at " + factory.wages + "$/hr (min:" + tile.owner.minimum_wage + "$/hr) produces " + factory.output + "(" + factory.pool[factory.output] + ") costing ");

				Console.ForegroundColor = ConsoleColor.Green;

				Console.Write(Math.Round(factory.cost(factory.output), 2) + "$");

				Console.ForegroundColor = ColorToConsoleColor(tile.owner.color);

				Console.Write(" with ");

				if (factory.input != null) {
					bool print = false;
					foreach (KeyValuePair<Market.products, double> i in factory.input) {
						if (print == false) {
							print = true;
						}
						else {
							Console.Write(',');
						}
						Console.Write(" " + i.Value + " " + i.Key + "(s) (" + factory.pool[i.Key] + "/" + factory.input[i.Key] * factory.location.owner.workhours * factory.population + ")");
					}
				}
				else {
					Console.Write("nothing");
				}

				Console.Write(" they have ");

				if (factory.capital > 0) {
					Console.ForegroundColor = ConsoleColor.Green;
				}
				else if (factory.capital < 0) {
					Console.ForegroundColor = ConsoleColor.Red;
				}
				else {
					Console.ForegroundColor = ConsoleColor.Yellow;
				}


				Console.Write(factory.capital + "$");
				Console.ForegroundColor = ColorToConsoleColor(tile.owner.color);


				if (factory.input != null) {

					//Console.ForegroundColor = ColorToConsoleColor(tile.owner.color);
					Console.WriteLine();

					Console.Write("\t\torders: ");
					bool print = false;
					foreach (KeyValuePair<Market.products, double> i in factory.orders) {
						if (print == false) {
							print = true;
						}
						else {
							Console.Write(',');
						}
						Console.Write(i.Key + " (" + factory.orders[i.Key] + ")");
					}
				}

				Console.ResetColor();

				Console.WriteLine();
			}

			Console.ResetColor();
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Employment");
			foreach (World.Jobs job in Enum.GetValues(typeof(World.Jobs))) {
				int workers = tile[null, null, null, null, job];
				if (workers > 0) {
					float unemployment = tile.unemployment(job);
					Console.ForegroundColor = ComparisonColor(unemployment, 1 - 0.045); // 4.5% is considered a good unemployment rate
					if (unemployment == 0) Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"{job} at {unemployment * 100}% ({Math.Round(workers * unemployment, 0)} / {workers})");
				}
			}
			Console.ResetColor();
		}

		public static void PrintPop(Pop pop) {
			Console.ForegroundColor = ColorToConsoleColor(pop.location.owner.color);
			Console.WriteLine("\n[" + pop.religion.index + "," + pop.ethnicity.index + "] Population: " + pop[null]);
			for (int i = 0; i < 2; i++) {
				switch (i) {
					case 0:
						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.WriteLine("Female: " + pop[i]);
						break;
					case 1:
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.WriteLine("Male:   " + pop[i]);
						break;
				}
				for (int j = 0; j < 10; j++) {
					Console.WriteLine("\tAge " + (j * 10) + ": " + pop[i, j]);

					foreach (World.Jobs k in Enum.GetValues(typeof(World.Jobs))) {
						if (pop[i, j, k] > 0) Console.WriteLine("\t" + k.ToString() + ": " + pop[i, j, k]);
					}
					Console.WriteLine();
				}

				Console.ResetColor();
				Console.ForegroundColor = ColorToConsoleColor(pop.location.owner.color);

				Console.WriteLine("Jobs: ");

				foreach (World.Jobs j in Enum.GetValues(typeof(World.Jobs))) {
					Console.WriteLine("\t" + j.ToString() + ": " + pop[i, null, j]);
				}

				Console.WriteLine();
			}
			Console.WriteLine("\n");
		}

		public static ConsoleColor ColorToConsoleColor(Color color, bool black = false) {

			Dictionary<ConsoleColor, Color> consoleColors = new Dictionary<ConsoleColor, Color>() {
				{ ConsoleColor.Black,       Color.FromArgb(0,     0,   0) },
				{ ConsoleColor.DarkBlue,    Color.FromArgb(0,     0, 128) },
				{ ConsoleColor.DarkGreen,   Color.FromArgb(0,   128,   0) },
				{ ConsoleColor.DarkCyan,    Color.FromArgb(0,   128, 128) },
				{ ConsoleColor.DarkRed,     Color.FromArgb(128,   0,   0) },
				{ ConsoleColor.DarkMagenta, Color.FromArgb(128,   0, 128) },
				{ ConsoleColor.DarkYellow,  Color.FromArgb(128, 128,   0) },
				{ ConsoleColor.Gray,        Color.FromArgb(192, 192, 192) },
				{ ConsoleColor.DarkGray,    Color.FromArgb(128, 128, 128) },
				{ ConsoleColor.Blue,        Color.FromArgb(0,     0, 255) },
				{ ConsoleColor.Green,       Color.FromArgb(0,   255,   0) },
				{ ConsoleColor.Cyan,        Color.FromArgb(0,   255, 255) },
				{ ConsoleColor.Red,         Color.FromArgb(255,   0,   0) },
				{ ConsoleColor.Magenta,     Color.FromArgb(255,   0, 255) },
				{ ConsoleColor.Yellow,      Color.FromArgb(255,   0,   0) },
				{ ConsoleColor.White,       Color.FromArgb(255, 255, 255) }
			};

			ConsoleColor closest = ConsoleColor.White;
			
			foreach (KeyValuePair<ConsoleColor, Color> i in consoleColors) {
				if (ColorSubtract(i.Value, color) < ColorSubtract(consoleColors[closest], color)) {
					closest = i.Key;
				}
			}
			return !black ? (closest.Equals(ConsoleColor.Black) ? ConsoleColor.DarkGray : closest) : closest;

		}

		private static int ColorSubtract(Color a, Color b) {
			return Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
		}

		public static void PrintMap() {
			try {
				Program.Screen.Fill(new Rectangle(0, 0, Program.wwidth, Program.wheight), Color.DarkBlue);

				int xx;
				int yy = (int)Camera.y;

				Camera.size_x = Program.wwidth;
				Camera.size_y = Program.wheight;

				for (int y = 0; y < (Camera.size_y - Camera.y) * Camera.scale_y && yy < Program.height; y += (int)Camera.scale_y * wpixel_height) {
					xx = (int)Camera.x;

					for (int x = 0; x < (Camera.size_x - Camera.x) * Camera.scale_x && xx < Program.width; x += (int)Camera.scale_x * wpixel_width) {

						if (World.map[xx, yy].owner != null) {
							Box draw_tile = new Box((short)x, (short)y, (short)(x + (Camera.scale_x * Program.wpixel_width)), (short)(y + (Camera.scale_y * Program.wpixel_height)));
							
							draw_tile.Draw(Program.Screen, World.map[xx, yy].owner.color, true, true);

							//this->DrawDecal(olc::vf2d(x, y), decal, olc::vf2d(Camera::scale_x / decal->sprite->width, Camera::scale_y / decal->sprite->height), pixel);
							
							/*
							if (false && false) { // <-----------------------------------------

								for (int i = 0; i < 4; i++) {
									int _x = xx;
									int _y = yy;
									switch (i) {
										case 0:
											_y--;
											break;
										case 1:
											_y++;
											break;
										case 2:
											_x--;
											break;
										case 3:
											_x++;
											break;
									}
									if (_x > -1 && _x < width && _y > -1 && _y < height) {
										if (World.map[_x,_y].owner != World.map[xx,yy].owner) { // If we remove null the program runs much much faster

											//Box line;
											int scale_x = (int)(Camera.scale_x / 16);
											int scale_y = (int)(Camera.scale_y / 16);

											switch (i) {
												case 0:
													DrawLineStraight(x, y, (int)Camera.scale_x + x, y, scale_x, scale_y, Color.Black);
													break;
												case 1:
													DrawLineStraight(x, (int)(y + Camera.scale_y), (int)(Camera.scale_x + x), (int)(Camera.scale_y + y), scale_x, scale_y, Color.Black);
													break;
												case 2:
													DrawLineStraight(x, y, x, (int)(Camera.scale_y + y), scale_x, scale_y, Color.Black);
													break;
												case 3:
													DrawLineStraight((int)(x + Camera.scale_x), y, (int)(x + Camera.scale_x), (int)(y + Camera.scale_y), scale_x, scale_y, Color.Black);
													break;
											}
										}
									}

								}
							}
							*/
							if (true) { // factory


								if (World.map[xx, yy].factories.Count > 0) {
									//Screen.Blit(factory, new Point(x, y));
									factory.Blit(Program.Screen, new Point(x, y), shade(World.map[xx, yy].owner.color, 0.50f), 200);
									Sprite number;
									switch (World.map[xx, yy].factories.Count) {
										case 1:
											number = one;
											break;
										case 2:
											number = two;
											break;
										case 3:
											number = three;
											break;
										case 4:
											number = four;
											break;
										case 5:
											number = five;
											break;
										case 6:
											number = six;
											break;
										case 7:
											number = seven;
											break;
										case 8:
											number = eight;
											break;
										case 9:
											number = nine;
											break;
										default:
											if (World.map[xx, yy].factories.Count < 1) {
												Console.ForegroundColor = ConsoleColor.Red;
												throw new Exception($"World.map[{xx}, {yy}] has {World.map[xx, yy].factories.Count}");
											}
											else number = plus; break;
									}
									Screen.Blit(number, new Point(x, y));
									
									//draw_tile.Draw(Program.Screen, Color.FromArgb(50, World.map[xx, yy].owner.color.R, World.map[xx, yy].owner.color.G, World.map[xx, yy].owner.color.B), true, true);
								}
							}


						}
						// forgot to draw land tile numbskull
						
						xx++;
					}

					yy++;

				}

				if (true) { // trader
					foreach (Country country in World.countries) {
						foreach (Trader trader in country.traders) {
							if (World.map[trader.x, trader.y].factories.Count == 0) {

								//Screen.Blit(caravan, new Point(trader.x * Program.wpixel_width, trader.y * Program.wpixel_height));
								caravan.Blit(Program.Screen, new Point(trader.x * Program.wpixel_width, trader.y * Program.wpixel_height), trader.home.location.owner.color, 200);
								//Screen.AlphaBlending = false;
								//Box tint = new Box(new Point(trader.x * Program.wpixel_width, trader.y * Program.wpixel_height), caravan.Size);
								//tint.Draw(Program.Screen, Color.FromArgb(50, trader.home.location.owner.color.R, trader.home.location.owner.color.G, trader.home.location.owner.color.B), true, true);
							}
						}
					}
				}

				Program.Screen.Update();
			}
			
			catch (Exception e) {
				Console.ResetColor();
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"{e.Message} {e.StackTrace}");
				Console.ReadLine();
				Environment.Exit(-1);
			}
		}



		// shade newR = currentR * (1 - shade_factor)
		// tint  newR = currentR + (255 - currentR) * tint_factor
		public static Color shade(Color color, float shade) {
			
			return Color.FromArgb( (int)(color.R * (1 - shade)), (int)(color.G * (1 - shade)), (int)(color.B * (1 - shade))  );
		}

		//public static Color tint()

		public static ConsoleColor ComparisonColor(double a, double b, ConsoleColor gt = ConsoleColor.Green, ConsoleColor eq = ConsoleColor.Yellow, ConsoleColor ls = ConsoleColor.Red) {
			if (a > b) return gt;
			else if (a < b) return ls;
			else if (a == b) return eq;
			else return ConsoleColor.White;
		}

		public static void DrawLineStraight(int x, int y, int x_end, int y_end, int width, int height, Color color) { // height and width parameters dont do squat

			int _height = Math.Abs(y_end - y);
			int _width = Math.Abs(x_end - x);

			_height = _height > 0 ? _height : 1;
			_width = _width > 0 ? _width : 1;

			//this->DrawDecal(olc::vf2d(float(x), float(y)), base, olc::vf2d(float(width + _width), float(height + _height)), pixel);
			Box box = new Box((short)x, (short)y, (short)(x + (width + _width)), (short)(y + (height + _height)));
			box.Draw(Screen, color);
		}

		public static Dictionary<T, R> CopyDictionary<T, R>(Dictionary<T, R> dictionary) {
			Dictionary<T, R> copy = new Dictionary<T, R>();
			foreach (KeyValuePair<T, R> i in dictionary) {
				copy.Add(i.Key, dictionary[i.Key]);
			}
			return copy;
		}


	}
}
