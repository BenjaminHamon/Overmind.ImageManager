using Overmind.ImageManager.Model;
using System;
using System.IO;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient
{
	public class ImageViewModel
	{
		public ImageViewModel(ImageModel model, Func<string> getStoragePath)
		{
			this.model = model;
			this.getStoragePath = getStoragePath;
		}

		private readonly ImageModel model;
		private readonly Func<string> getStoragePath;
		
		public string Name { get { return model.FileName; } }
		public string FilePath { get { return Path.Combine(getStoragePath(), model.FileName); } }
		public string Hash { get { return model.Hash; } }

		public string Title { get { return model.Title; } set { model.Title = value; } }

		public string TagCollection
		{
			get { return String.Join(" ", model.Tags); }
			set { model.Tags = value.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
		}
	}
}
