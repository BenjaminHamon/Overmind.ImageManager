using Overmind.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Overmind.ImageManager.WindowsClient.Extensions
{
	public partial class AutocompleteTextBox : UserControl
	{
		public static readonly DependencyProperty TextProperty;
		public static readonly DependencyProperty AllValuesProperty;

		static AutocompleteTextBox()
		{
			TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(AutocompleteTextBox),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
			AllValuesProperty = DependencyProperty.Register(nameof(AllValues), typeof(IEnumerable<string>), typeof(AutocompleteTextBox));
		}

		public AutocompleteTextBox()
		{
			InitializeComponent();

			scrollViewer = VisualTreeExtensions.GetDescendant<ScrollViewer>(listBox);

			Loaded += (s, e) => { parentWindow = Window.GetWindow(this); if (parentWindow != null) parentWindow.Deactivated += ClosePopup; };
			Unloaded += (s, e) => { if (parentWindow != null) parentWindow.Deactivated -= ClosePopup; };
		}

		private readonly ScrollViewer scrollViewer;
		private Window parentWindow;

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public IEnumerable<string> AllValues
		{
			get { return (IEnumerable<string>)GetValue(AllValuesProperty); }
			set { SetValue(AllValuesProperty, value); }
		}

		private void HandleInput(object sender, KeyEventArgs eventArguments)
		{
			if (eventArguments.Key == Key.Escape)
			{
				popup.IsOpen = false;
				listBox.SelectedItem = null;
				scrollViewer.ScrollToHome();

				eventArguments.Handled = true;
				return;
			}

			if ((eventArguments.Key == Key.Enter) || (eventArguments.Key == Key.Tab))
			{
				if (listBox.SelectedItem != null)
					SetTextValue((string)listBox.SelectedItem);
				return;
			}

			if ((eventArguments.Key == Key.Up) || (eventArguments.Key == Key.Down))
			{
				if (popup.IsOpen == false)
				{
					RefreshList();
					popup.IsOpen = listBox.HasItems;
				}
				else
				{
					int newIndex = 0;

					if (eventArguments.Key == Key.Up)
					{
						newIndex = listBox.SelectedIndex - 1;
						if (newIndex < 0)
							newIndex = listBox.Items.Count - 1;
					}

					if (eventArguments.Key == Key.Down)
					{
						newIndex = listBox.SelectedIndex + 1;
						if (newIndex >= listBox.Items.Count)
							newIndex = 0;
					}

					listBox.SelectedIndex = newIndex;
					listBox.ScrollIntoView(listBox.SelectedItem);
				}

				eventArguments.Handled = true;
				return;
			}
		}

		private void SelectValue(object sender, MouseButtonEventArgs eventArguments)
		{
			listBox.SelectedItem = ((FrameworkElement)sender).DataContext;
			eventArguments.Handled = true;
		}

		private void SetTextValue(object sender, MouseButtonEventArgs eventArguments)
		{
			SetTextValue((string)((FrameworkElement)sender).DataContext);
			eventArguments.Handled = true;
		}

		private void SetTextValue(string value)
		{
			textBox.Text = value;
			textBox.CaretIndex = textBox.Text.Length;
			popup.IsOpen = false;
		}

		private void HandleTextChanged(object sender, TextChangedEventArgs eventArguments)
		{
			if (textBox.IsFocused)
			{
				RefreshList();

				if (String.IsNullOrWhiteSpace(textBox.Text))
				{
					listBox.SelectedItem = null;
					popup.IsOpen = false;
				}
				else
				{
					popup.IsOpen = listBox.HasItems;
				}
			}
		}

		private void RefreshList()
		{
			if (AllValues == null)
			{
				listBox.ItemsSource = null;
			}
			else
			{
				string baseText = textBox.Text.Trim();
				List<string> possibleValues = AllValues.Where(value => value.IndexOf(baseText, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
				listBox.ItemsSource = possibleValues;

				string firstMatch = listBox.Items.Cast<string>().FirstOrDefault(value => value.StartsWith(baseText, StringComparison.CurrentCultureIgnoreCase));
				listBox.SelectedItem = firstMatch ?? listBox.Items.Cast<string>().FirstOrDefault();

				if (listBox.SelectedItem == null)
				{
					scrollViewer.ScrollToHome();
				}
				else
				{
					scrollViewer.ScrollToEnd();
					listBox.ScrollIntoView(listBox.SelectedItem);
				}
			}
		}

		private void ClosePopup(object sender, EventArgs eventArguments)
		{
			popup.IsOpen = false;
		}
	}
}
