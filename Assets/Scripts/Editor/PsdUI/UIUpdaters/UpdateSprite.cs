using UnityEngine;
using PsdUI;

namespace PsdUI.UIUpdaters
{
	[UIUpdater (typeof (Sprite))]
	public class UpdateSprite : IUIUpdater
	{
		public void updateLayout (GameObject gameObject, PsdReader.PsdLayer layer)
		{

		}
	}
}
