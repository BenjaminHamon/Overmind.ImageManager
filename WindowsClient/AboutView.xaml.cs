using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Overmind.ImageManager.WindowsClient
{
	public partial class AboutView : UserControl
	{
		private static readonly Logger Logger = LogManager.GetLogger(nameof(AboutView));

		public AboutView()
		{
			InitializeComponent();

			Loaded += InitializeContent;
		}

		private void InitializeContent(object sender, RoutedEventArgs eventArguments)
		{
			Loaded -= InitializeContent;

			string aboutFilePath = Path.Combine(WindowsApplication.InstallationDirectory, "About.md");
			string aboutText
				= String.Format("### {0} {1}", WindowsApplication.ApplicationTitle, WindowsApplication.ApplicationFullVersion)
				+ Environment.NewLine + Environment.NewLine
				+ WindowsApplication.ApplicationCopyright
				+ Environment.NewLine + Environment.NewLine;

			try
			{
				aboutText += File.ReadAllText(aboutFilePath);
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Failed to load about file (Path: '{0}')", aboutFilePath);
			}

			viewer.Markdown = aboutText;
		}

		private void NavigateToLink(object sender, ExecutedRoutedEventArgs eventArguments)
		{
			Uri uri = new Uri((string)eventArguments.Parameter);

			using (Process process = Process.Start(uri.ToString())) { }
		}
	}
}
