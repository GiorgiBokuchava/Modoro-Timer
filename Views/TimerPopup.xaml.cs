using Modoro_Timer.Models;
using Modoro_Timer.Properties;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace Modoro_Timer.Views
{
	public partial class TimerPopup : FluentWindow
	{
		public TimerPopup() : this(new ModoroManager()) { }

		public TimerPopup(ModoroManager manager)
		{
			InitializeComponent();
			DataContext = manager;

			this.LocationChanged += TimerPopup_LocationChanged;
			this.PreviewKeyDown += TimerPopup_PreviewKeyDown;
		}

		private void TimerPopup_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				this.Hide();
				e.Handled = true;
			}
		}

		private void TimerPopup_LocationChanged(object? sender, EventArgs e)
		{
			// save new position
			Settings.Default.PopupLeft = this.Left;
			Settings.Default.PopupTop = this.Top;
			Settings.Default.Save();
		}
		private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			// If the click landed on one of your ui:Buttons, do nothing special
			if (e.OriginalSource is ButtonBase)
				return;

			// 1) Clear the focused button
			Keyboard.ClearFocus();

			// 2) Re-focus the window so it still receives key events (ESC, etc.)
			//    Make sure your FluentWindow has Focusable="True" in XAML!
			this.Focus();
		}
	}
}