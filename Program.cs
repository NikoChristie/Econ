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
		public static Sprite beeg = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\beeg_yoshi.bmp");
		public static Sprite factory = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\Factory.bmp");
		public static Sprite caravan = new Sprite(@"C:\Users\nikol\source\repos\Econ\Econ\CaravanLand.bmp");


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
			
			Events.TargetFps = 120;
			Events.Quit += (QuitEventHandler);
			Events.Tick += (TickEventHandler);
			Events.KeyboardDown += (KeyBoardDownHandler);
			Events.Run();
		}
		private static void QuitEventHandler(object sender, QuitEventArgs args) {
			Events.QuitApplication();
		}
		private static void TickEventHandler(object sender, TickEventArgs args) {
			frame = (frame + 1) % Events.TargetFps; // every second
			if (World.map != null) {
				Screen.Blit(beeg, Mouse.MousePosition);
				PrintMap();
				Screen.Blit(beeg, Mouse.MousePosition);
				Screen.Update();
			}
			if(frame == 0) World.tick();
			Video.WindowCaption = $"{World.day} at {Events.Fps} fps";

		}

		private static void KeyBoardDownHandler(object sender, KeyboardEventArgs args) {

			if (args.Key == Key.Escape) Screen.Close();
			//if (GetKey(olc::Key::G).bPressed) this->grid = !this->grid;
			//if (GetKey(olc::Key::B).bPressed) this->border = !this->border;
			//if (GetKey(olc::Key::T).bPressed) this->trade = !this->trade;

			if (args.Key == Key.S) {
				Camera.y += 1;// this->frame * fElapsedTime;
				Camera.y = Camera.y + (wheight / Camera.scale_y) >= height ? Camera.y - 1 : Camera.y;
			}
			else if (args.Key == Key.W) {
				Camera.y -= 1;// this->frame * fElapsedTime;
				Camera.y = Camera.y < 0 ? 0 : Camera.y;
			}
			if (args.Key == Key.D) {
				Camera.x += 1;// this->frame * fElapsedTime;
				Camera.x = Camera.x + (wwidth / Camera.scale_x) >= width ? Camera.x - 1 : Camera.x;
			}
			else if (args.Key == Key.A) {
				Camera.x -= 1;//this->frame * fElapsedTime;
				Camera.x = Camera.x < 0 ? 0 : Camera.x;
			}
			if (args.Key == Key.Q) {
				Camera.scale_x += 1;//this->frame * fElapsedTime;
				Camera.scale_y += 1;//this->frame * fElapsedTime;
			}
			else if (args.Key == Key.E) {
				Camera.scale_x -= (Camera.x - 1) + (wwidth / (Camera.scale_x - 1)) >= width + 1 ? 0 : 1; // double check
				Camera.scale_y -= (Camera.y - 1) + (wheight / (Camera.scale_y - 1)) >= height ? 0 : 1;
				if (Camera.scale_x != Camera.scale_y) { // Keep Scale Resolution equal
					Camera.scale_x = Camera.scale_y;
				}
			}
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
							if (true) {


								if (World.map[xx, yy].factories.Count > 0) {
									
									Screen.Blit(factory, new Point(x, y));
								}
							}


						}
						
						
						xx++;
					}

					yy++;

				}

				if (true) {
					foreach (Country country in World.countries) {
						foreach (Trader trader in country.traders) {
							if(World.map[trader.x, trader.y].factories.Count == 0) Screen.Blit(caravan, new Point(trader.x * Program.wpixel_width, trader.y * Program.wpixel_height));
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
				System.Environment.Exit(-1);
			}
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
