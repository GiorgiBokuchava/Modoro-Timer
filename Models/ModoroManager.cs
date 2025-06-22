using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace Modoro_Timer.Models
{
	public enum ModoroSessionType { Focus, ShortBreak, LongBreak }

	public class ModoroManager : INotifyPropertyChanged
	{
		private readonly DispatcherTimer _timer;
		private TimeSpan _remainingTime;
		private int _sessionIndex;
		private bool _isRunning;

		public event PropertyChangedEventHandler? PropertyChanged;
		public event Action<string> OnTickUpdate = delegate { };
		public event Action? OnReset;

		public string RemainingTime => _remainingTime.ToString(@"mm\:ss");
		public ModoroSessionType CurrentSessionType { get; private set; }
		public string PauseResumeText => _isRunning ? "⏸ Pause" : "▶ Resume";

		private int _currentSessionIndex;
		public int CurrentSessionIndex
		{
			get => _currentSessionIndex;
			private set
			{
				if (_currentSessionIndex == value) return;
				_currentSessionIndex = value;
				OnPropertyChanged();
				UpdateDotStates();
			}
		}

		private double _progressFraction;
		public double ProgressFraction
		{
			get => _progressFraction;
			private set
			{
				if (_progressFraction == value) return;
				_progressFraction = value;
				OnPropertyChanged(nameof(ProgressFraction));
			}
		}

		public ObservableCollection<DotViewModel> SessionDots { get; } = new();

		public string SessionTypeDisplay => CurrentSessionType switch
		{
			ModoroSessionType.Focus => "Focus",
			ModoroSessionType.ShortBreak => "Short Break",
			ModoroSessionType.LongBreak => "Long Break",
			_ => CurrentSessionType.ToString()
		};

		// Commands
		public ICommand TogglePauseResumeCommand { get; }
		public ICommand SkipCommand { get; }
		public ICommand ResetCommand { get; }
		public ICommand StopCommand { get; }

		// Durations
		private readonly TimeSpan _focusDuration = TimeSpan.FromMinutes(25);
		private readonly TimeSpan _shortBreakDuration = TimeSpan.FromMinutes(5);
		private readonly TimeSpan _longBreakDuration = TimeSpan.FromMinutes(15);

		public ModoroManager()
		{
			_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
			_timer.Tick += OnTick;

			TogglePauseResumeCommand = new RelayCommand(_ => TogglePauseResume());
			SkipCommand = new RelayCommand(_ => Skip());
			ResetCommand = new RelayCommand(_ => Reset());
			StopCommand = new RelayCommand(_ => Stop());

			// initialize dots
			for (int i = 0; i < 4; i++)
				SessionDots.Add(new DotViewModel { Index = i, IsActive = i == 0 });

			StartNextSession();
		}

		private void OnTick(object? sender, EventArgs e)
		{
			if (_remainingTime > TimeSpan.Zero)
			{
				// subtract elapsed time
				_remainingTime -= TimeSpan.FromMilliseconds(100);

				// compute total & elapsed seconds for this session
				double total = CurrentSessionType switch
				{
					ModoroSessionType.Focus => _focusDuration.TotalSeconds,
					ModoroSessionType.ShortBreak => _shortBreakDuration.TotalSeconds,
					_ => _longBreakDuration.TotalSeconds
				};
				double elapsed = total - _remainingTime.TotalSeconds;

				// update your new ProgressFraction here:
				ProgressFraction = Math.Clamp(elapsed / total, 0, 1);

				// continue with your existing updates
				OnTickUpdate(_remainingTime.ToString(@"mm\:ss"));
				RaiseAllProperties();
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

			int dotIdx = Math.Clamp((_sessionIndex + 1) / 2 - 1, 0, SessionDots.Count - 1);
			CurrentSessionIndex = dotIdx;

			if (_sessionIndex % 2 == 1)
			{
				CurrentSessionType = ModoroSessionType.Focus;
				_remainingTime = _focusDuration;
			}
			else if (_sessionIndex < 7)
			{
				CurrentSessionType = ModoroSessionType.ShortBreak;
				_remainingTime = _shortBreakDuration;
			}
			else
			{
				CurrentSessionType = ModoroSessionType.LongBreak;
				_remainingTime = _longBreakDuration;
				_sessionIndex = 0;  // restart after long break
			}

			ProgressFraction = 0;

			OnTickUpdate(_remainingTime.ToString(@"mm\:ss"));
			RaiseAllProperties();
		}

		// Controls
		public void Start()
		{
			if (!_isRunning)
			{
				_isRunning = true;
				_timer.Start();
				RaiseAllProperties();
			}
		}

		public void Pause()
		{
			if (_isRunning)
			{
				_isRunning = false;
				_timer.Stop();
				RaiseAllProperties();
			}
		}

		public void Resume() => Start();

		public void Stop()
		{
			if (_isRunning)
			{
				_isRunning = false;
				_timer.Stop();
				RaiseAllProperties();
			}
		}

		public void TogglePauseResume()
		{
			if (_isRunning) Pause();
			else Resume();
		}

		public void Skip()
		{
			// always skip, even when paused
			Stop();
			StartNextSession();
		}

		public void Reset()
		{
			// stop & restore the current session's full duration
			Stop();
			switch (CurrentSessionType)
			{
				case ModoroSessionType.Focus:
					_remainingTime = _focusDuration;
					break;
				case ModoroSessionType.ShortBreak:
					_remainingTime = _shortBreakDuration;
					break;
				case ModoroSessionType.LongBreak:
					_remainingTime = _longBreakDuration;
					break;
			}

			ProgressFraction = 0;

			OnTickUpdate(_remainingTime.ToString(@"mm\:ss"));
			RaiseAllProperties();
			OnReset?.Invoke();
		}

		// Helpers
		private void UpdateDotStates()
		{
			for (int i = 0; i < SessionDots.Count; i++)
				SessionDots[i].IsActive = (i == CurrentSessionIndex);
		}

		public int GetRemainingSeconds()
		{
			return (int)_remainingTime.TotalSeconds;
		}

		private void RaiseAllProperties()
		{
			OnPropertyChanged(nameof(RemainingTime));
			OnPropertyChanged(nameof(CurrentSessionType));
			OnPropertyChanged(nameof(SessionTypeDisplay));
			OnPropertyChanged(nameof(PauseResumeText));
		}

		protected void OnPropertyChanged([CallerMemberName] string? name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
