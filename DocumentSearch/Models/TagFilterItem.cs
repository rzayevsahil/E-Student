using CommunityToolkit.Mvvm.ComponentModel;

namespace DocumentSearch.Models;

public partial class TagFilterItem : ObservableObject
{
    [ObservableProperty]
    private string tagName = string.Empty;

    [ObservableProperty]
    private int count;

    [ObservableProperty]
    private bool isSelected;

    public string DisplayText => $"#{TagName} ({Count})";

    public string FilterParameter => $"Tag:{TagName}";
}

public class DocumentTagParam
{
    public Document? Document { get; set; }
    public string Tag { get; set; } = string.Empty;
}
