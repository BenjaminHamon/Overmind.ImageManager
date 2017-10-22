using Overmind.ImageManager.Model;
using Overmind.WpfExtensions;
using System;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient
{
	public class ImageViewModel
	{
		public ImageViewModel(ImageModel model, Func<string> getImagePath)
		{
			this.model = model;
			this.getImagePath = getImagePath;

			ViewCommand = new DelegateCommand<object>(_ => WindowsApplication.ViewImage(this));
		}

		private readonly ImageModel model;
		private readonly Func<string> getImagePath;
		
		public ImageModel Model { get { return model; } }
		public string Name { get { return model.FileName; } }
		public string FilePath { get { return getImagePath(); } }
		public string Hash { get { return model.Hash; } }

		public string Title { get { return model.Title; } set { model.Title = value; } }

		public string TagCollection
		{
			get { return String.Join(" ", model.TagCollection); }
			set { model.TagCollection = value.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
		}

		public DelegateCommand<object> ViewCommand { get; }

		public bool IsSearchMatch(Func<ImageModel, bool> queryFunction)
		{
			return queryFunction(model);
		}
	}
}
