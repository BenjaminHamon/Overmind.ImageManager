using System.Windows;
using System.Windows.Input;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class ImageView : Window
	{
		// On load, the image sizes itself according to the window size,
		// then the window is resized to remove empty space around the image.
		// SizeToContent is not suitable because it can make the window too large for the screen.

		public ImageView()
		{
			InitializeComponent();
		}

		private void HandleKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Close();
		}

		private void ResizeWindow(object sender, RoutedEventArgs e)
		{
			double windowHeight = ActualHeight;
			double windowWidth = ActualWidth;
			double imageContainerHeight = imagePresenter.ActualHeight;
			double imageContainerWidth = imagePresenter.ActualWidth;
			double imageHeight = image.ActualHeight;
			double imageWidth = image.ActualWidth;

			if (imageHeight < imageContainerHeight)
			{
				Top += (imageContainerHeight - imageHeight) / 2;
				Height = imageHeight + windowHeight - imageContainerHeight;
			}
			if (imageWidth < Width)
			{
				Left += (imageContainerWidth - imageWidth) / 2;
				Width = imageWidth + windowWidth - imageContainerWidth;
			}
		}
	}
}
