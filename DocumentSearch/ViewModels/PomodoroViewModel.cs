using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentSearch.Services;
using System;
using System.Windows.Threading;

namespace DocumentSearch.ViewModels;

public partial class PomodoroViewModel : ObservableObject
{
    private readonly LanguageService _languageService;
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
    private string statusText = string.Empty;

    [ObservableProperty]
    private string buttonText = string.Empty;

    [ObservableProperty]
    private bool isRunning = false;

    [ObservableProperty]
    private int completedCount = 0;

    [ObservableProperty]
    private string currentPhase = string.Empty;

    [ObservableProperty]
    private string infoText = string.Empty;

    public PomodoroViewModel(LanguageService languageService)
    {
        _languageService = languageService;
        _languageService.LanguageChanged += (s, lang) => RefreshTexts();

        _remainingTime = _workDuration;
        UpdateTimeDisplay();
        RefreshTexts();
    }

    private void RefreshTexts()
    {
        InfoText = _languageService.GetString("Pomo_InfoText");

        switch (_currentState)
        {
            case PomodoroState.Idle:
                CurrentPhase = _languageService.GetString("Pomo_Phase_Work");
                StatusText = _languageService.GetString("Pomo_Status_Idle");
                ButtonText = _languageService.GetString("Pomo_Start");
                break;
            case PomodoroState.Working:
                CurrentPhase = _languageService.GetString("Pomo_Phase_Work");
                ButtonText = IsRunning ? _languageService.GetString("Pomo_Pause") : _languageService.GetString("Pomo_Resume");
                StatusText = IsRunning ? _languageService.GetString("Pomo_Status_Work") : _languageService.GetString("Pomo_Status_Paused");
                break;
            case PomodoroState.ShortBreak:
                CurrentPhase = _languageService.GetString("Pomo_Phase_ShortBreak");
                ButtonText = IsRunning ? _languageService.GetString("Pomo_Pause") : _languageService.GetString("Pomo_Resume");
                StatusText = IsRunning ? _languageService.GetString("Pomo_Status_ShortBreak") : _languageService.GetString("Pomo_Status_Paused");
                break;
            case PomodoroState.LongBreak:
                CurrentPhase = _languageService.GetString("Pomo_Phase_LongBreak");
                ButtonText = IsRunning ? _languageService.GetString("Pomo_Pause") : _languageService.GetString("Pomo_Resume");
                StatusText = IsRunning ? _languageService.GetString("Pomo_Status_LongBreak") : _languageService.GetString("Pomo_Status_Paused");
                break;
        }
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
        UpdateTimeDisplay();
        RefreshTexts();
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
        StartTimer();
    }

    private void CompleteWork()
    {
        _completedPomodoros++;
        CompletedCount = _completedPomodoros;
        
        StopTimer();
        IsRunning = false;

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
        UpdateTimeDisplay();
        RefreshTexts();
    }

    private void StartLongBreak()
    {
        _currentState = PomodoroState.LongBreak;
        _remainingTime = _longBreakDuration;
        UpdateTimeDisplay();
        RefreshTexts();
    }

    private void Pause()
    {
        StopTimer();
        IsRunning = false;
        RefreshTexts();
    }

    private void Resume()
    {
        StartTimer();
        IsRunning = true;
        RefreshTexts();
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
        RefreshTexts();
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
        RefreshTexts();

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

