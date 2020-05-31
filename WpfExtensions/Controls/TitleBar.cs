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
	}
}
