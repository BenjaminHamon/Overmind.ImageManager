using System.ComponentModel;
using System.Windows;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Closing += ExitApplication;
		}

		private void ExitApplication(object sender, CancelEventArgs eventArguments)
		{
			mainView.ExitApplication(sender, eventArguments);
			if (eventArguments.Cancel == false)
				Closing -= ExitApplication;
		}
	}
}
