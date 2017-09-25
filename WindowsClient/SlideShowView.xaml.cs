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

		private void SubmitInterval(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				BindingOperations.GetBindingExpression((TextBox)sender, TextBox.TextProperty).UpdateSource();
				Focus();
			}
		}
	}
}
