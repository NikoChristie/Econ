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
using System.Security;

namespace Econ {
	public class MaskedSprite : Sprite {

		private Box[] mask;

		public MaskedSprite(string surfaceFile, Box[] mask) : base(surfaceFile) {
			this.mask = mask;
		}

		public void Blit(Surface surface, Point point, Color color, int alpha = 255) {
			surface.Blit(this, point);
			if (alpha < 255) color = Color.FromArgb(alpha, color.R, color.G, color.B);
			foreach(Box i in mask) {
				Box box = new Box(new Point(point.X + i.Location.X, point.Y + i.Location.Y), i.Size);
				
				box.Draw(surface, color, false, true);
			}
		}

	}
}
