using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;
using Overmind.WpfExtensions;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Overmind.ImageManager.WindowsClient.Wallpapers
{
	public partial class WallpaperSettingsView : UserControl
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(WallpaperSettingsView));

		public WallpaperSettingsView()
		{
			InitializeComponent();

			DataContextChanged += HandleDataContextChanged;

			Loaded += RunPostLoad;
		}

		private WallpaperSettingsViewModel ViewModel { get { return (WallpaperSettingsViewModel) DataContext; } }

		private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArguments)
		{
			if (eventArguments.OldValue != null)
			{
				WallpaperSettingsViewModel oldDataContext = (WallpaperSettingsViewModel) eventArguments.OldValue;
				oldDataContext.ConfigurationCollection.CollectionChanged -= ShowNewConfiguration_Dispatch;
			}

			if (eventArguments.NewValue != null)
			{
				WallpaperSettingsViewModel newDataContext = (WallpaperSettingsViewModel) eventArguments.NewValue;
				newDataContext.ConfigurationCollection.CollectionChanged += ShowNewConfiguration_Dispatch;
			}
		}

		private void RunPostLoad(object sender, EventArgs eventArguments)
		{
			ViewModel.ConfigurationCollection.CollectionChanged -= ShowNewConfiguration_Dispatch;

			try
			{
				ViewModel.ReloadSettings();
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to reload settings");
				WindowsApplication.ShowError("Settings", "Failed to reload wallpaper settings.", exception);
			}

			ViewModel.ConfigurationCollection.CollectionChanged += ShowNewConfiguration_Dispatch;
		}

		private void CanSaveSettings(object sender, CanExecuteRoutedEventArgs eventArguments)
		{
			eventArguments.CanExecute = ViewModel?.SaveSettingsCommand.CanExecute(null) ?? false;
		}

		private void SaveSettings(object sender, EventArgs eventArguments)
		{
			try
			{
				ViewModel.SaveSettingsCommand.Execute(null);
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to save settings");
				WindowsApplication.ShowError("Settings", "Failed to save wallpaper settings.", exception);
			}
		}

		private void ShowNewConfiguration_Dispatch(object sender, NotifyCollectionChangedEventArgs eventArguments)
		{
			if (eventArguments.Action == NotifyCollectionChangedAction.Add)
			{
				// Dispatching because the item containers have not been created yet
				Dispatcher.BeginInvoke(new Action(() => ShowNewConfiguration(eventArguments.NewItems.Cast<object>().First(), true)));
				foreach (object item in eventArguments.NewItems.Cast<object>().Skip(1))
					Dispatcher.BeginInvoke(new Action(() => ShowNewConfiguration(item, false)));
			}
		}

		private void ShowNewConfiguration(object item, bool focus)
		{
			FrameworkElement itemElement = (FrameworkElement)itemsControl.ItemContainerGenerator.ContainerFromItem(item);
			VisualTreeExtensions.GetDescendant<Expander>(itemElement).IsExpanded = true;

			if (focus)
			{
				TextBox textBox = VisualTreeExtensions.GetDescendant<TextBox>(itemElement);
				textBox.Focus();
				textBox.CaretIndex = textBox.Text.Length;

				itemElement.BringIntoView();
			}
		}

		private void ExpandAll(object sender, RoutedEventArgs eventArguments)
		{
			foreach (object item in itemsControl.Items)
			{
				FrameworkElement itemElement = (FrameworkElement)itemsControl.ItemContainerGenerator.ContainerFromItem(item);
				VisualTreeExtensions.GetDescendant<Expander>(itemElement).IsExpanded = true;
			}
		}

		private void CollapseAll(object sender, RoutedEventArgs eventArguments)
		{
			foreach (object item in itemsControl.Items)
			{
				FrameworkElement itemElement = (FrameworkElement)itemsControl.ItemContainerGenerator.ContainerFromItem(item);
				VisualTreeExtensions.GetDescendant<Expander>(itemElement).IsExpanded = false;
			}
		}

		private void BrowseForCollectionPath(object sender, RoutedEventArgs eventArguments)
		{
			FrameworkElement senderElement = (FrameworkElement)sender;
			WallpaperConfigurationViewModel viewModel = (WallpaperConfigurationViewModel)senderElement.DataContext;

			CommonOpenFileDialog fileDialog = new CommonOpenFileDialog("Select Collection")
			{
				IsFolderPicker = true,
				DefaultDirectory = viewModel.CollectionPath,
			};

			if (fileDialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
				viewModel.CollectionPath = fileDialog.FileName;
		}
	}
}
