using Modoro_Timer.Models;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.Versioning;

namespace Modoro_Timer.Services
{
	[SupportedOSPlatform("windows6.1")]
	public class TrayService : IDisposable
	{
		private readonly NotifyIcon _trayIcon;
		private readonly ModoroManager _manager;
		private readonly Icon _defaultIcon;
		private Icon? _prevIcon;

		public TrayService(ModoroManager manager, NotifyIcon trayIcon)
		{
			_manager = manager;
			_trayIcon = trayIcon;
			_defaultIcon = new Icon("Assets/icon.ico");

			_manager.OnTickUpdate += time =>
			{
				_trayIcon.Text = $"Modoro Timer – {time} remaining";

				double total = _manager.CurrentSessionType switch
				{
					ModoroSessionType.Focus => 25 * 60,
					ModoroSessionType.ShortBreak => 5 * 60,
					_ => 15 * 60
				};
				double elapsed = total - _manager.GetRemainingSeconds();
				double fraction = Math.Clamp(elapsed / total, 0, 1);

				var icon = CreateProgressIcon(fraction);
				_trayIcon.Icon = icon;
				_prevIcon?.Dispose();
				_prevIcon = icon;
			};

			_manager.OnReset += () =>
			{
				_trayIcon.Icon = _defaultIcon;
				_prevIcon?.Dispose();
				_prevIcon = null;
			};
		}

		private Icon CreateProgressIcon(double fraction)
		{
			const int size = 32, thickness = 5;
			using var bmp = new Bitmap(size, size);
			using var g = Graphics.FromImage(bmp);

			g.Clear(Color.Transparent);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

			using var bgPen = new Pen(Color.Gray, thickness);
			g.DrawEllipse(bgPen, thickness / 2, thickness / 2, size - thickness, size - thickness);

			using var fgPen = new Pen(Color.Cyan, thickness)
			{
				StartCap = LineCap.Round,
				EndCap = LineCap.Round
			};
			float sweep = (float)(fraction * 360);
			g.DrawArc(fgPen, thickness / 2, thickness / 2, size - thickness, size - thickness, -90, sweep);

			IntPtr hIcon = bmp.GetHicon();
			var icon = Icon.FromHandle(hIcon);
			var clone = (Icon)icon.Clone();
			icon.Dispose();
			return clone;
		}

		public void Dispose()
		{
			_prevIcon?.Dispose();
			_defaultIcon?.Dispose();
		}
	}
}
