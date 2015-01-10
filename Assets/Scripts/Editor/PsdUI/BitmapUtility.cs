using UnityEngine;
using PhotoshopFile;
using System.IO;

public class BitmapUtility 
{
	public static byte[] layerData32argb (Layer layer)
	{
		var layerDataSize = 0;

		var channels = layer.Channels;
		var channelCount = channels.Count;
		var width = layer.Rect.Width;
		var height = layer.Rect.Height;
		
		for (int i = 0; i < channelCount; ++i) {
			var channel = layer.Channels [i];
			layerDataSize += channel.ImageData.Length;
		}
		
		var layerData = new byte[layerDataSize];

		//var channelDataSize = channels[0].ImageData.Length;
		
		for (int y = 0; y < height; ++y) {
			var rowOffset = (height - 1 - y) * width * channelCount;
			
			for (int x = 0; x < width; ++x) {
				for (int i = 0; i < channelCount; ++i) {
					var channel = channels [i];
					var layerDataAddress = rowOffset + x * channelCount + i;
					var channelDataAddress = y * width + x;
					
					layerData [layerDataAddress] = channel.ImageData[channelDataAddress];
				}
			}
		}
		
		return layerData;
	}

	public static Texture2D createTexture (int width, int height, byte[] imageData, TextureFormat format = TextureFormat.RGBA32)
	{
		var image = new Texture2D (width, height, format, false);
		image.LoadRawTextureData (imageData);

		return image;
	}

	public static void writeBitmapFile(string filename, int width, int height, byte[] imageData, TextureFormat format = TextureFormat.ARGB32)
	{
		var image = createTexture (width, height, imageData, format);

		var pngImage = image.EncodeToPNG ();

		GameObject.DestroyImmediate (image);

		File.WriteAllBytes (filename, pngImage);
	}
}
