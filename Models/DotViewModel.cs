using System.ComponentModel;
using System.Runtime.CompilerServices;

public class DotViewModel : INotifyPropertyChanged
{
	private bool _isActive;
	public int Index { get; set; }

	public bool IsActive
	{
		get => _isActive;
		set
		{
			if (_isActive != value)
			{
				_isActive = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(FillBrush));
			}
		}
	}

	public System.Windows.Media.Brush FillBrush => IsActive ? System.Windows.Media.Brushes.DodgerBlue : System.Windows.Media.Brushes.Gray;

	public event PropertyChangedEventHandler? PropertyChanged;
	private void OnPropertyChanged([CallerMemberName] string? propName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}
