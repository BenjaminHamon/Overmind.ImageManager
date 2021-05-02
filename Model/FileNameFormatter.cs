using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overmind.ImageManager.Model
{
	/// <summary>Formatter to generate valid and pretty file names for images.</summary>
	public class FileNameFormatter
	{
		private static readonly Regex InvalidCharactersRegex = new Regex("[" + new string(Path.GetInvalidFileNameChars()) + "]", RegexOptions.Compiled);
		private static readonly Regex WhiteSpaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

		public string WordSeparator { get; set; } = " ";
		public string ValueSeparator { get; set; } = ", ";
		public string ElementSeparator { get; set; } = " - ";
		public string EllipsisMark { get; set; } = "...";
		public string TitleDefaultValue { get; set; } = "NoTitle";
		public string ArtistDefaultValue { get; set; } = "NoArtist";
		public int TitleLengthLimit { get; set; } = 50;
		public int ArtistLengthLimit { get; set; } = 30;
		public Func<string, string> TextTransform { get; set; }

		/// <summary>
		/// Generates a file name for an image object, based on its properties and file extension.
		/// The title (or subjects) and artists are formatted as elements, then joined with the hash.
		/// </summary>
		/// <param name="image">The image object for which to generate a file name.</param>
		/// <param name="fileExtension">The image file extension (by default it is retrieved from the current image file name).</param>
		/// <returns>The generated file name.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the image is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the file extension was not provided and cannot be determined.</exception>
		public string Format(ImageModel image, string fileExtension = null)
		{
			if (image == null)
				throw new ArgumentNullException(nameof(image), "Image must not be null.");

			if (String.IsNullOrEmpty(fileExtension))
				fileExtension = Path.GetExtension(image.FileName);
			if (String.IsNullOrEmpty(fileExtension))
				throw new ArgumentException("File extension or an image file name with one is required.");

			fileExtension = fileExtension.TrimStart('.');

			string titleElement = FormatElement(new List<string>() { image.Title }, TitleLengthLimit)
				?? FormatElement(image.SubjectCollection, TitleLengthLimit) ?? TitleDefaultValue;
			string artistElement = FormatElement(image.ArtistCollection, ArtistLengthLimit) ?? ArtistDefaultValue;
			List<string> allElements = new List<string>() { titleElement, artistElement, image.Hash };
			return String.Join(ElementSeparator, allElements) + "." + fileExtension;
		}

		/// <summary>
		/// Format a value collection to a valid string to be part of a file name.
		/// Individual values are formatted then joined and the result may be cropped and appended with a ellipsis mark.
		/// </summary>
		/// <param name="valueCollection">The values to format.</param>
		/// <param name="lengthLimit">The maximal length for the result, including the ellipsis mark.</param>
		/// <returns>The formatted string.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the value collection is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the length limit is negative or too low.</exception>
		public string FormatElement(IEnumerable<string> valueCollection, int lengthLimit)
		{
			if (valueCollection == null)
				throw new ArgumentNullException(nameof(valueCollection), "Value collection must not be null.");
			if (lengthLimit < 0)
				throw new ArgumentOutOfRangeException("Length limit must be positive.");
			if (lengthLimit < EllipsisMark.Length)
				throw new ArgumentOutOfRangeException("Length limit is too low (smaller than the ellipsis mark).");

			string fullValue = String.Join(ValueSeparator, valueCollection.Select(FormatValue));
			if (String.IsNullOrEmpty(fullValue))
				return null;

			lengthLimit -= EllipsisMark.Length;

			string shortenedValue = fullValue;
			while ((shortenedValue.Length > lengthLimit) && shortenedValue.Contains(ValueSeparator))
				shortenedValue = shortenedValue.Substring(0, shortenedValue.LastIndexOf(ValueSeparator));
			while ((shortenedValue.Length > lengthLimit) && shortenedValue.Contains(WordSeparator))
				shortenedValue = shortenedValue.Substring(0, shortenedValue.LastIndexOf(WordSeparator));
			if (shortenedValue.Length > lengthLimit)
				shortenedValue = shortenedValue.Substring(0, lengthLimit);

			if (shortenedValue.Length < fullValue.Length)
				return shortenedValue + EllipsisMark;
			return shortenedValue;
		}

		/// <summary>
		/// Format a single text value to a valid string to be part of a file name.
		/// Invalid characters are removed and white spaces are collapsed.
		/// </summary>
		/// <param name="value">The value to format.</param>
		/// <returns>The formatted value.</returns>
		public string FormatValue(string value)
		{
			if (value == null)
				return null;

			if (TextTransform != null)
				value = TextTransform(value);

			value = InvalidCharactersRegex.Replace(value, String.Empty);
			value = WhiteSpaceRegex.Replace(value, WordSeparator);
			return value.Trim();
		}
	}
}
