using System;
using System.Runtime.Serialization;

namespace Overmind.ImageManager.Model
{
	[DataContract]
	public class ImageSource
	{
		[DataMember]
		public Uri Uri { get; set; }
	}
}
