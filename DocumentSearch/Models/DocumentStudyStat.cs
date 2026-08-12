using CommunityToolkit.Mvvm.ComponentModel;

namespace DocumentSearch.Models;

public partial class DocumentStudyStat : ObservableObject
{
    [ObservableProperty]
    private string filePath = string.Empty;

    [ObservableProperty]
    private string fileName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StudyTimeDisplay))]
    private int totalSeconds;

    public string StudyTimeDisplay
    {
        get
        {
            int totalMins = TotalSeconds / 60;
            if (totalMins == 0)
                return $"{TotalSeconds} sn";
            if (totalMins > 60)
                return $"{totalMins / 60} sa {totalMins % 60} dk";
            return $"{totalMins} dk";
        }
        set { }
    }
}
