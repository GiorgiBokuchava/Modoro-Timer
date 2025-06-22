using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace Modoro_Timer.Controls
{
	public partial class CircularProgressRing : System.Windows.Controls.UserControl
	{
		public static readonly DependencyProperty FractionProperty =
			DependencyProperty.Register(nameof(Fraction), typeof(double), typeof(CircularProgressRing),
				new PropertyMetadata(0.0, OnFractionChanged));

		/// <summary>
		/// 0.0–1.0 progress of the ring.
		/// </summary>
		public double Fraction
		{
			get => (double)GetValue(FractionProperty);
			set => SetValue(FractionProperty, value);
		}

		public CircularProgressRing()
		{
			InitializeComponent();
			SizeChanged += (_, __) => UpdateArc();
		}

		private static void OnFractionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			=> ((CircularProgressRing)d).UpdateArc();

		private void UpdateArc()
		{
			double f = Math.Clamp(Fraction, 0, 1);

			if (f <= 0)
			{
				PART_Arc.Data = null;
				return;
			}

			double startAngle = -90;
			double sweep = f * 360;
			double radius = (ActualWidth < ActualHeight ? ActualWidth : ActualHeight) / 2 - (5 / 2);

			if (radius <= 0) return;

			Point center = new(ActualWidth / 2, ActualHeight / 2);
			Point start = PointOnCircle(center, radius, startAngle);
			Point end = PointOnCircle(center, radius, startAngle + sweep);

			bool largeArc = sweep > 180;
			var segment = new ArcSegment(end,
										 new System.Windows.Size(radius, radius),
										 0,
										 largeArc,
										 SweepDirection.Clockwise,
										 true);

			var figure = new PathFigure(start, new[] { segment }, false);
			PART_Arc.Data = new PathGeometry(new[] { figure });
		}

		private static Point PointOnCircle(Point c, double r, double angleDeg)
		{
			double rad = angleDeg * Math.PI / 180;
			return new(c.X + r * Math.Cos(rad),
					   c.Y + r * Math.Sin(rad));
		}
	}
}