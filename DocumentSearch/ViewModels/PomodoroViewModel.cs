using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentSearch.Models;
using DocumentSearch.Services;
using System.Collections.ObjectModel;

namespace DocumentSearch.ViewModels;

public partial class PomodoroViewModel : ObservableObject
{
    private readonly LanguageService _languageService;
    public PomodoroService Service { get; }

    [ObservableProperty]
    private string statusText = string.Empty;

    [ObservableProperty]
    private string buttonText = string.Empty;

    [ObservableProperty]
    private string currentPhase = string.Empty;

    [ObservableProperty]
    private string infoText = string.Empty;

    public ObservableCollection<DocumentStudyStat> DocumentStats => Service.DocumentStats;
    public string TimeDisplay => Service.TimeDisplay;
    public int CompletedCount => Service.CompletedCount;

    public PomodoroViewModel(PomodoroService pomodoroService, LanguageService languageService)
    {
        Service = pomodoroService;
        _languageService = languageService;

        _languageService.LanguageChanged += (s, lang) => RefreshTexts();
        Service.StateChanged += (s, e) => 
        {
            RefreshTexts();
            OnPropertyChanged(nameof(TimeDisplay));
            OnPropertyChanged(nameof(CompletedCount));
        };
        Service.TimerTick += (s, e) => OnPropertyChanged(nameof(TimeDisplay));

        RefreshTexts();
    }

    private void RefreshTexts()
    {
        InfoText = _languageService.GetString("Pomo_InfoText");

        switch (Service.CurrentState)
        {
            case PomodoroState.Idle:
                CurrentPhase = _languageService.GetString("Pomo_Phase_Work");
                StatusText = _languageService.GetString("Pomo_Status_Idle");
                ButtonText = _languageService.GetString("Pomo_Start");
                break;
            case PomodoroState.Working:
                CurrentPhase = _languageService.GetString("Pomo_Phase_Work");
                ButtonText = Service.IsRunning ? _languageService.GetString("Pomo_Pause") : _languageService.GetString("Pomo_Resume");
                StatusText = Service.IsRunning ? _languageService.GetString("Pomo_Status_Work") : _languageService.GetString("Pomo_Status_Paused");
                break;
            case PomodoroState.ShortBreak:
                CurrentPhase = _languageService.GetString("Pomo_Phase_ShortBreak");
                ButtonText = Service.IsRunning ? _languageService.GetString("Pomo_Pause") : _languageService.GetString("Pomo_Resume");
                StatusText = Service.IsRunning ? _languageService.GetString("Pomo_Status_ShortBreak") : _languageService.GetString("Pomo_Status_Paused");
                break;
            case PomodoroState.LongBreak:
                CurrentPhase = _languageService.GetString("Pomo_Phase_LongBreak");
                ButtonText = Service.IsRunning ? _languageService.GetString("Pomo_Pause") : _languageService.GetString("Pomo_Resume");
                StatusText = Service.IsRunning ? _languageService.GetString("Pomo_Status_LongBreak") : _languageService.GetString("Pomo_Status_Paused");
                break;
        }
    }

    [RelayCommand]
    private void StartPause()
    {
        Service.StartPause();
        RefreshTexts();
    }

    [RelayCommand]
    private void Reset()
    {
        Service.Reset();
        RefreshTexts();
    }

    [ObservableProperty]
    private bool isHelpModalOpen;

    [RelayCommand]
    private void ToggleHelpModal()
    {
        IsHelpModalOpen = !IsHelpModalOpen;
    }

    [RelayCommand]
    private void CloseHelpModal()
    {
        IsHelpModalOpen = false;
    }

    [RelayCommand]
    private void Skip()
    {
        Service.Skip();
        RefreshTexts();
    }
}
