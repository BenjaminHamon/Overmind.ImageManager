using System.ComponentModel;

namespace Overmind.ImageManager.WindowsClient.Extensions
{
	public class ObservableString : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private string valueField;
		public string Value
		{
			get { return valueField; }
			set
			{
				valueField = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
			}
		}
	}
}
