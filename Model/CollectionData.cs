using System.Collections.Generic;

namespace Overmind.ImageManager.Model
{
	public class CollectionData
	{
		public CollectionMetadata Metadata { get; set; } = new CollectionMetadata();
		public List<ImageModel> Images { get; set; } = new List<ImageModel>();
	}
}
