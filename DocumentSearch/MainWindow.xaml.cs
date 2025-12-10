using DocumentSearch.ViewModels;
using System.Windows;

namespace DocumentSearch;

public partial class MainWindow : Window
{
    public MainWindow(NavigationViewModel navigationViewModel)
    {
        InitializeComponent();
        DataContext = navigationViewModel;
    }
}
