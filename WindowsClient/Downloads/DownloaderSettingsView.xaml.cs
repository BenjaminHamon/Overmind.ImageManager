using NLog;
using Overmind.WpfExtensions;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Overmind.ImageManager.WindowsClient.Downloads
{
	public partial class DownloaderSettingsView : UserControl
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(DownloaderSettingsView));

		public DownloaderSettingsView()
		{
			InitializeComponent();

			DataContextChanged += HandleDataContextChanged;

			Loaded += RunPostLoad;
		}

		private DownloaderSettingsViewModel ViewModel { get { return (DownloaderSettingsViewModel) DataContext; } }

		private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArguments)
		{
			if (eventArguments.OldValue != null)
			{
				DownloaderSettingsViewModel oldDataContext = (DownloaderSettingsViewModel)eventArguments.OldValue;
				oldDataContext.SourceConfigurationCollection.CollectionChanged -= ShowNewConfiguration_Dispatch;
			}

			if (eventArguments.NewValue != null)
			{
				DownloaderSettingsViewModel newDataContext = (DownloaderSettingsViewModel)eventArguments.NewValue;
				newDataContext.SourceConfigurationCollection.CollectionChanged += ShowNewConfiguration_Dispatch;
			}
		}

		private void RunPostLoad(object sender, EventArgs eventArguments)
		{
			ViewModel.SourceConfigurationCollection.CollectionChanged -= ShowNewConfiguration_Dispatch;

			try
			{
				ViewModel.ReloadSettings();
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to reload settings");
				WindowsApplication.ShowError("Settings", "Failed to reload downloader settings.", exception);
			}

			ViewModel.SourceConfigurationCollection.CollectionChanged += ShowNewConfiguration_Dispatch;
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
				WindowsApplication.ShowError("Settings", "Failed to save downloader settings.", exception);
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
	}
}
