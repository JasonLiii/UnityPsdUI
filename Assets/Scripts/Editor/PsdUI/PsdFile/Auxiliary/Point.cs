using System;
using System.Globalization;

namespace PhotoshopFile.Auxiliary
{
	public struct Point
	{
		int x;
		int y;
		//
		// Static Fields
		//
		public static readonly Point Empty;

		//
		// Properties
		//
		public bool IsEmpty {
			get {
				return this.x == 0 && this.y == 0;
			}
		}

		public int X {
			get {
				return this.x;
			}
			set {
				this.x = value;
			}
		}

		public int Y {
			get {
				return this.y;
			}
			set {
				this.y = value;
			}
		}

		//
		// Constructors
		//
		public Point (int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public Point (Size sz)
		{
			this.x = sz.Width;
			this.y = sz.Height;
		}

		public Point (int dw)
		{
			this.y = dw >> 16;
			this.x = (dw & 65535);
		}

		//
		// Static Methods
		//
		public static Point Add (Point pt, Size sz)
		{
			return new Point (pt.X + sz.Width, pt.Y + sz.Height);
		}

		public static Point Subtract (Point pt, Size sz)
		{
			return new Point (pt.X - sz.Width, pt.Y - sz.Height);
		}

		//
		// Methods
		//
		public override bool Equals (object obj)
		{
			return obj is Point && this == (Point)obj;
		}

		public override int GetHashCode ()
		{
			return this.x ^ this.y;
		}

		public void Offset (Point p)
		{
			this.Offset (p.X, p.Y);
		}

		public void Offset (int dx, int dy)
		{
			this.x += dx;
			this.y += dy;
		}

		public override string ToString ()
		{
			return string.Format ("{{X={0},Y={1}}}", this.x.ToString (CultureInfo.InvariantCulture), this.y.ToString (CultureInfo.InvariantCulture));
		}

		//
		// Operators
		//
		public static Point operator + (Point pt, Size sz) {
			return new Point (pt.X + sz.Width, pt.Y + sz.Height);
		}

		public static bool operator == (Point left, Point right) {
			return left.X == right.X && left.Y == right.Y;
		}

		public static explicit operator Size (Point p) {
			return new Size (p.X, p.Y);
		}

		public static bool operator != (Point left, Point right) {
			return left.X != right.X || left.Y != right.Y;
		}

		public static Point operator - (Point pt, Size sz) {
			return new Point (pt.X - sz.Width, pt.Y - sz.Height);
		}
	}
}

