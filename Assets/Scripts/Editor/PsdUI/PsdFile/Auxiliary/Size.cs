namespace PhotoshopFile.Auxiliary
{
	public struct Size
	{
		int width;
		int height;
		//
		// Static Fields
		//
		public static readonly Size Empty;

		//
		// Properties
		//
		public int Height {
			get {
				return this.height;
			}
			set {
				this.height = value;
			}
		}
			
		public bool IsEmpty {
			get {
				return this.width == 0 && this.height == 0;
			}
		}

		public int Width {
			get {
				return this.width;
			}
			set {
				this.width = value;
			}
		}

		//
		// Constructors
		//
		public Size (int width, int height)
		{
			this.width = width;
			this.height = height;
		}

		public Size (Point pt)
		{
			this.width = pt.X;
			this.height = pt.Y;
		}

		//
		// Static Methods
		//
		public static Size Add (Size sz1, Size sz2)
		{
			return new Size (sz1.Width + sz2.Width, sz1.Height + sz2.Height);
		}

		public static Size Subtract (Size sz1, Size sz2)
		{
			return new Size (sz1.Width - sz2.Width, sz1.Height - sz2.Height);
		}

		//
		// Methods
		//
		public override bool Equals (object obj)
		{
			return obj is Size && this == (Size)obj;
		}

		public override int GetHashCode ()
		{
			return this.width ^ this.height;
		}

		public override string ToString ()
		{
			return string.Format ("{{Width={0}, Height={1}}}", this.width, this.height);
		}

		//
		// Operators
		//
		public static Size operator + (Size sz1, Size sz2) {
			return new Size (sz1.Width + sz2.Width, sz1.Height + sz2.Height);
		}

		public static bool operator == (Size sz1, Size sz2) {
			return sz1.Width == sz2.Width && sz1.Height == sz2.Height;
		}

		public static explicit operator Point (Size size) {
			return new Point (size.Width, size.Height);
		}

		public static bool operator != (Size sz1, Size sz2) {
			return sz1.Width != sz2.Width || sz1.Height != sz2.Height;
		}

		public static Size operator - (Size sz1, Size sz2) {
			return new Size (sz1.Width - sz2.Width, sz1.Height - sz2.Height);
		}
	}
}

