using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DocumentSearch.Models;

public partial class Document : ObservableObject
{
    [ObservableProperty]
    private string filePath = string.Empty;

    [ObservableProperty]
    private string fileName = string.Empty;

    [ObservableProperty]
    private string fileExtension = string.Empty;

    [ObservableProperty]
    private long fileSize;

    [ObservableProperty]
    private DateTime uploadDate = DateTime.Now;

    [ObservableProperty]
    private List<PriceItem> priceItems = new();

    [ObservableProperty]
    private string rawContent = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavoriteIcon))]
    [NotifyPropertyChangedFor(nameof(FavoriteBrush))]
    private bool isFavorite;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTags))]
    [NotifyPropertyChangedFor(nameof(HasTags))]
    private ObservableCollection<string> tags = new();

    [ObservableProperty]
    private string tagInputText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StudyTimeDisplay))]
    [NotifyPropertyChangedFor(nameof(HasStudyTime))]
    private int studyMinutes;

    public string StudyTimeDisplay
    {
        get => StudyMinutes > 60
            ? $"{StudyMinutes / 60} sa {StudyMinutes % 60} dk"
            : $"{StudyMinutes} dk";
        set { }
    }

    public bool HasStudyTime => StudyMinutes > 0;

    public string DisplayTags => Tags != null && Tags.Any() ? string.Join(", ", Tags) : string.Empty;

    public bool HasTags => Tags != null && Tags.Any();

    public string FavoriteIcon => IsFavorite ? "★" : "☆";

    public string FavoriteBrush => IsFavorite ? "#F59E0B" : "#9CA3AF";
}
