using Overmind.ImageManager.Model;
using System;

namespace Overmind.ImageManager.WindowsClient
{
	public class ImageViewModel
	{
		public ImageViewModel(ImageModel model, Func<string> getImagePath)
		{
			this.model = model;
			this.getImagePath = getImagePath;
		}

		private readonly ImageModel model;
		private readonly Func<string> getImagePath;

		public ImageModel Model { get { return model; } }
		public string FilePath { get { return getImagePath(); } }
		public string Name { get { return model.FileName; } }
		public string Hash { get { return model.Hash; } }

		public string Group { get; set; }
	}
}
