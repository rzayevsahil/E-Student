using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Threading;

namespace DocumentSearch.ViewModels;

public partial class PomodoroViewModel : ObservableObject
{
    private DispatcherTimer? _timer;
    private TimeSpan _remainingTime;
    private TimeSpan _workDuration = TimeSpan.FromMinutes(25);
    private TimeSpan _shortBreakDuration = TimeSpan.FromMinutes(5);
    private TimeSpan _longBreakDuration = TimeSpan.FromMinutes(15);
    private int _completedPomodoros = 0;
    private PomodoroState _currentState = PomodoroState.Idle;

    [ObservableProperty]
    private string timeDisplay = "25:00";

    [ObservableProperty]
    private string statusText = "Pomodoro Tekniği";

    [ObservableProperty]
    private string buttonText = "Başlat";

    [ObservableProperty]
    private bool isRunning = false;

    [ObservableProperty]
    private int completedCount = 0;

    [ObservableProperty]
    private string currentPhase = "Çalışma";

    [ObservableProperty]
    private string infoText = "Pomodoro Tekniği, odaklanmayı artırmak için 25 dakikalık çalışma ve kısa molalar kullanır.";

    public PomodoroViewModel()
    {
        _remainingTime = _workDuration;
        UpdateTimeDisplay();
    }

    [RelayCommand]
    private void StartPause()
    {
        if (_currentState == PomodoroState.Idle)
        {
            StartWork();
        }
        else if (IsRunning)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    [RelayCommand]
    private void Reset()
    {
        StopTimer();
        _currentState = PomodoroState.Idle;
        _remainingTime = _workDuration;
        IsRunning = false;
        ButtonText = "Başlat";
        CurrentPhase = "Çalışma";
        UpdateTimeDisplay();
        StatusText = "Pomodoro Tekniği";
    }

    [RelayCommand]
    private void Skip()
    {
        if (_currentState == PomodoroState.Working)
        {
            CompleteWork();
        }
        else if (_currentState == PomodoroState.ShortBreak || _currentState == PomodoroState.LongBreak)
        {
            StartWork();
        }
    }

    private void StartWork()
    {
        _currentState = PomodoroState.Working;
        _remainingTime = _workDuration;
        CurrentPhase = "Çalışma";
        StartTimer();
        StatusText = "Çalışma zamanı! Odaklanın.";
    }

    private void CompleteWork()
    {
        _completedPomodoros++;
        CompletedCount = _completedPomodoros;
        
        StopTimer();
        IsRunning = false;
        ButtonText = "Başlat";

        // Her 4 pomodoroda bir uzun mola
        if (_completedPomodoros % 4 == 0)
        {
            StartLongBreak();
        }
        else
        {
            StartShortBreak();
        }
    }

    private void StartShortBreak()
    {
        _currentState = PomodoroState.ShortBreak;
        _remainingTime = _shortBreakDuration;
        CurrentPhase = "Kısa Mola";
        StatusText = "Kısa mola zamanı. Rahatlayın!";
        UpdateTimeDisplay();
    }

    private void StartLongBreak()
    {
        _currentState = PomodoroState.LongBreak;
        _remainingTime = _longBreakDuration;
        CurrentPhase = "Uzun Mola";
        StatusText = "Uzun mola zamanı. İyi dinlenin!";
        UpdateTimeDisplay();
    }

    private void Pause()
    {
        StopTimer();
        IsRunning = false;
        ButtonText = "Devam Et";
        StatusText = "Duraklatıldı";
    }

    private void Resume()
    {
        StartTimer();
        IsRunning = true;
        ButtonText = "Duraklat";
        if (_currentState == PomodoroState.Working)
        {
            StatusText = "Çalışma zamanı! Odaklanın.";
        }
        else
        {
            StatusText = "Mola zamanı. Rahatlayın!";
        }
    }

    private void StartTimer()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
        IsRunning = true;
        ButtonText = "Duraklat";
    }

    private void StopTimer()
    {
        if (_timer != null)
        {
            _timer.Tick -= Timer_Tick;
            _timer.Stop();
            _timer = null;
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_remainingTime.TotalSeconds > 0)
        {
            _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
            UpdateTimeDisplay();
        }
        else
        {
            OnTimerComplete();
        }
    }

    private void OnTimerComplete()
    {
        StopTimer();
        IsRunning = false;
        ButtonText = "Başlat";

        if (_currentState == PomodoroState.Working)
        {
            CompleteWork();
            System.Media.SystemSounds.Asterisk.Play();
        }
        else if (_currentState == PomodoroState.ShortBreak || _currentState == PomodoroState.LongBreak)
        {
            StartWork();
            System.Media.SystemSounds.Asterisk.Play();
        }
    }

    private void UpdateTimeDisplay()
    {
        TimeDisplay = $"{_remainingTime.Minutes:D2}:{_remainingTime.Seconds:D2}";
    }

    private enum PomodoroState
    {
        Idle,
        Working,
        ShortBreak,
        LongBreak
    }
}

