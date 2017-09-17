using Overmind.ImageManager.Model;

namespace Overmind.ImageManager.WindowsClient
{
	public class ImageViewModel
	{
		public ImageViewModel(ImageModel model)
		{
			this.model = model;
		}

		private readonly ImageModel model;
		
		public string Name { get { return model.FileName; } }
		public string Hash { get { return model.Hash; } }
	}
}
