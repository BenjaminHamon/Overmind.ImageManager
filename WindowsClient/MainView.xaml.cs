using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Overmind.ImageManager.Model;
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
	}
}
