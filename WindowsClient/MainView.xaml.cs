using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Overmind.ImageManager.Model;
using Overmind.ImageManager.WindowsClient.Downloads;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class MainView : UserControl
	{
		public MainView()
		{
			InitializeComponent();
		}

		private Window downloaderWindow;

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

		private void ExportQuery(object sender, RoutedEventArgs eventArguments)
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

			MainViewModel viewModel = (MainViewModel)DataContext;
			viewModel.ExportQuery(collectionPath);
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
