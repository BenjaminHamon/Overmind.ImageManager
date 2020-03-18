using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

namespace Overmind.ImageManager.Model
{
	/// <summary>Operations related to images.</summary>
	public class ImageOperations : IImageOperations
	{
		public ImageOperations(HashAlgorithm hashAlgorithm)
		{
			this.hashAlgorithm = hashAlgorithm;
		}

		private readonly HashAlgorithm hashAlgorithm;

		/// <summary>Check if a byte array contains an image data.</summary>
		public bool IsImage(byte[] data)
		{
			try
			{
				using (MemoryStream stream = new MemoryStream(data))
				using (Image.FromStream(stream))
					return true;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}

		/// <summary>Compute a hash from an image data.</summary>
		public string ComputeHash(byte[] data)
		{
			return BitConverter.ToString(hashAlgorithm.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
		}

		/// <summary>Get an image format from its data</summary>
		public string GetFormat(byte[] data)
		{
			using (MemoryStream stream = new MemoryStream(data))
			using (Image image = Image.FromStream(stream))
				return new ImageFormatConverter().ConvertToString(image.RawFormat).ToLower();
		}

		/// <summary>Get an image dimensions from its data</summary>
		public string GetDimensions(byte[] data)
		{
			using (MemoryStream stream = new MemoryStream(data))
			using (Image image = Image.FromStream(stream))
				return image.Width + "x" + image.Height;
		}
	}
}
