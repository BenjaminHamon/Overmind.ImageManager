using Overmind.WpfExtensions;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class CollectionView : UserControl
	{
		public CollectionView()
		{
			InitializeComponent();

			scrollViewer = VisualTreeExtensions.GetDescendant<ScrollViewer>(listBox);

			DataContextChanged += HandleDataContextChanged;
			DataContextChanged += (s, e) => ResetDisplay();
			layoutComboBox.SelectionChanged += (s, e) => ResetDisplay();
			scrollViewer.ScrollChanged += (s, e) => ScheduleTryDisplayMore();
		}

		private readonly ScrollViewer scrollViewer;
		private bool isTryDisplayMoreScheduled;

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
			{
				CollectionViewModel viewModel = (CollectionViewModel)DataContext;
				if (viewModel.DisplayedImages.Contains(viewModel.SelectedImage))
					listBox.ScrollIntoView(viewModel.SelectedImage);
			}
		}
		
		private void ResetDisplay()
		{
			if (DataContext == null)
				return;

			CollectionViewModel viewModel = (CollectionViewModel)DataContext;
			viewModel.ResetDisplay();
			ScheduleTryDisplayMore();
		}

		// TryDisplayMore may be invoked several times in a row, but it needs to wait for the view properties to update.
		// ScheduleTryDisplayMore ensures events in quick succession do not invoke TryDisplayMore before the view updates.

		private void ScheduleTryDisplayMore()
		{
			if (DataContext == null)
				return;

			if (isTryDisplayMoreScheduled)
				return;

			isTryDisplayMoreScheduled = true;
			Dispatcher.BeginInvoke(new Action(() => TryDisplayMore()), DispatcherPriority.ContextIdle);
		}

		private void TryDisplayMore()
		{
			CollectionViewModel viewModel = (CollectionViewModel)DataContext;
			bool availableDisplaySpace = (scrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
				|| (scrollViewer.VerticalOffset + (scrollViewer.ViewportHeight / 5) > scrollViewer.ScrollableHeight);
			if (viewModel.CanDisplayMore && availableDisplaySpace)
			{
				viewModel.DisplayMore();
				Dispatcher.BeginInvoke(new Action(() => TryDisplayMore()), DispatcherPriority.ContextIdle);
			}
			else
			{
				isTryDisplayMoreScheduled = false;
			}
		}

		private void OpenSlideShow(object sender, RoutedEventArgs eventArguments)
		{
			CollectionViewModel viewModel = (CollectionViewModel)DataContext;
			SlideShowViewModel slideShowViewModel = new SlideShowViewModel(viewModel.FilteredImages, new Random());
			SlideShowView slideShowView = new SlideShowView() { DataContext = slideShowViewModel };
			Window window = new Window() { Title = "SlideShow", Content = slideShowView };
			window.Closed += (s, e) => slideShowViewModel.Dispose();
			window.Show();
			slideShowView.Focus();
		}
	}
}
