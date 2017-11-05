using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class CollectionView : UserControl
	{
		public CollectionView()
		{
			InitializeComponent();

			DataContextChanged += HandleDataContextChanged;
		}

		private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArguments)
		{
			if (eventArguments.OldValue != null)
			{
				INotifyPropertyChanged oldDataContext = (INotifyPropertyChanged)eventArguments.OldValue;
				oldDataContext.PropertyChanged -= ScrollToSelection;
			}

			if (eventArguments.NewValue != null)
			{
				INotifyPropertyChanged newDataContext = (INotifyPropertyChanged)eventArguments.NewValue;
				newDataContext.PropertyChanged += ScrollToSelection;
			}
		}

		private void ScrollToSelection(object sender, PropertyChangedEventArgs eventArguments)
		{
			if (eventArguments.PropertyName == nameof(CollectionViewModel.SelectedImage))
				listBox.ScrollIntoView(((CollectionViewModel)DataContext).SelectedImage);
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
