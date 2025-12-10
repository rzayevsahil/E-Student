using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentSearch.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace DocumentSearch.ViewModels;

public partial class NavigationViewModel : ObservableObject
{
    private readonly IServiceProvider? _serviceProvider;

    [ObservableProperty]
    private UserControl? currentView;

    [ObservableProperty]
    private string selectedMenuItem = "DocumentSearch";

    public NavigationViewModel(IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
        NavigateToDocumentSearch();
    }

    [RelayCommand]
    private void NavigateToDocumentSearch()
    {
        SelectedMenuItem = "DocumentSearch";
        var view = new DocumentSearchView();
        if (_serviceProvider != null)
        {
            var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            view.DataContext = viewModel;
        }
        CurrentView = view;
    }

    [RelayCommand]
    private void NavigateToPomodoro()
    {
        SelectedMenuItem = "Pomodoro";
        var view = new PomodoroView();
        if (_serviceProvider != null)
        {
            var viewModel = _serviceProvider.GetRequiredService<PomodoroViewModel>();
            view.DataContext = viewModel;
        }
        CurrentView = view;
    }
}

