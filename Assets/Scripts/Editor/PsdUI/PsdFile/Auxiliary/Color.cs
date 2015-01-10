using System;
using System.ComponentModel;

namespace PhotoshopFile.Auxiliary
{
	public struct Color
	{
		//
		// Static Fields
		//
		public static readonly Color Empty;

		long value;
		long state;

		//
		// Properties
		//
		public byte A {
			get {
				return (byte)(this.Value >> 24);
			}
		}
		
		public byte B {
			get {
				return (byte)this.Value;
			}
		}
		
		public byte G {
			get {
				return (byte)(this.Value >> 8);
			}
		}
		
		public bool IsEmpty {
			get {
				return this.state == 0;
			}
		}
		
		public bool IsKnownColor {
			get {
				return (this.state & 1) != 0;
			}
		}
		
		public bool IsNamedColor {
			get {
				return (this.state & 5) != 0;
			}
		}
		
		public bool IsSystemColor {
			get {
				return (this.state & 8) != 0;
			}
		}
		
		public byte R {
			get {
				return (byte)(this.Value >> 16);
			}
		}
		
		internal long Value {
			get {
				return this.value;
			}
			set {
				this.value = value;
			}
		}
		
		//
		// Static Methods
		//
		private static void CheckARGBValues (int alpha, int red, int green, int blue)
		{
			if (alpha > 255 || alpha < 0) {
				throw Color.CreateColorArgumentException (alpha, "alpha");
			}
			Color.CheckRGBValues (red, green, blue);
		}
		
		private static void CheckRGBValues (int red, int green, int blue)
		{
			if (red > 255 || red < 0) {
				throw Color.CreateColorArgumentException (red, "red");
			}
			if (green > 255 || green < 0) {
				throw Color.CreateColorArgumentException (green, "green");
			}
			if (blue > 255 || blue < 0) {
				throw Color.CreateColorArgumentException (blue, "blue");
			}
		}
		
		private static ArgumentException CreateColorArgumentException (int value, string color)
		{
			return new ArgumentException (string.Format ("'{0}' is not a valid value for '{1}'. '{1}' should be greater or equal to 0 and less than or equal to 255.", value, color));
		}
		
		public static Color FromArgb (int alpha, int red, int green, int blue)
		{
			Color.CheckARGBValues (alpha, red, green, blue);
			return new Color {
				state = 2,
				Value = (long)((alpha << 24) + (red << 16) + (green << 8) + blue)
			};
		}
		
		public static Color FromArgb (int red, int green, int blue)
		{
			return Color.FromArgb (255, red, green, blue);
		}
		
		public static Color FromArgb (int argb)
		{
			return Color.FromArgb (argb >> 24 & 255, argb >> 16 & 255, argb >> 8 & 255, argb & 255);
		}
		
		public static Color FromArgb (int alpha, Color baseColor)
		{
			return Color.FromArgb (alpha, (int)baseColor.R, (int)baseColor.G, (int)baseColor.B);
		}
		
		//
		// Methods
		//
		public override bool Equals (object obj)
		{
			if (!(obj is Color)) {
				return false;
			}
			Color right = (Color)obj;
			return this == right;
		}
		
		public float GetBrightness ()
		{
			byte b = Math.Min (this.R, Math.Min (this.G, this.B));
			byte b2 = Math.Max (this.R, Math.Max (this.G, this.B));
			return (float)(b2 + b) / 510;
		}
		
		public override int GetHashCode ()
		{
			int num = (int)(this.Value ^ this.Value >> 32 ^ (long)this.state);
			return num;
		}
		
		public float GetHue ()
		{
			int r = (int)this.R;
			int g = (int)this.G;
			int b = (int)this.B;
			byte b2 = (byte)Math.Min (r, Math.Min (g, b));
			byte b3 = (byte)Math.Max (r, Math.Max (g, b));
			if (b3 == b2) {
				return 0;
			}
			float num = (float)(b3 - b2);
			float num2 = (float)((int)b3 - r) / num;
			float num3 = (float)((int)b3 - g) / num;
			float num4 = (float)((int)b3 - b) / num;
			float num5 = 0;
			if (r == (int)b3) {
				num5 = 60 * (6 + num4 - num3);
			}
			if (g == (int)b3) {
				num5 = 60 * (2 + num2 - num4);
			}
			if (b == (int)b3) {
				num5 = 60 * (4 + num3 - num2);
			}
			if (num5 > 360) {
				num5 -= 360;
			}
			return num5;
		}
		
		public float GetSaturation ()
		{
			byte b = Math.Min (this.R, Math.Min (this.G, this.B));
			byte b2 = Math.Max (this.R, Math.Max (this.G, this.B));
			if (b2 == b) {
				return 0;
			}
			int num = (int)(b2 + b);
			if (num > 255) {
				num = 510 - num;
			}
			return (float)(b2 - b) / (float)num;
		}
		
		public int ToArgb ()
		{
			return (int)this.Value;
		}
		
		public override string ToString ()
		{
			if (this.IsEmpty) {
				return "Color [Empty]";
			}
			return string.Format ("Color [A={0}, R={1}, G={2}, B={3}]", new object[] {
				this.A,
				this.R,
				this.G,
				this.B
			});
		}
		
		//
		// Operators
		//
		public static bool operator == (Color left, Color right) {
			return left.Value == right.Value && left.IsSystemColor == right.IsSystemColor && left.IsEmpty == right.IsEmpty;
		}
		
		public static bool operator != (Color left, Color right) {
			return !(left == right);
		}
		
		//
		// Nested Types
		//
		[Flags]
		internal enum ColorType : short
		{
			Empty = 0,
			Known = 1,
			ARGB = 2,
			Named = 4,
			System = 8
		}
	}
}

