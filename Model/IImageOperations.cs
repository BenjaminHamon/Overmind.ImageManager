namespace Overmind.ImageManager.Model
{
	/// <summary>Operations related to images.</summary>
	public interface IImageOperations
	{
		/// <summary>Check if a byte array contains an image data.</summary>
		bool IsImage(byte[] data);

		/// <summary>Compute a hash from an image data.</summary>
		string ComputeHash(byte[] data);

		/// <summary>Get an image format from its data.</summary>
		string GetFormat(byte[] data);

		/// <summary>Get an image dimensions from its data.</summary>
		string GetDimensions(byte[] data);
	}
}
