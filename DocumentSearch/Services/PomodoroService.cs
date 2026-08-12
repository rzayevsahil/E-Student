using CommunityToolkit.Mvvm.ComponentModel;
using DocumentSearch.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace DocumentSearch.Services;

public enum PomodoroState
{
    Idle,
    Working,
    ShortBreak,
    LongBreak
}

public partial class PomodoroService : ObservableObject
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
    private bool isRunning = false;

    [ObservableProperty]
    private int completedCount = 0;

    [ObservableProperty]
    private Document? activeDocument;

    [ObservableProperty]
    private string activeDocumentName = "Belge Seçilmedi";

    [ObservableProperty]
    private ObservableCollection<DocumentStudyStat> documentStats = new();

    public event EventHandler? TimerTick;
    public event EventHandler? StateChanged;

    public PomodoroState CurrentState => _currentState;

    public PomodoroService()
    {
        _remainingTime = _workDuration;
        UpdateTimeDisplay();
    }

    public void SetActiveDocument(Document? doc)
    {
        ActiveDocument = doc;
        ActiveDocumentName = doc != null ? doc.FileName : "Belge Seçilmedi";
    }

    public void StartPause()
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

    public void Reset()
    {
        StopTimer();
        _currentState = PomodoroState.Idle;
        _remainingTime = _workDuration;
        IsRunning = false;
        UpdateTimeDisplay();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Skip()
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

    public void StartWorkForDocument(Document doc)
    {
        SetActiveDocument(doc);
        if (_currentState != PomodoroState.Working || !IsRunning)
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
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void StartLongBreak()
    {
        _currentState = PomodoroState.LongBreak;
        _remainingTime = _longBreakDuration;
        UpdateTimeDisplay();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Pause()
    {
        StopTimer();
        IsRunning = false;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Resume()
    {
        StartTimer();
        IsRunning = true;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void StartTimer()
    {
        if (_timer == null)
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
        }
        
        _timer.Start();
        IsRunning = true;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void StopTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_remainingTime.TotalSeconds > 0)
        {
            _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
            UpdateTimeDisplay();

            if (_currentState == PomodoroState.Working && ActiveDocument != null)
            {
                RecordStudyTime(ActiveDocument);
            }

            TimerTick?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            OnTimerComplete();
        }
    }

    private void RecordStudyTime(Document doc)
    {
        var stat = DocumentStats.FirstOrDefault(s => s.FilePath.Equals(doc.FilePath, StringComparison.OrdinalIgnoreCase));
        if (stat == null)
        {
            stat = new DocumentStudyStat
            {
                FilePath = doc.FilePath,
                FileName = doc.FileName,
                TotalSeconds = 0
            };
            DocumentStats.Add(stat);
        }

        stat.TotalSeconds += 1;
        doc.StudyMinutes = stat.TotalSeconds / 60;
    }

    private void OnTimerComplete()
    {
        StopTimer();
        IsRunning = false;
        StateChanged?.Invoke(this, EventArgs.Empty);

        if (_currentState == PomodoroState.Working)
        {
            CompleteWork();
            string title = GetLocalizedString("Pomo_Notif_WorkComplete_Title", "🍅 Pomodoro Tamamlandı!");
            string body = GetLocalizedString("Pomo_Notif_WorkComplete_Body", "Tebrikler! 25 dakikalık çalışma seansı bitti. Mola zamanı! ☕");
            NotificationService.ShowNotification(title, body);
        }
        else if (_currentState == PomodoroState.ShortBreak || _currentState == PomodoroState.LongBreak)
        {
            StartWork();
            string title = GetLocalizedString("Pomo_Notif_BreakComplete_Title", "☕ Mola Bitti!");
            string body = GetLocalizedString("Pomo_Notif_BreakComplete_Body", "Mola süreniz doldu. Yeni çalışma seansına başlayabilirsiniz! 🚀");
            NotificationService.ShowNotification(title, body);
        }
    }

    private static string GetLocalizedString(string resourceKey, string fallback)
    {
        try
        {
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Resources.Contains(resourceKey))
            {
                return System.Windows.Application.Current.Resources[resourceKey] as string ?? fallback;
            }
        }
        catch { }
        return fallback;
    }

    private void UpdateTimeDisplay()
    {
        TimeDisplay = $"{_remainingTime.Minutes:D2}:{_remainingTime.Seconds:D2}";
    }
}
