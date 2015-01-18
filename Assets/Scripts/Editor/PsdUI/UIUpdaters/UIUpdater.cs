using System;

namespace PsdUI.UIUpdaters
{
	[AttributeUsage (AttributeTargets.Class)]
	public class UIUpdater : Attribute
	{
		public Type type;

		public UIUpdater (Type type)
		{
			this.type = type;
		}
	}
}