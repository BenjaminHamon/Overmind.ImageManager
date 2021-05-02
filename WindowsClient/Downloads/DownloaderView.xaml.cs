using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public partial class DownloaderView : UserControl
	{
		public DownloaderView()
		{
			InitializeComponent();

			scrollViewer = VisualTreeExtensions.GetDescendant<ScrollViewer>(listView);

			DataContextChanged += HandleDataContextChanged;
		}

		private readonly ScrollViewer scrollViewer;

		private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArguments)
		{
			if (eventArguments.OldValue != null)
			{
				DownloaderViewModel oldDataContext = (DownloaderViewModel)eventArguments.OldValue;
				oldDataContext.DownloadCollection.CollectionChanged -= ScrollToEnd;
			}

			if (eventArguments.NewValue != null)
			{
				DownloaderViewModel newDataContext = (DownloaderViewModel)eventArguments.NewValue;
				newDataContext.DownloadCollection.CollectionChanged += ScrollToEnd;
			}
		}

		private void ScrollToEnd(object sender, NotifyCollectionChangedEventArgs eventArguments)
		{
			if (eventArguments.Action == NotifyCollectionChangedAction.Add)
			{
				scrollViewer.ScrollToEnd();
			}
		}

		private void CheckDragData(object sender, DragEventArgs eventArguments)
		{
			eventArguments.Effects = DragDropEffects.None;

			if (eventArguments.Data.GetDataPresent(DataFormats.FileDrop)
				|| eventArguments.Data.GetDataPresent(DataFormats.Text))
			{
				eventArguments.Effects = DragDropEffects.Copy;
			}

			eventArguments.Handled = true;
		}

		private void DropUri_FromTextBox(object sender, DragEventArgs eventArguments)
		{
			DownloaderViewModel downloader = (DownloaderViewModel)DataContext;
			TextBox textBox = (TextBox)sender;

			if (eventArguments.Data.GetDataPresent(DataFormats.FileDrop))
			{
				textBox.Text = ((IList<string>)eventArguments.Data.GetData(DataFormats.FileDrop)).First();
			}
			else if (eventArguments.Data.GetDataPresent(DataFormats.Text))
			{
				textBox.Text = (string)eventArguments.Data.GetData(DataFormats.Text);
			}

			eventArguments.Handled = true;
		}

		private void DropUri_FromListView(object sender, DragEventArgs eventArguments)
		{
			DownloaderViewModel downloader = (DownloaderViewModel)DataContext;
			IEnumerable<string> imageUriCollection = new List<string>();

			if (eventArguments.Data.GetDataPresent(DataFormats.FileDrop))
			{
				imageUriCollection = (IEnumerable<string>) eventArguments.Data.GetData(DataFormats.FileDrop);
			}
			else if (eventArguments.Data.GetDataPresent(DataFormats.Text))
			{
				imageUriCollection = ((string) eventArguments.Data.GetData(DataFormats.Text))
					.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			}

			foreach (string imageUri in imageUriCollection)
			{
				downloader.AddDownload(imageUri);
			}

			eventArguments.Handled = true;
		}
	}
}
