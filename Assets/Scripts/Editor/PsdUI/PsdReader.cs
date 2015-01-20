using System.Collections.Generic;
using System.IO;
using PhotoshopFile;
using PhotoshopFile.Auxiliary;

namespace PsdUI
{
	public class PsdReader
	{
		string _psdFileName;
		string _layersDirectory;

		PsdFile _psdFile;

		public bool hasChannelsData { get; private set; }

		public class PsdLayer
		{
			public string name;

			public Point position;
			public Size size;

			public bool visible;

			public bool isTextLayer;
			public string fontName;
			public float fontSize;
			public string text;
			public TextJustification justification;
			public Color fillColor;

			public List<PsdLayer> children;
		}

		public PsdLayer layersHierarchy {
			get {
				return createLayersHierarchy ();
			}
		}

		public PsdReader (string psdFileName, bool dontReadChannels = false)
		{
			hasChannelsData = !dontReadChannels;
			_psdFileName = psdFileName;
			_psdFile = new PsdFile (psdFileName, dontReadChannels);
		}

		static bool canExtract (Layer layer)
		{
			return !layer.IsPixelDataIrrelevant && layer.Channels.Count == 4 && !layer.IsTextLayer;
		}

		public List<string> extractLayers (string layersOutputDirectory)
		{
			var extractedFiles = new List<string> ();

			if (!hasChannelsData) {
				return extractedFiles;
			}

			Directory.CreateDirectory (layersOutputDirectory);
			
			foreach (var layer in _psdFile.Layers) {
				//skip group layers
				if (isLayerGroupEnd (layer) || isLayerGroupBegin (layer)) continue;

				var layerFileName = Path.Combine (layersOutputDirectory, PsdUtility.imageNameFromLayerName (layer.Name));

				//TODO: 
				if (extractedFiles.Contains (layerFileName)) continue;
				if (!canExtract (layer)) continue;
				
				var data = BitmapUtility.layerData32argb (layer);
				BitmapUtility.writeBitmapFile (layerFileName, layer.Rect.Width, layer.Rect.Height, data);

				extractedFiles.Add (layerFileName);
			}

			return extractedFiles;
		}

		PsdLayer createLayersHierarchy ()
		{
			var rootLayer = new PsdLayer {
				name = Path.GetFileNameWithoutExtension (_psdFileName),
				children = new List<PsdLayer> (),
				size = new Size (_psdFile.Width, _psdFile.Height)
			};

			var layerStack = new Stack<PsdLayer> ();

			layerStack.Push (rootLayer);

			var layers = new List<Layer> (_psdFile.Layers);
			layers.Reverse ();

			for (int i = 0; i < layers.Count; ++i) {
				var layer = layers[i];
				var currentRoot = layerStack.Peek ();

				if (isLayerGroupEnd (layer)) {
					currentRoot.children.Reverse ();
					layerStack.Pop ();
					continue;
				} else if (isLayerGroupBegin (layer)) {
					var layerGroup = new PsdLayer {
						name = layer.Name,
						visible = layer.Visible,
						children = new List<PsdLayer> ()
					};

					currentRoot.children.Add (layerGroup);

					layerStack.Push (layerGroup);
					continue;
				}


				var psdLayer = toPsdLayer (layer);
				currentRoot.children.Add (psdLayer);
			}

			rootLayer.children.Reverse ();

			return rootLayer;
		}

		static PsdLayer toPsdLayer (Layer layer)
		{
			var psdLayer = new PsdLayer {
				name = layer.Name,
				visible = layer.Visible,
				size = new Size (layer.Rect.Width, layer.Rect.Height),
				position = new Point (layer.Rect.X, layer.Rect.Y),
				isTextLayer = layer.IsTextLayer,
				text = layer.Text,
				fontName = layer.FontName,
				fontSize = layer.FontSize,
				justification = layer.Justification,
				fillColor = layer.FillColor
			};

			return psdLayer;
		}

		static bool isLayerGroupBegin (Layer layer)
		{
			//TODO: can't say for sure that is the begining of a group =(
			//return layer.IsPixelDataIrrelevant && layer.Rect.Height == 0;
			return layer.isFolder;
		}
		
		static bool isLayerGroupEnd (Layer layer)
		{
			return layer.Name.Contains("</Layer set>") ||
				layer.Name.Contains("</Layer group>") ||
					(layer.Name == " copy" && layer.Rect.Height == 0);
		}
	}
}
