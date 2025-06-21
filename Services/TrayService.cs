using Modoro_Timer.Models;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Modoro_Timer.Services
{
	public class TrayService
	{
		private readonly NotifyIcon _trayIcon;
		private readonly ModoroManager _manager;
		private Icon? _prevIcon;

		public TrayService(ModoroManager manager, NotifyIcon trayIcon)
		{
			_manager = manager;
			_trayIcon = trayIcon;

			_manager.OnTickUpdate += time =>
			{
				_trayIcon.Text = $"Modoro Timer - {time} remaining";

				double total = _manager.CurrentSessionType switch
				{
					ModoroSessionType.Focus => 25 * 60,
					ModoroSessionType.ShortBreak => 5 * 60,
					_ => 15 * 60
				};
				double elapsed = total - _manager.GetRemainingSeconds();
				double fraction = Math.Clamp(elapsed / total, 0, 1);

				UpdateIcon(fraction);
			};
		}

		private Icon CreateProgressIcon(double fraction)
		{
			const int size = 32, thickness = 5;

			using var bitmap = new Bitmap(size, size);
			using var g = Graphics.FromImage(bitmap);

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

			IntPtr hIcon = bitmap.GetHicon();
			var icon = Icon.FromHandle(hIcon);
			var clone = (Icon)icon.Clone();
			icon.Dispose();
			return clone;
		}

		private void UpdateIcon(double fraction)
		{
			var newIcon = CreateProgressIcon(fraction);
			_trayIcon.Icon = newIcon;
			_prevIcon?.Dispose();
			_prevIcon = newIcon;
		}
	}
}
