using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class SlideShowView : UserControl
	{
		public SlideShowView()
		{
			InitializeComponent();

			cycleTimer = new DispatcherTimer();
			cycleTimer.Interval = TimeSpan.FromSeconds(1);

			Loaded += InitializeTimer;
			Unloaded += DisposeTimer;
			DataContextChanged += HandleDataContextChanged;
		}

		private readonly DispatcherTimer cycleTimer;
		private DateTime lastImageChange;

		private void InitializeTimer(object sender, RoutedEventArgs eventArguments)
		{
			lastImageChange = DateTime.Now;
			cycleTimer.Tick += CycleImage;
			cycleTimer.Start();
		}

		private void DisposeTimer(object sender, RoutedEventArgs eventArguments)
		{
			cycleTimer.Stop();
			cycleTimer.Tick -= CycleImage;
		}

		private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArguments)
		{
			if (eventArguments.OldValue != null)
			{
				SlideShowViewModel viewModel = (SlideShowViewModel)eventArguments.OldValue;
				viewModel.PropertyChanged -= HandleImageChanged;
			}

			if (eventArguments.NewValue != null)
			{
				SlideShowViewModel viewModel = (SlideShowViewModel)eventArguments.NewValue;
				viewModel.PropertyChanged += HandleImageChanged;
			}
		}

		private void HandleImageChanged(object sender, PropertyChangedEventArgs eventArguments)
		{
			if (eventArguments.PropertyName == nameof(SlideShowViewModel.CurrentImage))
			{
				Dispatcher.BeginInvoke(new Action(() => lastImageChange = DateTime.Now));
			}
		}

		private void CycleImage(object sender, EventArgs eventArguments)
		{
			if (DataContext == null)
				return;

			SlideShowViewModel viewModel = (SlideShowViewModel)DataContext;
			if (viewModel.IsRunning == false)
				return;

			if (DateTime.Now > (lastImageChange + viewModel.Interval))
			{
				if (viewModel.NextCommand.CanExecute(null))
					viewModel.NextCommand.Execute(null);
			}
		}

		// Move focus back to the user control so that the input bindings work
		private void ResetFocus(object sender, MouseButtonEventArgs eventArguments)
		{
			Focus();
		}

		private void SubmitInterval(object sender, KeyEventArgs eventArguments)
		{
			if (eventArguments.Key == Key.Enter)
			{
				BindingOperations.GetBindingExpression((TextBox)sender, TextBox.TextProperty).UpdateSource();
				Focus(); // Move focus back to the user control so that the input bindings work
			}
		}
	}
}
