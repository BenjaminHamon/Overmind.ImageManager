using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace Overmind.ImageManager.Model
{
	[DataContract]
	public class ImageModel
	{
		[DataMember]
		public string FileName { get; set; }

		public string FileNameAsTemporary { get; set; }
		public string FileNameAsSaved { get; set; }

		[DataMember]
		public string Hash { get; set; }

		[DataMember]
		public string Title { get; set; }
		[DataMember]
		public List<string> SubjectCollection { get; set; } = new List<string>();
		[DataMember]
		public List<string> ArtistCollection { get; set; } = new List<string>();
		[DataMember]
		public List<string> TagCollection { get; set; } = new List<string>();

		[DataMember]
		public int Score { get; set; }
		[DataMember]
		public DateTime AdditionDate { get; set; }
		[DataMember]
		public Uri Source { get; set; }

		public static string CreateHash(byte[] imageData)
		{
			using (MD5 md5 = MD5.Create())
				return BitConverter.ToString(md5.ComputeHash(imageData)).Replace("-", "").ToLowerInvariant();
		}

		public IEnumerable<string> GetSearchableValues()
		{
			if (String.IsNullOrEmpty(Title) == false)
				yield return Title;
			foreach (string subject in SubjectCollection)
				yield return subject;
			foreach (string artist in ArtistCollection)
				yield return artist;
			foreach (string tag in TagCollection)
				yield return tag;
		}

		public Document ToDocument()
		{
			Document document = new Document();

			document.Add(new Field("hash", Hash, Field.Store.YES, Field.Index.NOT_ANALYZED));

			document.Add(new Field("title", Title ?? "", Field.Store.NO, Field.Index.ANALYZED));
			foreach (string subject in SubjectCollection)
				document.Add(new Field("subject", subject, Field.Store.NO, Field.Index.ANALYZED));
			foreach (string artist in ArtistCollection)
				document.Add(new Field("artist", artist, Field.Store.NO, Field.Index.ANALYZED));
			foreach (string tag in TagCollection)
				document.Add(new Field("tag", tag, Field.Store.NO, Field.Index.ANALYZED));

			document.Add(new NumericField("score", Field.Store.NO, true).SetIntValue(Score));
			document.Add(new Field("date", DateTools.DateToString(AdditionDate, DateTools.Resolution.DAY), Field.Store.NO, Field.Index.ANALYZED));

			foreach (string searchableValue in GetSearchableValues())
				document.Add(new Field("any", searchableValue, Field.Store.NO, Field.Index.ANALYZED));

			return document;
		}
	}
}
