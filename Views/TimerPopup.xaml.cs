using Modoro_Timer.Models;
using Modoro_Timer.Properties;
using System.Windows;
using System.Windows.Input;

namespace Modoro_Timer.Views
{
	public partial class TimerPopup : Window
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
	}
}