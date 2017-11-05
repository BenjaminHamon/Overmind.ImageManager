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

		private void CheckDraggedImageUri(object sender, DragEventArgs eventArguments)
		{
			eventArguments.Effects = DragDropEffects.None;

			if (eventArguments.Data.GetDataPresent(DataFormats.FileDrop))
			{
				IList<string> files = (IList<string>)eventArguments.Data.GetData(DataFormats.FileDrop);
				if (files.Count == 1)
					eventArguments.Effects = DragDropEffects.Copy;
			}
			else if (eventArguments.Data.GetDataPresent(DataFormats.Text))
			{
				string text = (string)eventArguments.Data.GetData(DataFormats.Text);
				if (Uri.IsWellFormedUriString(text, UriKind.Absolute))
					eventArguments.Effects = DragDropEffects.Copy;
			}

			eventArguments.Handled = true;
		}

		private void DropImageUri(object sender, DragEventArgs eventArguments)
		{
			if (eventArguments.Data.GetDataPresent(DataFormats.FileDrop))
				newImageUri.Text = ((IList<string>)eventArguments.Data.GetData(DataFormats.FileDrop)).First();
			else if (eventArguments.Data.GetDataPresent(DataFormats.Text))
				newImageUri.Text = (string)eventArguments.Data.GetData(DataFormats.Text);

			eventArguments.Handled = true;
		}
	}
}
