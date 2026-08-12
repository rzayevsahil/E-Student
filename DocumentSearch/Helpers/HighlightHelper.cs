using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DocumentSearch.Helpers;

/// <summary>
/// TextBlock içerisinde aranan kelimeyi sarı arka plan (Highlight) ile vurgulamaya yarayan Attached Property yardımcısı.
/// </summary>
public static class HighlightHelper
{
    public static readonly DependencyProperty HighlightTextProperty =
        DependencyProperty.RegisterAttached(
            "HighlightText",
            typeof(string),
            typeof(HighlightHelper),
            new PropertyMetadata(null, OnHighlightChanged));

    public static readonly DependencyProperty SearchQueryProperty =
        DependencyProperty.RegisterAttached(
            "SearchQuery",
            typeof(string),
            typeof(HighlightHelper),
            new PropertyMetadata(null, OnHighlightChanged));

    public static string GetHighlightText(DependencyObject obj) => (string)obj.GetValue(HighlightTextProperty);
    public static void SetHighlightText(DependencyObject obj, string value) => obj.SetValue(HighlightTextProperty, value);

    public static string GetSearchQuery(DependencyObject obj) => (string)obj.GetValue(SearchQueryProperty);
    public static void SetSearchQuery(DependencyObject obj, string value) => obj.SetValue(SearchQueryProperty, value);

    private static void OnHighlightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBlock textBlock)
        {
            UpdateHighlighting(textBlock);
        }
    }

    private static void UpdateHighlighting(TextBlock textBlock)
    {
        string text = GetHighlightText(textBlock) ?? string.Empty;
        string query = GetSearchQuery(textBlock) ?? string.Empty;

        textBlock.Inlines.Clear();

        if (string.IsNullOrWhiteSpace(text))
            return;

        string trimmedQuery = query.Trim();
        if (string.IsNullOrWhiteSpace(trimmedQuery))
        {
            textBlock.Inlines.Add(new Run(text));
            return;
        }

        int index = 0;
        string textLower = text.ToLowerInvariant();
        string queryLower = trimmedQuery.ToLowerInvariant();

        while (index < text.Length)
        {
            int foundIndex = textLower.IndexOf(queryLower, index, StringComparison.OrdinalIgnoreCase);
            if (foundIndex < 0)
            {
                textBlock.Inlines.Add(new Run(text.Substring(index)));
                break;
            }

            if (foundIndex > index)
            {
                textBlock.Inlines.Add(new Run(text.Substring(index, foundIndex - index)));
            }

            string match = text.Substring(foundIndex, trimmedQuery.Length);
            var highlightedRun = new Run(match)
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FACC15")), // Warm Vibrant Yellow Background
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A")), // Dark Slate Text for Contrast
                FontWeight = FontWeights.Bold
            };
            textBlock.Inlines.Add(highlightedRun);

            index = foundIndex + trimmedQuery.Length;
        }
    }
}
