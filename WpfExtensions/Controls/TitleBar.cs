using System.Windows;
using System.Windows.Controls;

namespace Overmind.WpfExtensions.Controls
{
	public class TitleBar : ContentControl
	{
		public static readonly DependencyProperty IconProperty
			= DependencyProperty.Register(nameof(Icon), typeof(FrameworkElement), typeof(TitleBar));
		public static readonly DependencyProperty MenuProperty
			= DependencyProperty.Register(nameof(Menu), typeof(FrameworkElement), typeof(TitleBar));
		public static readonly DependencyProperty TitleProperty
			= DependencyProperty.Register(nameof(Title), typeof(FrameworkElement), typeof(TitleBar));

		public FrameworkElement Icon
		{
			get { return (FrameworkElement)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public FrameworkElement Menu
		{
			get { return (FrameworkElement)GetValue(MenuProperty); }
			set { SetValue(MenuProperty, value); }
		}

		public FrameworkElement Title
		{
			get { return (FrameworkElement)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			((Button)Template.FindName("MinimizeButton", this)).Click += Minimize;
			((Button)Template.FindName("MaximizeRestoreButton", this)).Click += MaximizeOrRestore;
			((Button)Template.FindName("CloseButton", this)).Click += Close;
		}

		private void Minimize(object sender, RoutedEventArgs eventArguments)
		{
			Window.GetWindow(this).WindowState = WindowState.Minimized;
		}

		private void MaximizeOrRestore(object sender, RoutedEventArgs eventArguments)
		{
			Window window = Window.GetWindow(this);

			switch (window.WindowState)
			{
				case WindowState.Normal: window.WindowState = WindowState.Maximized; break;
				case WindowState.Maximized: window.WindowState = WindowState.Normal; break;
				default: break;
			}
		}

		private void Close(object sender, RoutedEventArgs eventArguments)
		{
			Window.GetWindow(this).Close();
		}
	}
}
