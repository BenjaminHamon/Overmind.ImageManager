using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overmind.ImageManager.Model
{
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

		public string Format(ImageModel image, string fileExtension = null)
		{
			if (image == null)
				throw new ArgumentNullException("Image must not be null.");

			if (String.IsNullOrEmpty(fileExtension))
				fileExtension = Path.GetExtension(image.FileName);
			if (String.IsNullOrEmpty(fileExtension))
				throw new ArgumentException("File extension or an image file name with one is required");

			fileExtension = fileExtension.TrimStart('.');

			string titleElement = FormatElement(new List<string>() { image.Title }, TitleLengthLimit)
				?? FormatElement(image.SubjectCollection, TitleLengthLimit) ?? TitleDefaultValue;
			string artistElement = FormatElement(image.ArtistCollection, ArtistLengthLimit) ?? ArtistDefaultValue;
			List<string> allElements = new List<string>() { titleElement, artistElement, image.Hash };
			return String.Join(ElementSeparator, allElements) + "." + fileExtension;
		}

		public string FormatElement(IEnumerable<string> valueCollection, int lengthLimit)
		{
			if (valueCollection == null)
				throw new ArgumentNullException("Value collection must not be null.");
			if (lengthLimit < 0)
				throw new ArgumentException("Length limit must be positive.");
			if (lengthLimit < EllipsisMark.Length)
				throw new ArgumentException("Length limit is too short (smaller than the ellipsis mark).");

			string fullValue = String.Join(ValueSeparator, valueCollection.Select(FormatValue));
			if (String.IsNullOrEmpty(fullValue))
				return null;

			lengthLimit -= EllipsisMark.Length;

			string shortenedValue = fullValue;
			while ((shortenedValue.Length > lengthLimit) && (shortenedValue.Contains(ValueSeparator)))
				shortenedValue = shortenedValue.Substring(0, shortenedValue.LastIndexOf(ValueSeparator));
			while ((shortenedValue.Length > lengthLimit) && (shortenedValue.Contains(WordSeparator)))
				shortenedValue = shortenedValue.Substring(0, shortenedValue.LastIndexOf(WordSeparator));
			if (shortenedValue.Length > lengthLimit)
				shortenedValue = shortenedValue.Substring(0, lengthLimit);

			if (shortenedValue.Length < fullValue.Length)
				return shortenedValue + EllipsisMark;
			return shortenedValue;
		}

		public string FormatValue(string value)
		{
			if (value == null)
				return null;

			if (TextTransform != null)
				value = TextTransform(value);

			value = InvalidCharactersRegex.Replace(value, "");
			value = WhiteSpaceRegex.Replace(value, WordSeparator);
			return value.Trim();
		}
	}
}
