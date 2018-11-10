using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class TokenListView : UserControl
	{
		static TokenListView()
		{
			FrameworkElementFactory textBoxFactory = new FrameworkElementFactory(typeof(TextBox));
			textBoxFactory.SetBinding(TextBox.TextProperty, new Binding(nameof(ObservableString.Value)));

			ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<string>), typeof(TokenListView),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnItemsSourceUpdated));
			TextBoxTemplateProperty = DependencyProperty.Register(nameof(TextBoxTemplate), typeof(DataTemplate), typeof(TokenListView),
				new FrameworkPropertyMetadata(new DataTemplate() { VisualTree = textBoxFactory }));
		}

		public static readonly DependencyProperty ItemsSourceProperty;
		public static readonly DependencyProperty TextBoxTemplateProperty;

		public TokenListView()
		{
			InitializeComponent();
		}

		private bool isUpdatingSource;

		public IEnumerable<string> ItemsSource
		{
			get { return (IEnumerable<string>)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public DataTemplate TextBoxTemplate
		{
			get { return (DataTemplate)GetValue(TextBoxTemplateProperty); }
			set { SetValue(TextBoxTemplateProperty, value); }
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
			// Ensure the focus event is handled before adding an item,
			// so that the source does not get updated with an empty item.
			itemsControl.Focus();

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
			TextBox textBox = VisualTreeExtensions.GetDescendant<TextBox>((UIElement)sender);
			if (String.IsNullOrWhiteSpace(textBox.Text))
				itemsControl.Items.Remove(textBox.DataContext);
			UpdateSource();
		}

		private void UpdateSource()
		{
			isUpdatingSource = true;
			ItemsSource = itemsControl.Items.Cast<ObservableString>().Select(item => item.Value).ToList();
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
