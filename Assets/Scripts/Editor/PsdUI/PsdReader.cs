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

		public class PsdLayer
		{
			public string name;

			public Point position;
			public Size size;

			public List<PsdLayer> children;
		}

		public PsdLayer layersHierarchy {
			get {
				return createLayersHierarchy ();
			}
		}

		public PsdReader (string psdFileName)
		{
			_psdFileName = psdFileName;
			_psdFile = new PsdFile (psdFileName);
		}

		public List<string> extractLayers (string layersOutputDirectory)
		{
			var extractedFiles = new List<string> ();

			Directory.CreateDirectory (layersOutputDirectory);
			
			foreach (var layer in _psdFile.Layers) {
				//skip group layers
				if (isLayerGroupEnd (layer) || isLayerGroupBegin (layer)) continue;

				var layerFileName = Path.Combine (layersOutputDirectory, PsdUtility.imageNameFromLayerName (layer.Name));

				//TODO: 
				if (extractedFiles.Contains (layerFileName)) continue;
				
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
						children = new List<PsdLayer> ()
					};

					currentRoot.children.Add (layerGroup);

					layerStack.Push (layerGroup);
					continue;
				} else if (!layer.Visible) {
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
				size = new Size (layer.Rect.Width, layer.Rect.Height),
				position = new Point (layer.Rect.X, layer.Rect.Y)
			};

			return psdLayer;
		}

		static bool isLayerGroupBegin (Layer layer)
		{
			return layer.IsPixelDataIrrelevant;
		}
		
		static bool isLayerGroupEnd (Layer layer)
		{
			return layer.Name.Contains("</Layer set>") ||
				layer.Name.Contains("</Layer group>") ||
					(layer.Name == " copy" && layer.Rect.Height == 0);
		}
	}
}
