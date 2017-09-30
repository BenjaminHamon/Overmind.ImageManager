using System;
using System.Windows.Input;

namespace Overmind.WpfExtensions
{
	/// <summary>
	/// ICommand implementation for MVVM.
	/// See http://wpftutorial.net/DelegateCommand.html
	/// </summary>
	/// <typeparam name="TParameter">Type of parameter passed to the command, use object if there is none.</typeparam>
	public class DelegateCommand<TParameter> : ICommand
	{
		public DelegateCommand(Action<TParameter> execute, Predicate<TParameter> canExecute = null)
		{
			this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
			this.canExecute = canExecute;
		}

		private readonly Action<TParameter> execute;
		private readonly Predicate<TParameter> canExecute;
		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter) { execute((TParameter)parameter); }
		public bool CanExecute(object parameter) { return canExecute == null ? true : canExecute((TParameter)parameter); }
		public void RaiseCanExecuteChanged() { CanExecuteChanged?.Invoke(this, EventArgs.Empty); }
	}
}
