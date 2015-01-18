using UnityEditor;
using UnityEngine;
using System;

namespace PsdUI
{
	[CustomEditor(typeof(TextureImporter))]
	public class PsdUIInspector : Editor
	{
		/// <summary>
		/// The native Unity editor used to render the <see cref="TextureImporter"/>'s Inspector.
		/// </summary>
		Editor nativeEditor;

		/// <summary>
		/// The style used to draw the section header text.
		/// </summary>
		GUIStyle guiStyle;

		public void OnEnable()
		{
			// use reflection to get the default Inspector
			Type type = Type.GetType("UnityEditor.TextureImporterInspector, UnityEditor");
			nativeEditor = CreateEditor(target, type);
			
			// set up the GUI style for the section headers
			guiStyle = new GUIStyle();
			guiStyle.richText = true;
			guiStyle.fontSize = 14;
			guiStyle.normal.textColor = Color.black;
			
			if (Application.HasProLicense()) {
				guiStyle.normal.textColor = Color.white;
			}
		}

		public override void OnInspectorGUI()
		{
			if (nativeEditor == null) {
				return;
			}
			var textureImporter = (TextureImporter)target;

			// check if it is a PSD file selected
			string assetPath = textureImporter.assetPath;
			
			if (!assetPath.EndsWith(".psd")) {
				nativeEditor.OnInspectorGUI();
				return;
			}

			var settings = readSettings (textureImporter);

			GUILayout.Label("<b>PSD</b>", guiStyle, GUILayout.Height(23));

			var createLayoutNewValue = GUILayout.Toggle (settings.createLayout, "Create layout");

			settings.createLayout = createLayoutNewValue;
			

			nativeEditor.OnInspectorGUI();

			saveSettings (textureImporter, settings);

			if (settings.createLayout != createLayoutNewValue) {
				AssetDatabase.ImportAsset (assetPath);
			}
		}

		PsdImporterSettings readSettings (TextureImporter importer)
		{
			var settings = new PsdImporterSettings ();

			settings.createLayout = importer.userData == "1";

			return settings;
		}

		void saveSettings (TextureImporter importer, PsdImporterSettings settings)
		{
			importer.userData = settings.createLayout ? "1" : "0";
		}
	}
}
