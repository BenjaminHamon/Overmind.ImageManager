using System;
using System.Runtime.Serialization;

namespace Overmind.ImageManager.Model
{
	[DataContract]
	public class ImageSource
	{
		[DataMember]
		public Uri Uri { get; set; }
		[DataMember]
		public string FileName { get; set; }
		[DataMember]
		public string Title { get; set; }
		[DataMember]
		public string Hash { get; set; }
	}
}
