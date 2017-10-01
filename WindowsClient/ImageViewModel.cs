using Overmind.ImageManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;

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
		
		public string Name { get { return model.FileName; } }
		public string FilePath { get { return getImagePath(); } }
		public string Hash { get { return model.Hash; } }

		public string Title { get { return model.Title; } set { model.Title = value; } }

		public string TagCollection
		{
			get { return String.Join(" ", model.TagCollection); }
			set { model.TagCollection = value.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
		}

		public bool IsSearchMatch(Func<ImageModel, bool> queryFunction)
		{
			return queryFunction(model);
		}
	}
}
