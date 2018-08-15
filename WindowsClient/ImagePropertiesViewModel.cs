using Overmind.ImageManager.Model;
using System;
using System.Collections.Generic;

namespace Overmind.ImageManager.WindowsClient
{
	public class ImagePropertiesViewModel
	{
		public ImagePropertiesViewModel(ImageModel model, Func<string> getImagePath)
		{
			this.model = model;
			this.getImagePath = getImagePath;
			displayViewModel = new ImageViewModel(model, getImagePath);
		}

		private readonly ImageModel model;
		private readonly Func<string> getImagePath;
		private readonly ImageViewModel displayViewModel;

		public string FilePath { get { return getImagePath(); } }
		public string Name { get { return model.FileName; } }
		public string Title { get { return model.Title; } set { model.Title = value; } }
		public List<string> SubjectCollection { get { return model.SubjectCollection; } set { model.SubjectCollection = value; } }
		public List<string> ArtistCollection { get { return model.ArtistCollection; } set { model.ArtistCollection = value; } }
		public List<string> TagCollection { get { return model.TagCollection; } set { model.TagCollection = value; } }
		public int Score { get { return model.Score; } set { model.Score = value; } }
		public DateTime AdditionDate { get { return model.AdditionDate; } }
		public Uri Source { get { return model.Source; } set { model.Source = value; } }
		public string Hash { get { return model.Hash; } }

		public List<string> AllSubjects { get; set; } = new List<string>();
		public List<string> AllArtists { get; set; } = new List<string>();
		public List<string> AllTags { get; set; } = new List<string>();
	}
}
