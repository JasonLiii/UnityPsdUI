namespace PsdUI
{
	public static class PsdUtility
	{
		public static string imageNameFromLayerName (string layerName)
		{
			var groupIndex = layerName.IndexOf (':');
			if (groupIndex != -1) {
				layerName = layerName.Substring (0, groupIndex);
			}
			return layerName + ".png";
		}
	}
}
