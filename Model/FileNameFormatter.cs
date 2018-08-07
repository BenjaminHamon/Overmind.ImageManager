using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overmind.ImageManager.Model
{
	public class FileNameFormatter
	{
		public string AllowedAlphabet = "a-zA-Z0-9";
		public string WordSeparator { get; set; } = " ";
		public string ValueSeparator { get; set; } = ", ";
		public string ElementSeparator { get; set; } = " - ";
		public string EllipsisMark { get; set; } = " ...";
		public string TitleDefaultValue { get; set; } = "NoTitle";
		public string ArtistDefaultValue { get; set; } = "NoArtist";
		public int TitleSizeLimit { get; set; } = 50;
		public int ArtistSizeLimit { get; set; } = 30;
		public Func<string, string> TextTransform { get; set; }

		public string Format(ImageModel image)
		{
			string titleElement = FormatElement(new List<string>() { image.Title }, TitleSizeLimit)
				?? FormatElement(image.SubjectCollection, TitleSizeLimit) ?? TitleDefaultValue;
			string artistElement = FormatElement(image.ArtistCollection, ArtistSizeLimit) ?? ArtistDefaultValue;
			List<string> allElements = new List<string>() { titleElement, artistElement, image.Hash };
			return String.Join(ElementSeparator, allElements) + Path.GetExtension(image.FileName);
		}

		private string FormatElement(IEnumerable<string> valueCollection, int sizeLimit)
		{
			string fullValue = String.Join(ValueSeparator, valueCollection.Select(FormatValue));
			if (String.IsNullOrEmpty(fullValue))
				return null;

			string shortenedValue = fullValue;
			while ((shortenedValue.Length > sizeLimit) && (shortenedValue.Contains(",")))
				shortenedValue = shortenedValue.Substring(0, shortenedValue.LastIndexOf(","));
			while ((shortenedValue.Length > sizeLimit) && (shortenedValue.Contains(" ")))
				shortenedValue = shortenedValue.Substring(0, shortenedValue.LastIndexOf(" "));
			if (shortenedValue.Length > sizeLimit)
				shortenedValue = shortenedValue.Substring(0, sizeLimit);

			if (shortenedValue.Length < fullValue.Length)
				return shortenedValue + EllipsisMark;
			return shortenedValue;
		}

		private string FormatValue(string value)
		{
			if (value == null)
				return null;

			if (TextTransform != null)
				value = TextTransform(value);

			value = Regex.Replace(value, "[^" + AllowedAlphabet + "]", WordSeparator);
			value = Regex.Replace(value, WordSeparator + "+", WordSeparator);
			return value.Trim();
		}
	}
}
