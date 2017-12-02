using System;
using System.Collections.Generic;
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
		}

		private void CheckDragData(object sender, DragEventArgs eventArguments)
		{
			eventArguments.Effects = DragDropEffects.None;

			if (eventArguments.Data.GetDataPresent(DataFormats.FileDrop)
				|| eventArguments.Data.GetDataPresent(DataFormats.Text))
				eventArguments.Effects = DragDropEffects.Copy;

			eventArguments.Handled = true;
		}

		private void TextBox_DropImageUri(object sender, DragEventArgs eventArguments)
		{
			Downloader downloader = (Downloader)DataContext;
			TextBox textBox = (TextBox)sender;

			if (eventArguments.Data.GetDataPresent(DataFormats.FileDrop))
				textBox.Text = ((IList<string>)eventArguments.Data.GetData(DataFormats.FileDrop)).First();
			else if (eventArguments.Data.GetDataPresent(DataFormats.Text))
				textBox.Text = (string)eventArguments.Data.GetData(DataFormats.Text);

			eventArguments.Handled = true;
		}

		private void ListView_DropImageUri(object sender, DragEventArgs eventArguments)
		{
			Downloader downloader = (Downloader)DataContext;
			IEnumerable<string> imageUriCollection = new List<string>();

			if (eventArguments.Data.GetDataPresent(DataFormats.FileDrop))
				imageUriCollection = (IEnumerable<string>)eventArguments.Data.GetData(DataFormats.FileDrop);
			else if (eventArguments.Data.GetDataPresent(DataFormats.Text))
				imageUriCollection = ((string)eventArguments.Data.GetData(DataFormats.Text))
					.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string imageUri in imageUriCollection)
				downloader.AddDownload(imageUri);

			eventArguments.Handled = true;
		}
	}
}
