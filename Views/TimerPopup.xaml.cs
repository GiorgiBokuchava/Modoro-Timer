using Modoro_Timer.Models;
using System.Windows;
using System.Windows.Input;

namespace Modoro_Timer.Views;
/// <summary>
/// Interaction logic for TimerPopup.xaml
/// </summary>
public partial class TimerPopup : Window
{
	public TimerPopup() : this(new ModoroManager()) { }
	private App App => (App)System.Windows.Application.Current;

	public TimerPopup(ModoroManager manager)
	{
		InitializeComponent();
		DataContext = manager;

		// Hide window when it loses focus (user clicks elsewhere)
		this.Deactivated += TimerPopup_Deactivated;
		this.PreviewKeyDown += TimerPopup_PreviewKeyDown;
	}

	internal void TimerPopup_Deactivated(object? sender, EventArgs e)
	{
		Console.WriteLine("Deactivated triggered");
		this.Hide();
	}

	internal void TimerPopup_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			this.Hide();
			e.Handled = true;
		}
	}
}
