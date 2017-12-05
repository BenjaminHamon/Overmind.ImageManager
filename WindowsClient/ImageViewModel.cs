using Overmind.ImageManager.Model;
using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;

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
		public string FilePath { get { return getImagePath(); } }
		public string Name { get { return model.FileName; } }
		public string Title { get { return model.Title; } set { model.Title = value; } }
		public List<string> SubjectCollection { get { return model.SubjectCollection; } set { model.SubjectCollection = value; } }
		public List<string> ArtistCollection { get { return model.ArtistCollection; } set { model.ArtistCollection = value; } }
		public List<string> TagCollection { get { return model.TagCollection; } set { model.TagCollection = value; } }
		public int Score { get { return model.Score; } set { model.Score = value; } }
		public DateTime AdditionDate { get { return model.AdditionDate; } }
		public string Hash { get { return model.Hash; } }
		
		public DelegateCommand<object> ViewCommand { get; }

		public bool IsSearchMatch(Func<ImageModel, bool> queryFunction)
		{
			return queryFunction(model);
		}
	}
}
