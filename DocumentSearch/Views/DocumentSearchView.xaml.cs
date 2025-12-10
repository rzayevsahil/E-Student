using DocumentSearch.Models;
using DocumentSearch.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DocumentSearch.Views;

public partial class DocumentSearchView : UserControl
{
    public DocumentSearchView()
    {
        InitializeComponent();
    }

    private void RemoveDocument_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Document document)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.RemoveDocumentCommand.Execute(document);
            }
        }
        e.Handled = true;
    }

    private void DocumentListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is Document document)
        {
            OpenDocument(document);
        }
    }

    private void SearchResultsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem is SearchResult result)
        {
            if (!string.IsNullOrEmpty(result.DocumentPath))
            {
                var document = new Document
                {
                    FilePath = result.DocumentPath,
                    FileName = result.DocumentName
                };
                OpenDocument(document, result.PageNumber > 0 ? result.PageNumber : null);
            }
        }
    }

    private void OpenDocument(Document document, int? pageNumber = null)
    {
        if (string.IsNullOrEmpty(document.FilePath) || !System.IO.File.Exists(document.FilePath))
        {
            MessageBox.Show("Dosya bulunamadı. Dosya taşınmış veya silinmiş olabilir.", 
                "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = document.FilePath,
                UseShellExecute = true
            };

            if (pageNumber.HasValue && pageNumber.Value > 0 && 
                document.FileExtension.ToLower() == ".pdf")
            {
                processStartInfo.Arguments = $"\"{document.FilePath}#page={pageNumber.Value}\"";
            }

            Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Dosya açılırken hata oluştu: {ex.Message}", 
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

