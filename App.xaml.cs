using Modoro_Timer.Models;
using Modoro_Timer.Properties;
using Modoro_Timer.Services;
using Modoro_Timer.Views;
using NHotkey.Wpf;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms; // Add this for ContextMenuStrip
using Control = System.Windows.Forms.Control;
using MouseButtons = System.Windows.Forms.MouseButtons;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using Point = System.Drawing.Point;
using Screen = System.Windows.Forms.Screen;

namespace Modoro_Timer
{
	[SupportedOSPlatform("windows6.1")]
	public partial class App : System.Windows.Application
	{
		private NotifyIcon _trayIcon = null!;
		private TimerPopup _popup = null!;
		private ModoroManager _modoro = null!;
		private Point _lastClickPx;
		private DateTime _popupHideTime = DateTime.MinValue;
		private ContextMenuStrip _trayMenu = null!;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			_modoro = new ModoroManager();
			_modoro.OnTickUpdate += UpdateTooltip;
			_popup = new TimerPopup(_modoro);

			// Add notifications for session completion
			_modoro.OnSessionCompleted += sessionType =>
			{
				string message = sessionType switch
				{
					ModoroSessionType.Focus => "Focus session complete! Time for a break.",
					ModoroSessionType.ShortBreak => "Short break over! Back to focus.",
					ModoroSessionType.LongBreak => "Long break over! Back to focus.",
					_ => "Session complete!"
				};
				_trayIcon.ShowBalloonTip(3000, "Modoro Timer", message, ToolTipIcon.Info);
			};

			// Outside popup click behavior
			_popup.Deactivated += (s, args) =>
			{
				_popup.Hide();
				_popupHideTime = DateTime.Now;
			};

			// Context menu for tray icon
			_trayMenu = new ContextMenuStrip();
			var showTimerItem = new ToolStripMenuItem("Show Timer");
			showTimerItem.Click += (s, args) =>
			{
				ShowPopupAt(new Point(Settings.Default.LastClickX, Settings.Default.LastClickY));
			};
			var quitItem = new ToolStripMenuItem("Quit");
			quitItem.Click += (s, args) => Shutdown();

			_trayMenu.Items.Add(showTimerItem);
			_trayMenu.Items.Add(new ToolStripSeparator());
			_trayMenu.Items.Add(quitItem);

			_trayIcon = new NotifyIcon
			{
				Icon = new System.Drawing.Icon("Assets/icon.ico"),
				Text = "Modoro Timer",
				Visible = true,
				ContextMenuStrip = _trayMenu
			};

			// Tray icon click behavior
			_trayIcon.MouseDown += (s, args) =>
			{
				if (args.Button == MouseButtons.Right)
				{
					_trayMenu.Show(Control.MousePosition);
					return;
				}
				if (args.Button != MouseButtons.Left)
					return;

				_lastClickPx = Control.MousePosition;
				Settings.Default.LastClickX = _lastClickPx.X;
				Settings.Default.LastClickY = _lastClickPx.Y;
				Settings.Default.Save();

				if (!_popup.IsVisible &
					(DateTime.Now.Subtract(_popupHideTime).TotalMilliseconds > 500))
				{
					ShowPopupAt(_lastClickPx);
				}
				else if (_popup.IsVisible)
				{
					_popup.Hide();
				}
			};

			new TrayService(_modoro, _trayIcon);

			HotkeyManager.Current.AddOrReplace(
				"TogglePopup",
				Key.F, ModifierKeys.Alt,
				(s, args) => Dispatcher.Invoke(() =>
				{
					if (_popup.IsVisible)
						_popup.Hide();
					else
						ShowPopupAt(new Point(Settings.Default.LastClickX, Settings.Default.LastClickY));
				})
			);
		}

		private void ShowPopupAt(Point clickPx)
		{
			var screen = Screen.FromPoint(clickPx);
			var wa = screen.WorkingArea;
			var dpiInfo = VisualTreeHelper.GetDpi(_popup);
			double sx = dpiInfo.DpiScaleX, sy = dpiInfo.DpiScaleY;

			var clickDip = new System.Windows.Point(
				clickPx.X / sx,
				clickPx.Y / sy
			);

			double leftDip = clickDip.X - (_popup.Width / 2) - 5;
			double minX = wa.Left / sx;
			double maxX = (wa.Right / sx) - _popup.Width;
			leftDip = Math.Max(minX, Math.Min(leftDip, maxX));

			double bottomDip = wa.Bottom / sy;
			double topDip = bottomDip - _popup.Height - 10;

			_popup.Left = leftDip;
			_popup.Top = topDip;
			_popup.ShowActivated = true;
			_popup.Topmost = true;
			_popup.Show();
			_popup.Focus();
			_popup.Activate();

			// Probably redundant
			_popup.MouseEnter -= Popup_Interaction;
			_popup.PreviewMouseLeftButtonDown -= Popup_Interaction;
			_popup.MouseEnter += Popup_Interaction;
			_popup.PreviewMouseLeftButtonDown += Popup_Interaction;
		}

		private void Popup_Interaction(object? sender, EventArgs e)
		{
			_popup.MouseEnter -= Popup_Interaction;
			_popup.PreviewMouseLeftButtonDown -= Popup_Interaction;
			_popup.Activate();
			_popup.Focus();
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