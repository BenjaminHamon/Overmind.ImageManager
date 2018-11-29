﻿using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Overmind.ImageManager.Model;
using Overmind.ImageManager.WindowsClient.Downloads;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPFCustomMessageBox;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class MainView : UserControl
	{
		public MainView()
		{
			InitializeComponent();
		}

		private MainViewModel viewModel { get { return (MainViewModel)DataContext; } }

		private Window downloaderWindow;

		private void CreateCollection(object sender, EventArgs eventArguments)
		{
			CloseCollection(sender, eventArguments);
			if (viewModel.ActiveCollection != null)
				return;

			CommonOpenFileDialog fileDialog = new CommonOpenFileDialog("Create Collection") { IsFolderPicker = true };
			if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
				return;

			string collectionPath = fileDialog.FileName;
			if (Directory.Exists(fileDialog.FileName) && Directory.EnumerateFileSystemEntries(fileDialog.FileName).Any())
			{
				MessageBox.Show("Directory is not empty", "Create Collection", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			viewModel.CreateCollectionCommand.Execute(collectionPath);
		}

		private void LoadCollection(object sender, EventArgs eventArguments)
		{
			CloseCollection(sender, eventArguments);
			if (viewModel.ActiveCollection != null)
				return;

			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "Collection|" + DataProvider.ImageCollectionFileName;
			if (fileDialog.ShowDialog() == false)
				return;

			viewModel.LoadCollectionCommand.Execute(Path.GetDirectoryName(fileDialog.FileName));
		}

		private void CloseCollection(object sender, EventArgs eventArguments)
		{
			if (viewModel.ActiveCollection == null)
				return;

			if (viewModel.IsActiveCollectionSaved())
			{
				viewModel.CloseCollectionCommand.Execute(null);
			}
			else
			{
				MessageBoxResult result = CustomMessageBox.ShowYesNoCancel(
					"Save changes before closing?", "Closing collection", "Save and close", "Close without saving", "Cancel closing", MessageBoxImage.Warning);

				if (result == MessageBoxResult.Yes)
				{
					viewModel.SaveCollectionCommand.Execute(null);
					viewModel.CloseCollectionCommand.Execute(null);
				}
				else if (result == MessageBoxResult.No)
				{
					viewModel.CloseCollectionCommand.Execute(null);
				}
				else
				{
					if (eventArguments is CancelEventArgs)
						((CancelEventArgs)eventArguments).Cancel = true;
				}
			}
		}

		private void ExportQuery(object sender, EventArgs eventArguments)
		{
			CommonOpenFileDialog fileDialog = new CommonOpenFileDialog("Export Query") { IsFolderPicker = true };
			if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
				return;

			string collectionPath = fileDialog.FileName;
			if (Directory.Exists(fileDialog.FileName) && Directory.EnumerateFileSystemEntries(fileDialog.FileName).Any())
			{
				MessageBox.Show("Directory is not empty", "Export Query", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			viewModel.ExportQuery(collectionPath);
		}

		internal void ExitApplication(object sender, EventArgs eventArguments)
		{
			CloseCollection(sender, eventArguments);
			if (viewModel.ActiveCollection != null)
				return;

			viewModel.ExitApplicationCommand.Execute(null);
		}

		private void ShowDownloader(object sender, RoutedEventArgs eventArguments)
		{
			if (downloaderWindow == null)
			{
				DownloaderView downloaderView = new DownloaderView();
				Binding dataContextBinding = new Binding() { Source = DataContext, Path = new PropertyPath(nameof(MainViewModel.Downloader)) };
				BindingOperations.SetBinding(downloaderView, DataContextProperty, dataContextBinding);

				downloaderWindow = new Window()
				{
					Title = "Downloads - " + WindowsApplication.Name,
					Content = downloaderView,
					Height = 400,
					Width = 600,
				};

				downloaderWindow.Closed += (s, e) => downloaderWindow = null;
				downloaderWindow.Show();
			}
			else
			{
				if (downloaderWindow.WindowState == WindowState.Minimized)
					downloaderWindow.WindowState = WindowState.Normal;
				downloaderWindow.Activate();
			}
		}
	}
}
