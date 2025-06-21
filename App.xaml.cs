using Modoro_Timer.Models;
using Modoro_Timer.Properties;
using Modoro_Timer.Services;
using Modoro_Timer.Views;
using NHotkey.Wpf;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Point = System.Drawing.Point;

namespace Modoro_Timer
{
	[SupportedOSPlatform("windows6.1")]
	public partial class App : System.Windows.Application
	{
		private NotifyIcon _trayIcon = null!;
		private TimerPopup _popup = null!;
		private ModoroManager _modoro = null!;
		private Point _lastClickPx;
		internal int LastTrayClickTick;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			_modoro = new ModoroManager();
			_modoro.OnTickUpdate += UpdateTooltip;

			_popup = new TimerPopup(_modoro);
			_popup.Hide();

			_trayIcon = new NotifyIcon
			{
				Icon = new Icon("Assets/icon.ico"),
				Text = "Modoro Timer",
				Visible = true
			};

			_lastClickPx = new Point(
				Settings.Default.LastClickX,
				Settings.Default.LastClickY
			);

			_trayIcon.MouseUp += (s, args) =>
			{
				if (args.Button != MouseButtons.Left)
					return;

				_lastClickPx = Control.MousePosition;

				Settings.Default.LastClickX = _lastClickPx.X;
				Settings.Default.LastClickY = _lastClickPx.Y;
				Settings.Default.Save();

				TogglePopupAt(_lastClickPx);
			};

			new TrayService(_modoro, _trayIcon);


			HotkeyManager.Current.AddOrReplace(
				"TogglePopup",
				Key.F, ModifierKeys.Alt,
				(s, a) => TogglePopupAt(_lastClickPx)
			);
		}

		private void TogglePopupAt(Point clickPx)
		{
			if (_popup.IsVisible)
			{
				_popup.Hide();
				return;
			}

			var screen = Screen.FromPoint(clickPx);
			var wa = screen.WorkingArea;
			var dpiInfo = VisualTreeHelper.GetDpi(_popup);
			double sx = dpiInfo.DpiScaleX;
			double sy = dpiInfo.DpiScaleY;
			var clickDip = new System.Windows.Point(
				clickPx.X / sx,
				clickPx.Y / sy
			);

			double leftDip = clickDip.X - (_popup.Width / 2) - 5;
			double minX = wa.Left / sx;
			double maxX = (wa.Right / sx) - _popup.Width;
			leftDip = Math.Max(minX, Math.Min(leftDip, maxX));

			double bottomDip = (wa.Bottom / sy) - 5;
			double topDip = bottomDip - _popup.Height;

			_popup.ShowActivated = true;
			_popup.Left = leftDip;
			_popup.Top = topDip;
			_popup.Topmost = true;
			_popup.Show();
			_popup.Focus();
			_popup.Activate();
		}

		private void UpdateTooltip(string timeLeft)
		{
			_trayIcon.Text = $"Modoro Timer – {timeLeft} remaining";
		}

		protected override void OnExit(ExitEventArgs e)
		{
			HotkeyManager.Current.Remove("TogglePopup");
			_trayIcon.Dispose();
			base.OnExit(e);
		}
	}
}
