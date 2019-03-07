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
		public AutocompleteTextBox()
		{
			InitializeComponent();

			scrollViewer = VisualTreeExtensions.GetDescendant<ScrollViewer>(listBox);

			Loaded += (s, e) => { parentWindow = Window.GetWindow(this); if (parentWindow != null) parentWindow.Deactivated += ClosePopup; };
			Unloaded += (s, e) => { if (parentWindow != null) parentWindow.Deactivated -= ClosePopup; };
		}

		private readonly ScrollViewer scrollViewer;
		private Window parentWindow;

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(nameof(Text), typeof(string), typeof(AutocompleteTextBox),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty AllValuesProperty =
			DependencyProperty.Register(nameof(AllValues), typeof(IEnumerable<string>), typeof(AutocompleteTextBox),
				new PropertyMetadata(OnAllValuesUpdated));

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
				eventArguments.Handled = true;
				return;
			}

			if ((Keyboard.Modifiers == ModifierKeys.Control) && (eventArguments.Key == Key.Space))
			{
				if ((popup.IsOpen == false) && listBox.HasItems)
				{
					popup.IsOpen = true;
					ResetSelection();
				}

				eventArguments.Handled = true;
				return;
			}

			if (popup.IsOpen == false)
			{
				if (String.IsNullOrEmpty(textBox.Text))
				{
					popup.IsOpen = true;
				}
			}
			else
			{
				if (eventArguments.Key == Key.Up)
				{
					int newIndex = listBox.SelectedIndex - 1;
					if (newIndex < 0)
						newIndex = listBox.Items.Count - 1;
					listBox.SelectedIndex = newIndex;
					listBox.ScrollIntoView(listBox.SelectedItem);
					eventArguments.Handled = true;
				}
				else if (eventArguments.Key == Key.Down)
				{
					int newIndex = listBox.SelectedIndex + 1;
					if (newIndex >= listBox.Items.Count)
						newIndex = 0;
					listBox.SelectedIndex = newIndex;
					listBox.ScrollIntoView(listBox.SelectedItem);
					eventArguments.Handled = true;
				}
				else if ((eventArguments.Key == Key.Enter) || (eventArguments.Key == Key.Tab))
				{
					if (listBox.SelectedItem != null)
					{
						SetTextValue((string)listBox.SelectedItem);
						if (eventArguments.Key == Key.Space)
							eventArguments.Handled = true;
					}
				}
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

		private static void OnAllValuesUpdated(DependencyObject sender, DependencyPropertyChangedEventArgs eventArguments)
		{
			((AutocompleteTextBox)sender).RefreshList();
		}

		private void RefreshList(object sender, EventArgs eventArguments)
		{
			RefreshList();
		}

		private void RefreshList()
		{
			if (AllValues == null)
			{
				listBox.ItemsSource = null;
			}
			else
			{
				string baseText = textBox.Text.Substring(0, textBox.CaretIndex);
				List<string> possibleValues = AllValues.Where(value => value.IndexOf(baseText, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
				listBox.ItemsSource = possibleValues;

				if (popup.IsOpen)
					ResetSelection();
			}

			if (listBox.HasItems == false)
				popup.IsOpen = false;
		}

		private void ResetSelection()
		{
			string baseText = textBox.Text.Substring(0, textBox.CaretIndex);
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

		private void ClosePopup(object sender, EventArgs eventArguments)
		{
			popup.IsOpen = false;
		}
	}
}
