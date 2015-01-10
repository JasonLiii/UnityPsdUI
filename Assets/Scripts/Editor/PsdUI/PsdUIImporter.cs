using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PsdUI 
{
	public class PsdUIImporter : AssetPostprocessor
	{
		const string PsdUiExtension = ".ui.psd";
		const string TargetPrefabFolder = "ui";
		const string TextureFolder = "layers";

		static Dictionary<string, PsdImportContext> _importContextList = new Dictionary<string, PsdImportContext> ();

		class PsdImportContext
		{
			public List<string> extractedFiles;
			public PsdReader.PsdLayer rootLayer;
		}

		bool shouldUpdateImportSettings (string assetPath)
		{
			foreach (var importContext in _importContextList) {
				if (importContext.Value.extractedFiles == null) {
					continue;
				}

				if (importContext.Value.extractedFiles.Contains (assetPath)) {
					return true;
				}
			}

			return false;
		}

	
		/// <summary>
		/// Raises the preprocess texture event.
		/// </summary>
		void OnPreprocessTexture ()
		{
			if (assetPath.EndsWith (PsdUiExtension)) {
				importPsdUi (assetPath);
			} else if (shouldUpdateImportSettings (assetPath)) {
				setSpriteMetaData (assetImporter as TextureImporter);
			}
		}

		static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			foreach (var assetPath in importedAssets) {
				if (assetPath.EndsWith (PsdUiExtension)) {
					createLayoutFromPsd (assetPath);
				}
			}

			//Clear import context list
			_importContextList = new Dictionary<string, PsdImportContext> ();
		}

		static void importPsdUi (string assetPath)
		{
			var psdReader = new PsdReader (assetPath);
			var assetDirectory = Path.GetDirectoryName (assetPath);
			var layersOutputDirectory = Path.Combine (assetDirectory, TextureFolder);
			
			var extractedFiles = psdReader.extractLayers (layersOutputDirectory);
			var rootLayer = psdReader.layersHierarchy;

			var importContext = new PsdImportContext {
				extractedFiles = extractedFiles,
				rootLayer = rootLayer
			};

			_importContextList[assetPath] = importContext;
			
			AssetDatabase.Refresh ();
		}

		static void createLayoutFromPsd (string assetPath)
		{
			PsdImportContext context;
			if (!_importContextList.TryGetValue (assetPath, out context)) {
				Debug.LogError (string.Format ("Can't find context for {0}!", assetPath));
				return;
			}

			var assetDirectory = Path.GetDirectoryName (assetPath);
			var layersOutputDirectory = Path.Combine (assetDirectory, TextureFolder);
			var uiFolder = Path.Combine (assetDirectory, "ui");
			var uiPrefabPath = Path.Combine (uiFolder, Path.GetFileNameWithoutExtension (assetPath) + ".prefab");

			var psdLayout = new PsdLayout (layersOutputDirectory);

			psdLayout.createOrUpdatePrefab (uiPrefabPath, context.rootLayer);
		}

		/// <summary>
		/// Configure importer for import texture as a sprite
		/// </summary>
		/// <param name="importer">Current texture importer instance</param>
		static void setSpriteMetaData (TextureImporter importer)
		{
			importer.textureType = TextureImporterType.Sprite;
			importer.maxTextureSize = 2048;
			importer.spriteImportMode = SpriteImportMode.Single;
			importer.spritePivot = new Vector2(0.5f, 0.5f);
			importer.mipmapEnabled = false;
		}
	}

}