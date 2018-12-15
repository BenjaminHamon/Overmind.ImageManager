using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
				MessageBox.Show("The selected directory is not empty.", "Create Collection", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			viewModel.CreateCollectionCommand.Execute(collectionPath);
		}

		private void LoadCollection(object sender, EventArgs eventArguments)
		{
			CloseCollection(sender, eventArguments);
			if (viewModel.ActiveCollection != null)
				return;

			CommonOpenFileDialog fileDialog = new CommonOpenFileDialog("Open Collection") { IsFolderPicker = true };
			if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
				return;

			try
			{
				viewModel.LoadCollectionCommand.Execute(fileDialog.FileName);
			}
			catch
			{
				MessageBox.Show("Failed to open an image collection from the selected directory.", "Open Collection", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void SaveCollection(object sender, EventArgs eventArguments)
		{
			ForceUpdateOnFocusedElement();
			viewModel.SaveCollectionCommand.Execute(null);
		}

		private void CloseCollection(object sender, EventArgs eventArguments)
		{
			if (viewModel.ActiveCollection == null)
				return;

			ForceUpdateOnFocusedElement();

			if (viewModel.ActiveCollection.IsSaved())
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

		private void ForceUpdateOnFocusedElement()
		{
			// Some controls update their source when they lose focus, but this does not happen when switching focus scope.
			// The issue occurs when using a menu command or closing the window (directly or from the system task bar).

			// See also https://stackoverflow.com/questions/57493/wpf-databind-before-saving
			// - Disabling the menu focus scope is only a partial fix since it does not catch the issue when the main window is closed,
			//   plus it introduces a bug where the main window close button may require two clicks, from the focus being stuck on the menu somehow
			//   (the behavior is similar to having a menu open, the first click would dismiss the menu and does not trigger on the button)
			// - Updating the active element directly relies on Keyboard.FocusedElement, which is null when closing the window from the system task bar,
			//   and requires supporting any control type (TokenListView does not expose its internal update logic).

			Window mainWindow = Window.GetWindow(this);
			IInputElement focusedElement = FocusManager.GetFocusedElement(mainWindow);
			if (focusedElement != null)
			{
				FocusManager.SetFocusedElement(mainWindow, this);
				FocusManager.SetFocusedElement(mainWindow, focusedElement);
			}
		}
	}
}
