using System.ComponentModel;
using System.Windows;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void ExitApplication(object sender, CancelEventArgs eventArguments)
		{
			Application.Current.Shutdown();
		}
	}
}
