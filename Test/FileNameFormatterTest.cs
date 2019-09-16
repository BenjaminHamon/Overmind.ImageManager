using Microsoft.VisualStudio.TestTools.UnitTesting;
using Overmind.ImageManager.Model;
using System.Collections.Generic;

namespace Overmind.ImageManager.Test
{
	[TestClass]
	public class FileNameFormatterTest
	{
		[TestMethod]
		public void FileNameFormatter_Format_Default()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			ImageModel image = new ImageModel() { Hash = "0800fc577294c34e0b28ad2839435945" };

			string expectedResult = "NoTitle - NoArtist - 0800fc577294c34e0b28ad2839435945.jpeg";
			string actualResult = fileNameFormatter.Format(image, "jpeg");

			Assert.IsTrue(actualResult.Length < 125);
			Assert.AreEqual(actualResult, expectedResult);
		}

		[TestMethod]
		public void FileNameFormatter_Format_WithData()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			ImageModel image = new ImageModel() { Hash = "0800fc577294c34e0b28ad2839435945" };
			image.SubjectCollection = new List<string>() { "Subject" };
			image.ArtistCollection = new List<string>() { "Artist" };

			string expectedResult = "Subject - Artist - 0800fc577294c34e0b28ad2839435945.jpeg";
			string actualResult = fileNameFormatter.Format(image, "jpeg");

			Assert.IsTrue(actualResult.Length < 125);
			Assert.AreEqual(actualResult, expectedResult);
		}

		[TestMethod]
		public void FileNameFormatter_Format_WithLargeData()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			ImageModel image = new ImageModel() { Hash = "0800fc577294c34e0b28ad2839435945" };
			image.SubjectCollection = new List<string>() { "First Subject", "Second Subject", "Third Subject", "Fourth Subject", "Fifth Subject" };
			image.ArtistCollection = new List<string>() { "First Artist", "Second Artist", "Third Artist" };

			string expectedResult = "First Subject, Second Subject, Third Subject... - First Artist, Second Artist... - 0800fc577294c34e0b28ad2839435945.jpeg";
			string actualResult = fileNameFormatter.Format(image, "jpeg");

			Assert.IsTrue(actualResult.Length < 125);
			Assert.AreEqual(actualResult, expectedResult);
		}

		[TestMethod]
		public void FileNameFormatter_FormatElement_Empty()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			Assert.IsNull(fileNameFormatter.FormatElement(new List<string>(), 10));
		}

		[TestMethod]
		public void FileNameFormatter_FormatElement_SingleValueAndShort()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			List<string> valueCollection = new List<string>() { "Short" };

			string expectedResult = "Short";
			string actualResult = fileNameFormatter.FormatElement(valueCollection, 10);

			Assert.IsTrue(actualResult.Length <= 10);
			Assert.AreEqual(actualResult, expectedResult);
		}

		[TestMethod]
		public void FileNameFormatter_FormatElement_SingleValueAndLong()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			List<string> valueCollection = new List<string>() { "VeryLongWord" };

			string expectedResult = "VeryLon...";
			string actualResult = fileNameFormatter.FormatElement(valueCollection, 10);

			Assert.IsTrue(actualResult.Length <= 10);
			Assert.AreEqual(actualResult, expectedResult);
		}

		[TestMethod]
		public void FileNameFormatter_FormatElement_SingleValueAndSeveralWords()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			List<string> valueCollection = new List<string>() { "First Second Third" };

			string expectedResult = "First...";
			string actualResult = fileNameFormatter.FormatElement(valueCollection, 10);

			Assert.IsTrue(actualResult.Length <= 10);
			Assert.AreEqual(actualResult, expectedResult);
		}

		[TestMethod]
		public void FileNameFormatter_FormatElement_SeveralValuesAndShort()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			List<string> valueCollection = new List<string>() { "First", "Second", "Third" };

			string expectedResult = "First, Second, Third";
			string actualResult = fileNameFormatter.FormatElement(valueCollection, 30);

			Assert.IsTrue(actualResult.Length <= 30);
			Assert.AreEqual(actualResult, expectedResult);
		}

		[TestMethod]
		public void FileNameFormatter_FormatElement_SeveralValuesAndLong()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			List<string> valueCollection = new List<string>() { "VeryLongWord", "VeryLongWord", "VeryLongWord" };

			string expectedResult = "VeryLongWord, VeryLongWord...";
			string actualResult = fileNameFormatter.FormatElement(valueCollection, 30);

			Assert.IsTrue(actualResult.Length <= 30);
			Assert.AreEqual(actualResult, expectedResult);
		}

		[TestMethod]
		public void FileNameFormatter_FormatValue_Null()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			Assert.IsNull(fileNameFormatter.FormatValue(null));
		}

		[TestMethod]
		public void FileNameFormatter_FormatValue_Ascii()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			Assert.AreEqual(fileNameFormatter.FormatValue("Abcde"), "Abcde");
		}

		[TestMethod]
		public void FileNameFormatter_FormatValue_Unicode()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			Assert.AreEqual(fileNameFormatter.FormatValue("English"), "English");
			Assert.AreEqual(fileNameFormatter.FormatValue("Français"), "Français");
			Assert.AreEqual(fileNameFormatter.FormatValue("русский язык"), "русский язык"); // Russian
			Assert.AreEqual(fileNameFormatter.FormatValue("汉语"), "汉语"); // Simplified chinese
			Assert.AreEqual(fileNameFormatter.FormatValue("漢語"), "漢語"); // Traditional chinese
			Assert.AreEqual(fileNameFormatter.FormatValue("日本語"), "日本語"); // Japanese
		}

		[TestMethod]
		public void FileNameFormatter_FormatValue_InvalidCharacters()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			Assert.AreEqual(fileNameFormatter.FormatValue("A:/*Z"), "AZ");
		}

		[TestMethod]
		public void FileNameFormatter_FormatValue_WhiteSpace()
		{
			FileNameFormatter fileNameFormatter = new FileNameFormatter();
			Assert.AreEqual(fileNameFormatter.FormatValue(" A   B \t  C \t"), "A B C");
		}
	}
}
