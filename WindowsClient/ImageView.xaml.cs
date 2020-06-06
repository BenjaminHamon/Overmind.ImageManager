using System.Windows;
using System.Windows.Controls;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class ImageView : UserControl
	{
		public ImageView()
		{
			InitializeComponent();
		}

		// On load, the image sizes itself according to the window size,
		// then the window is resized to remove empty space around the image.
		// SizeToContent is not suitable because it can make the window too large for the screen.

		public void ResizeWindow()
		{
			Window window = Window.GetWindow(this);

			double windowHeight = window.ActualHeight;
			double windowWidth = window.ActualWidth;
			double imageContainerHeight = ActualHeight;
			double imageContainerWidth = ActualWidth;
			double imageHeight = image.ActualHeight;
			double imageWidth = image.ActualWidth;

			if (imageHeight < imageContainerHeight)
			{
				window.Top += (imageContainerHeight - imageHeight) / 2;
				window.Height = imageHeight + windowHeight - imageContainerHeight;
			}

			if (imageWidth < window.Width)
			{
				window.Left += (imageContainerWidth - imageWidth) / 2;
				window.Width = imageWidth + windowWidth - imageContainerWidth;
			}
		}
	}
}
