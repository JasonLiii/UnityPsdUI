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

		public PsdLayout (string sourceImagesFolder)
		{
			_sourceImagesFolder = sourceImagesFolder;
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

			updateGameObject (rootGameObject, rootLayer, rootLayer.size);
			
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


		void updateGameObject (GameObject layerGameObject, PsdReader.PsdLayer layer, Size canvasSize)
		{
			var layerRectTransform = layerGameObject.GetComponent<RectTransform> ();

			var x = layer.position.X - canvasSize.Width / 2;
			var y = layer.position.Y + canvasSize.Height / 2;
			var width = layer.size.Width;
			var height = layer.size.Height;
			
			layerRectTransform.sizeDelta = new Vector2 (width, height);
			layerRectTransform.position = new Vector3 (x + width / 2, canvasSize.Height - y - height / 2, 0);

			if (layer.children != null) {
				foreach (var childLayer in layer.children) {
					var layerTransform = layerGameObject.transform.FindChild (childLayer.name);
					
					GameObject gameObject;
					
					if (layerTransform == null) {
						var layerFileName = Path.Combine (_sourceImagesFolder, PsdUtility.imageNameFromLayerName (childLayer.name));

						if (childLayer.children == null) {
							gameObject = createImageFromFile (childLayer.name, layerFileName);
						} else {
							gameObject = new GameObject (childLayer.name, typeof (RectTransform));
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
