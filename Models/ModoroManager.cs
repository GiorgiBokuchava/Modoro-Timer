using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace Modoro_Timer.Models;

public enum ModoroSessionType { Focus, ShortBreak, LongBreak }

public class ModoroManager : INotifyPropertyChanged
{
	private readonly DispatcherTimer _timer;
	private TimeSpan _remainingTime;
	private int _sessionIndex;
	private bool _isRunning;

	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string? propName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

	public event Action<string> OnTickUpdate = delegate { };

	public string RemainingTime
	{
		get => _remainingTime.ToString(@"mm\:ss");
	}
	public ModoroSessionType CurrentSessionType { get; private set; }
	public ICommand TogglePauseResumeCommand { get; }
	public ICommand StopCommand { get; }
	public ICommand SkipCommand { get; }

	public string SessionTypeDisplay =>
	CurrentSessionType switch
	{
		ModoroSessionType.Focus => "Focus",
		ModoroSessionType.ShortBreak => "Short Break",
		ModoroSessionType.LongBreak => "Long Break",
		_ => CurrentSessionType.ToString()
	};

	public int CurrentSessionDotIndex
	{
		get
		{
			int focusCount = (_sessionIndex + 1) / 2;
			return Math.Clamp(focusCount - 1, 0, 3);
		}
	}

	public ObservableCollection<DotViewModel> SessionDots { get; } = new();

	private readonly TimeSpan _focusDuration = TimeSpan.FromMinutes(25);
	private readonly TimeSpan _shortBreakDuration = TimeSpan.FromMinutes(5);
	private readonly TimeSpan _longBreakDuration = TimeSpan.FromMinutes(15);

	public ModoroManager()
	{
		_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
		_timer.Tick += OnTick;
		StartNextSession();

		TogglePauseResumeCommand = new RelayCommand(_ => TogglePauseResume());
		StopCommand = new RelayCommand(_ => Stop());
		SkipCommand = new RelayCommand(_ => Skip());

		for (int i = 0; i < 4; i++)
			SessionDots.Add(new DotViewModel { Index = i, IsActive = i == 0 });

	}

	private void UpdateDotStates()
	{
		for (int i = 0; i < SessionDots.Count; i++)
			SessionDots[i].IsActive = (i == CurrentSessionIndex);
	}


	private void OnTick(object? sender, EventArgs e)
	{
		if (_remainingTime > TimeSpan.Zero)
		{
			_remainingTime -= TimeSpan.FromSeconds(1);
			OnTickUpdate?.Invoke(_remainingTime.ToString(@"mm\:ss"));
			NotifyChange();
		}
		else
		{
			Stop();
			StartNextSession();
		}
	}

	public void StartNextSession()
	{
		_sessionIndex++;

		int newIndex = (_sessionIndex + 1) / 2 - 1;
		CurrentSessionIndex = Math.Clamp(newIndex, 0, 3);

		if (_sessionIndex % 2 == 1)
		{
			CurrentSessionType = ModoroSessionType.Focus;
			_remainingTime = TimeSpan.FromMinutes(25);
		}
		else if (_sessionIndex < 7)
		{
			CurrentSessionType = ModoroSessionType.ShortBreak;
			_remainingTime = TimeSpan.FromMinutes(5);
		}
		else
		{
			CurrentSessionType = ModoroSessionType.LongBreak;
			_remainingTime = TimeSpan.FromMinutes(15);
			_sessionIndex = 0; // Reset after long break
		}

		NotifyChange();
		OnTickUpdate?.Invoke(_remainingTime.ToString(@"mm\:ss"));
	}

	public void Start()
	{
		if (!_isRunning)
		{
			_isRunning = true;
			_timer.Start();
			NotifyChange();
		}
	}

	public void Pause()
	{
		if (_isRunning)
		{
			_isRunning = false;
			_timer.Stop();
			NotifyChange();
		}
	}

	public void Resume()
	{
		if (!_isRunning)
		{
			_isRunning = true;
			_timer.Start();
			NotifyChange();
		}
	}

	public void Stop()
	{
		if (_isRunning)
		{
			_isRunning = false;
			_timer.Stop();
			NotifyChange();
		}
	}

	public void Skip()
	{
		if (_isRunning)
		{
			Stop();
			StartNextSession();
		}
	}

	public void TogglePauseResume()
	{
		if (_isRunning)
		{
			Pause();
		}
		else
		{
			Resume();
		}
	}

	private void NotifyChange()
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingTime)));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSessionType)));
		PropertyChanged?.Invoke(this, new(nameof(SessionTypeDisplay)));
		PropertyChanged?.Invoke(this, new(nameof(CurrentSessionDotIndex)));
	}

	public int GetRemainingSeconds()
	{
		return (int)_remainingTime.TotalSeconds;
	}
}