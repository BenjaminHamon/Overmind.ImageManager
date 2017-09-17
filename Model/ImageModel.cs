using System;
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

		public static string CreateHash(byte[] imageData)
		{
			using (MD5 md5 = MD5.Create())
				return BitConverter.ToString(md5.ComputeHash(imageData)).Replace("-", "").ToLowerInvariant();
		}
	}
}
