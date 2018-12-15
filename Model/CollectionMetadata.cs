using System.Runtime.Serialization;

namespace Overmind.ImageManager.Model
{
	[DataContract]
	public class CollectionMetadata
	{
		[DataMember]
		public string FormatVersion { get; set; }
	}
}
