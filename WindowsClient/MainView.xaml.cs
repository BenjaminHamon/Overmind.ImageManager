using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPFCustomMessageBox;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class MainView : UserControl
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(MainView));

		public static RoutedUICommand ExportCommand { get; } = new RoutedUICommand("Export", "Export", typeof(MainView));
		public static RoutedUICommand ExitCommand { get; } = new RoutedUICommand("Exit", "Exit", typeof(MainView));

		public MainView()
		{
			InitializeComponent();

			SelectListDisplayStyle(collectionView.ListDisplayStyle);

			gridDisplayMenuItem.Click += (s, e) => SelectListDisplayStyle("Grid");
			listDisplayMenuItem.Click += (s, e) => SelectListDisplayStyle("List");
		}

		private MainViewModel viewModel { get { return (MainViewModel)DataContext; } }

		private void IsCollectionOpened(object sender, CanExecuteRoutedEventArgs eventArguments)
		{
			eventArguments.CanExecute = viewModel.ActiveCollection != null;
		}

		private void CreateCollection(object sender, EventArgs eventArguments)
		{
			CloseCollection(sender, eventArguments);
			if (viewModel.ActiveCollection != null)
				return;

			CommonOpenFileDialog fileDialog = new CommonOpenFileDialog("Create Collection") { IsFolderPicker = true };
			if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
				return;

			try
			{
				viewModel.CreateCollectionCommand.Execute(fileDialog.FileName);
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to create collection (Path: '{0}')", fileDialog.FileName);
				WindowsApplication.ShowError("Create Collection", "Failed to create the image collection.", exception);
			}
		}

		private void LoadCollection(object sender, EventArgs eventArguments)
		{
			CommonOpenFileDialog fileDialog = new CommonOpenFileDialog("Open Collection") { IsFolderPicker = true };
			if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
				return;

			CloseCollection(sender, eventArguments);
			if (viewModel.ActiveCollection != null)
				return;

			try
			{
				viewModel.LoadCollectionCommand.Execute(fileDialog.FileName);
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to load collection (Path: '{0}')", fileDialog.FileName);
				WindowsApplication.ShowError("Open Collection", "Failed to open the image collection.", exception);
			}
		}

		private void SaveCollection(object sender, EventArgs eventArguments)
		{
			ForceUpdateOnFocusedElement();

			try
			{
				viewModel.SaveCollectionCommand.Execute(null);
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to save collection (Path: '{0}')", viewModel.ActiveCollection.StoragePath);
				WindowsApplication.ShowError("Save Collection", "Failed to save the image collection.", exception);
			}
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
					try
					{
						viewModel.SaveCollectionCommand.Execute(null);
					}
					catch (Exception exception)
					{
						Logger.Error(exception, "Failed to save collection (Path: '{0}')", viewModel.ActiveCollection.StoragePath);
						WindowsApplication.ShowError("Save Collection", "Failed to save the image collection.", exception);
						return;
					}

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

		private void ExportQueryResults(object sender, EventArgs eventArguments)
		{
			CommonOpenFileDialog fileDialog = new CommonOpenFileDialog("Export Query Results") { IsFolderPicker = true };
			if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
				return;

			try
			{
				viewModel.ExportCollectionCommand.Execute(fileDialog.FileName);
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to export collection ('{0}' => '{1}')", viewModel.ActiveCollection.StoragePath, fileDialog.FileName);
				WindowsApplication.ShowError("Export Collection", "Failed to export the image collection.", exception);
			}
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

		private void SelectListDisplayStyle(string style)
		{
			gridDisplayMenuItem.IsChecked = false;
			listDisplayMenuItem.IsChecked = false;

			switch (style)
			{
				case "Grid": gridDisplayMenuItem.IsChecked = true; break;
				case "List": listDisplayMenuItem.IsChecked = true; break;
				default: break;
			}

			if (style != collectionView.ListDisplayStyle)
			{
				collectionView.ListDisplayStyle = style;

				if (viewModel.ActiveCollection != null)
					viewModel.ActiveCollection.ResetDisplay();
			}
		}

		private void ShowApplicationHelp(object sender, RoutedEventArgs eventArguments)
		{
			WindowsApplication.ShowDocumentation("");
		}
	}
}
