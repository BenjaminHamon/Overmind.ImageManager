using Overmind.ImageManager.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Overmind.ImageManager.WindowsClient
{
	public class CollectionQuery : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private string searchField;
		public string Search
		{
			get { return searchField; }
			set
			{
				if (searchField == value)
					return;
				searchField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Search)));
				Error = null;
			}
		}

		private string errorField;
		public string Error
		{
			get { return errorField; }
			set
			{
				if (errorField == value)
					return;
				errorField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
			}
		}

		public IEnumerable<ImageViewModel> Execute(CollectionModel model, IEnumerable<ImageViewModel> allImages)
		{
			Func<ImageModel, string> groupQuery = image => "#All";

			foreach (ImageViewModel image in allImages)
				image.Group = groupQuery(image.Model);

			IEnumerable<ImageViewModel> resultImages = allImages;

			if (String.IsNullOrEmpty(Search) == false)
			{
				IEnumerable<ImageModel> searchResult = model.SearchAdvanced(Search);
				resultImages = resultImages.Where(image => searchResult.Contains(image.Model));
			}

			resultImages = resultImages.OrderBy(image => image.Group);

			return new List<ImageViewModel>(resultImages);
		}
	}
}
