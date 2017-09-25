using System;
using System.Windows;
using System.Windows.Controls;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class CollectionView : UserControl
	{
		public CollectionView()
		{
			InitializeComponent();
		}

		private void OpenSlideShow(object sender, RoutedEventArgs eventArguments)
		{
			SlideShowViewModel slideShowViewModel = new SlideShowViewModel(((CollectionViewModel)DataContext).ImageCollection, new Random());
			SlideShowView slideShowView = new SlideShowView() { DataContext = slideShowViewModel };
			Window window = new Window() { Title = "SlideShow", Content = slideShowView };
			window.Closed += (s, e) => slideShowViewModel.Dispose();
			window.Show();
			slideShowView.Focus();
		}
	}
}
