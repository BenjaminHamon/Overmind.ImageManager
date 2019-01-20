using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class SlideShowView : UserControl
	{
		public SlideShowView()
		{
			InitializeComponent();
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
