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
		public string FileNameInStorage { get; set; }
		[DataMember]
		public string Hash { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string Title { get; set; }
		[DataMember]
		public List<string> TagCollection { get; set; } = new List<string>();

		public static string CreateHash(byte[] imageData)
		{
			using (MD5 md5 = MD5.Create())
				return BitConverter.ToString(md5.ComputeHash(imageData)).Replace("-", "").ToLowerInvariant();
		}

		public IEnumerable<string> GetSearchableValues()
		{
			if (String.IsNullOrEmpty(Title) == false)
				yield return Title;
			foreach (string tag in TagCollection)
				yield return tag;
		}
		
		public Document ToDocument()
		{
			Document document = new Document();
			document.Add(new Field("title", Title ?? "", Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field("tag", String.Join(";", TagCollection), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field("hash", Hash, Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field("any", String.Join(";", GetSearchableValues()), Field.Store.YES, Field.Index.ANALYZED));
			return document;
		}
	}
}
