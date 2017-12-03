using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class TokenListView : UserControl
	{
		public TokenListView()
		{
			InitializeComponent();
		}

		private bool isUpdatingSource;

		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<string>), typeof(TokenListView),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnItemsSourceUpdated));

		public IEnumerable<string> ItemsSource
		{
			get { return (IEnumerable<string>)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}
		
		private void AddItem(object sender, MouseButtonEventArgs eventArguments)
		{
			if (eventArguments.OriginalSource is Border)
			{
				AddItem();

				// Prevent the mouse event from making the text box lose focus
				eventArguments.Handled = true;
			}
		}

		private void AddItem(object sender, KeyEventArgs eventArguments)
		{
			if (eventArguments.Key == Key.Enter)
				AddItem();
		}

		private void AddItem()
		{
			int itemIndex = itemsControl.Items.Add(new ObservableString());
			UIElement itemElement = (UIElement)itemsControl.ItemContainerGenerator.ContainerFromIndex(itemIndex);
			TextBox textBox = VisualTreeExtensions.GetDescendant<TextBox>(itemElement);
			textBox.Focus();
			textBox.CaretIndex = textBox.Text.Length;
		}

		private void RemoveItem(object sender, RoutedEventArgs eventArguments)
		{
			itemsControl.Items.Remove(((FrameworkElement)sender).DataContext);
			UpdateSource();
		}

		private void HandleItemLostFocus(object sender, RoutedEventArgs eventArguments)
		{
			TextBox textBox = (TextBox)sender;
			if (String.IsNullOrWhiteSpace(textBox.Text))
				itemsControl.Items.Remove(textBox.DataContext);
			UpdateSource();
		}

		private void UpdateSource()
		{
			isUpdatingSource = true;

			List<string> newList = new List<string>();
			foreach (ObservableString item in itemsControl.Items)
				newList.Add(item.Value);
			ItemsSource = newList;

			isUpdatingSource = false;
		}

		private static void OnItemsSourceUpdated(DependencyObject sender, DependencyPropertyChangedEventArgs eventArguments)
		{
			((TokenListView)sender).OnItemsSourceUpdated((IEnumerable<string>)eventArguments.OldValue, (IEnumerable<string>)eventArguments.NewValue);
		}

		private void OnItemsSourceUpdated(IEnumerable<string> oldValue, IEnumerable<string> newValue)
		{
			if (isUpdatingSource == false)
			{
				itemsControl.Items.Clear();

				if (newValue != null)
				{
					foreach (string item in newValue)
						itemsControl.Items.Add(new ObservableString() { Value = item });
				}
			}
		}
	}
}
