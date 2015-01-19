using System.IO;
using UnityEditor;
using UnityEngine;
using PhotoshopFile.Auxiliary;
using UnityEngine.UI;

namespace PsdUI
{
	public class PsdLayout
	{
		string _sourceImagesFolder;
		string _fontsFolder;

		public PsdLayout (string sourceImagesFolder, string fontsFolder)
		{
			_sourceImagesFolder = sourceImagesFolder;
			_fontsFolder = fontsFolder;
		}

		static PsdReader.PsdLayer skipFirstLayer (PsdReader.PsdLayer layer)
		{
			if (layer.children.Count == 1 && layer.children[0].children != null) {
				return layer.children[0];
			}

			return layer;
		}

		public void createOrUpdatePrefab (string prefabFileName, PsdReader.PsdLayer rootLayer)
		{
			var prefabDirectory = Path.GetDirectoryName (prefabFileName);
			Directory.CreateDirectory (prefabDirectory);
			
			GameObject rootGameObjectPrefab = AssetDatabase.LoadAssetAtPath (prefabFileName, typeof (GameObject)) as GameObject;
			
			bool createNew = false;
			
			GameObject rootGameObject;
			
			if (rootGameObjectPrefab == null) {
				rootGameObject = new GameObject (rootLayer.name, typeof (RectTransform));
				createNew = true;
			} else {
				rootGameObject = GameObject.Instantiate (rootGameObjectPrefab) as GameObject;
			}

			var layers = skipFirstLayer (rootLayer);
			updateGameObject (rootGameObject, layers, rootLayer.size);
			
			if (createNew) {
				PrefabUtility.CreatePrefab (prefabFileName, rootGameObject);
			} else {
				var existingPrefab = PrefabUtility.GetPrefabObject (rootGameObjectPrefab);
				PrefabUtility.ReplacePrefab (rootGameObject, existingPrefab, ReplacePrefabOptions.ReplaceNameBased);
			}
			
			GameObject.DestroyImmediate (rootGameObject);
		}

		static Vector2 pointToVector2 (Point point)
		{
			return new Vector2 (point.X, point.Y);
		}

		static Vector3 pointToVector3 (Point point, float z = 0.0f)
		{
			return new Vector3 (point.X, point.Y, z);
		}

		static string cleanLayerName (string sourceLayerName)
		{
			return Path.GetFileNameWithoutExtension (sourceLayerName);
		}

		static bool isGroup (PsdReader.PsdLayer layer)
		{
			return layer.children != null;
		}

		static bool isText (PsdReader.PsdLayer layer)
		{
			return layer.isTextLayer;
		}

		static bool shouldDigDeeper (PsdReader.PsdLayer layer)
		{
			return !layer.name.EndsWith (".png");
		}

		//TODO: give the method a proper name
		static float toFloat (byte b)
		{
			return ((float)b) / 255;
		}

		static UnityEngine.Color convertColorToUnity (PhotoshopFile.Auxiliary.Color color)
		{
			return new UnityEngine.Color (toFloat (color.R),
			                              toFloat (color.G),
			                              toFloat (color.B),
			                              toFloat (color.A));
		}

		/// <summary>
		/// Creates an image from texture file.
		/// </summary>
		/// <returns>The newly created image game object.</returns>
		/// <param name="layerName">Layer name. Used to set the game object name.</param>
		/// <param name="fileName">Texture file name.</param>
		static GameObject createImageFromFile (string layerName, string fileName)
		{
			var gameObject = new GameObject (layerName, typeof (RectTransform), typeof (Image));
			var image = gameObject.GetComponent<Image> ();
			
			var layerSprite = (Sprite) Resources.LoadAssetAtPath (fileName, typeof(Sprite));
			
			if (layerSprite != null) {
				image.sprite = layerSprite;
			}
			
			return gameObject;
		}

		static bool shouldIgnoreLayer (string layerName)
		{
			return layerName.StartsWith ("-");
		}

		GameObject createText (string layerName, string fontName, float fontSize, UnityEngine.Color fontColor, string textString)
		{
			var gameObject = new GameObject (layerName, typeof (RectTransform), typeof (Text));
			var text = gameObject.GetComponent<Text> ();

			text.text = textString;
			text.fontSize = (int)fontSize;
			text.verticalOverflow = VerticalWrapMode.Overflow;
			text.horizontalOverflow = HorizontalWrapMode.Overflow;
			text.color = fontColor;

			text.font = findFontByName (fontName);

			return gameObject;
		}

		static bool isFontLike (string[] fontNames, string fontNameToCompare)
		{
			foreach (var fontName in fontNames) {
				var name = fontName.Replace (" ", "");
				if (name.Contains (fontNameToCompare)) {
					return true;
				}
			}

			return false;
		}

		Font findFontByName (string fontName)
		{
			var fonts = Directory.GetFiles (_fontsFolder, "*.ttf");

			foreach (var fontFileName in fonts) {
				var fontFilePath = Path.Combine (_fontsFolder, Path.GetFileName (fontFileName));
				var font = Resources.LoadAssetAtPath<Font> (fontFilePath);

				if (isFontLike (font.fontNames, fontName)) {
					return font;
				}

				Resources.UnloadAsset (font);
			}

			return null;
		}

		void updateGameObject (GameObject layerGameObject, PsdReader.PsdLayer layer, Size canvasSize)
		{
			var layerRectTransform = layerGameObject.GetComponent<RectTransform> ();

			var x = layer.position.X - canvasSize.Width / 2;
			var y = layer.position.Y + canvasSize.Height / 2;
			var width = layer.size.Width;
			var height = layer.size.Height;
			
			layerRectTransform.sizeDelta = new Vector2 (width, height);
			layerRectTransform.position = new Vector3 (x + width / 2, canvasSize.Height - y - height / 2, 0);

			//TODO: add updaters for different UI objects

			if (isGroup (layer) && shouldDigDeeper (layer)) {
				foreach (var childLayer in layer.children) {
					if (shouldIgnoreLayer (childLayer.name)) continue;

					var layerName = cleanLayerName (childLayer.name);
					var layerTransform = layerGameObject.transform.FindChild (layerName);
					
					GameObject gameObject;
					
					if (layerTransform == null) {
						var layerFileName = Path.Combine (_sourceImagesFolder, PsdUtility.imageNameFromLayerName (layerName));

						if (isGroup (childLayer)) {
							gameObject = new GameObject (layerName, typeof (RectTransform));
						} else if (isText (childLayer)) {
							gameObject = createText (layerName, childLayer.fontName,
							                         childLayer.fontSize,
							                         convertColorToUnity (childLayer.fillColor),
							                         childLayer.text);
						} else {
							gameObject = createImageFromFile (layerName, layerFileName);
						}
						var rectTransform = gameObject.GetComponent<RectTransform> ();
						rectTransform.SetParent (layerGameObject.transform);
					} else {
						gameObject = layerTransform.gameObject;
					}

					updateGameObject (gameObject, childLayer, canvasSize);
				}
			}
		}
	}
}
