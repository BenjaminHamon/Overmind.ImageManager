using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Overmind.ImageManager.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class MainView : UserControl
	{
		public MainView()
		{
			InitializeComponent();
		}

		private void CreateCollection(object sender, RoutedEventArgs eventArguments)
		{
			CommonOpenFileDialog fileDialog = new CommonOpenFileDialog("Create Collection") { IsFolderPicker = true };
			if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
				return;

			string collectionPath = fileDialog.FileName;
			if (Directory.Exists(fileDialog.FileName) && Directory.EnumerateFileSystemEntries(fileDialog.FileName).Any())
			{
				MessageBox.Show("Directory is not empty", "Create Collection", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			MainViewModel viewModel = (MainViewModel)DataContext;
			viewModel.CreateCollectionCommand.Execute(collectionPath);

		}

		private void LoadCollection(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "Collection|" + DataProvider.ImageCollectionFileName;
			if (fileDialog.ShowDialog() == false)
				return;

			MainViewModel viewModel = (MainViewModel)DataContext;
			viewModel.LoadCollectionCommand.Execute(Path.GetDirectoryName(fileDialog.FileName));
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
