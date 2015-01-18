using PsdUI;
using UnityEngine;

namespace PsdUI.UIUpdaters
{
	interface IUIUpdater
	{
		void updateLayout (GameObject gameObject, PsdReader.PsdLayer layer);
	}
}
