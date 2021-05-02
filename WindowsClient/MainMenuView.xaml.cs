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
	public partial class MainMenuView : UserControl
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(MainMenuView));

		public static RoutedUICommand ExportCommand { get; } = new RoutedUICommand("Export", "Export", typeof(MainView));
		public static RoutedUICommand ExitCommand { get; } = new RoutedUICommand("Exit", "Exit", typeof(MainView));

		public MainMenuView()
		{
			InitializeComponent();

			gridDisplayMenuItem.Click += (s, e) => SelectListDisplayStyle("Grid");
			listDisplayMenuItem.Click += (s, e) => SelectListDisplayStyle("List");
		}

		private MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }

		public static readonly DependencyProperty CollectionViewProperty
			 = DependencyProperty.Register(nameof(CollectionView), typeof(CollectionView), typeof(MainMenuView));

		public CollectionView CollectionView
		{
			get { return (CollectionView)GetValue(CollectionViewProperty); }
			set { SetValue(CollectionViewProperty, value); }
		}

		// Register commands on the main element so that they are available everywhere regardless of focus
		public void RegisterCommands(FrameworkElement mainElement)
		{
			mainElement.Focusable = true;
			mainElement.FocusVisualStyle = null;

			mainElement.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, CreateCollection));
			mainElement.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, LoadCollection));
			mainElement.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveCollection, IsCollectionOpened));
			mainElement.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, CloseCollection, IsCollectionOpened));
			mainElement.CommandBindings.Add(new CommandBinding(ExportCommand, ExportQueryResults, IsCollectionOpened));
			mainElement.CommandBindings.Add(new CommandBinding(ExitCommand, ExitApplication));
			mainElement.CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, ShowApplicationHelp));

			mainElement.InputBindings.Add(new KeyBinding(ApplicationCommands.New, Key.N, ModifierKeys.Control));
			mainElement.InputBindings.Add(new KeyBinding(ApplicationCommands.Open, Key.N, ModifierKeys.Control));
			mainElement.InputBindings.Add(new KeyBinding(ApplicationCommands.Save, Key.N, ModifierKeys.Control));
			mainElement.InputBindings.Add(new KeyBinding(ApplicationCommands.Help, Key.F1, ModifierKeys.None));
		}

		private void IsCollectionOpened(object sender, CanExecuteRoutedEventArgs eventArguments)
		{
			eventArguments.CanExecute = ViewModel?.ActiveCollection != null;
		}

		private void CreateCollection(object sender, EventArgs eventArguments)
		{
			CloseCollection(sender, eventArguments);
			if (ViewModel.ActiveCollection != null)
				return;

			CommonOpenFileDialog fileDialog = new CommonOpenFileDialog("Create Collection") { IsFolderPicker = true };
			if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
				return;

			try
			{
				ViewModel.CreateCollectionCommand.Execute(fileDialog.FileName);
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
			if (ViewModel.ActiveCollection != null)
				return;

			try
			{
				ViewModel.LoadCollectionCommand.Execute(fileDialog.FileName);
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to load collection (Path: '{0}')", fileDialog.FileName);
				WindowsApplication.ShowError("Open Collection", "Failed to open the image collection.", exception);
			}
		}

		private void SaveCollection(object sender, EventArgs eventArguments)
		{
			WindowsApplication.ForceUpdateOnFocusedElement(this);

			try
			{
				ViewModel.SaveCollectionCommand.Execute(null);
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to save collection (Path: '{0}')", ViewModel.ActiveCollection.StoragePath);
				WindowsApplication.ShowError("Save Collection", "Failed to save the image collection.", exception);
			}
		}

		private void CloseCollection(object sender, EventArgs eventArguments)
		{
			if (ViewModel.ActiveCollection == null)
				return;

			WindowsApplication.ForceUpdateOnFocusedElement(this);

			if (ViewModel.ActiveCollection.IsSaved())
			{
				ViewModel.CloseCollectionCommand.Execute(null);
			}
			else
			{
				MessageBoxResult result = CustomMessageBox.ShowYesNoCancel(
					"Save changes before closing?", "Closing collection", "Save and close", "Close without saving", "Cancel closing", MessageBoxImage.Warning);

				if (result == MessageBoxResult.Yes)
				{
					try
					{
						ViewModel.SaveCollectionCommand.Execute(null);
					}
					catch (Exception exception)
					{
						Logger.Error(exception, "Failed to save collection (Path: '{0}')", ViewModel.ActiveCollection.StoragePath);
						WindowsApplication.ShowError("Save Collection", "Failed to save the image collection.", exception);
						return;
					}

					ViewModel.CloseCollectionCommand.Execute(null);
				}
				else if (result == MessageBoxResult.No)
				{
					ViewModel.CloseCollectionCommand.Execute(null);
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
				ViewModel.ExportCollectionCommand.Execute(fileDialog.FileName);
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to export collection ('{0}' => '{1}')", ViewModel.ActiveCollection.StoragePath, fileDialog.FileName);
				WindowsApplication.ShowError("Export Collection", "Failed to export the image collection.", exception);
			}
		}

		internal void ExitApplication(object sender, EventArgs eventArguments)
		{
			CloseCollection(sender, eventArguments);
			if (ViewModel.ActiveCollection != null)
				return;

			ViewModel.ExitApplicationCommand.Execute(null);
		}

		private void SelectListDisplayStyle(string style)
		{
			if (style != CollectionView.ListDisplayStyle)
			{
				WindowsApplication.ForceUpdateOnFocusedElement(this);
				CollectionView.ListDisplayStyle = style;
			}
		}

		private void ShowApplicationHelp(object sender, RoutedEventArgs eventArguments)
		{
			WindowsApplication.ShowDocumentation(String.Empty);
		}
	}
}
