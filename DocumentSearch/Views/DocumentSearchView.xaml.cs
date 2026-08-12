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

    private void DocumentsBorder_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private async void DocumentsBorder_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0 && DataContext is MainViewModel viewModel)
            {
                await viewModel.LoadFilesFromPathsAsync(files);
            }
        }
    }

    private void StartFocusSession_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is Document document)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.StartFocusSessionCommand.Execute(document);
            }
        }
        e.Handled = true;
    }

    private void OpenDocumentItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is Document document)
        {
            OpenDocument(document);
        }
        e.Handled = true;
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

    private void AddTagAndClose_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is Document document)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.AddTagCommand.Execute(document);
            }

            var popup = FindParent<System.Windows.Controls.Primitives.Popup>(element);
            if (popup != null)
            {
                popup.IsOpen = false;
            }
        }
    }

    private void TagTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            AddTagAndClose_Click(sender, e);
            e.Handled = true;
        }
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject? parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);
        if (parentObject == null)
        {
            parentObject = LogicalTreeHelper.GetParent(child);
        }
        if (parentObject == null) return null;
        if (parentObject is T parent) return parent;
        return FindParent<T>(parentObject);
    }

    private void DocumentListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is Document document)
        {
            OpenDocument(document);
        }
    }

    private void OpenExternalFromPreview_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel && !string.IsNullOrEmpty(viewModel.PreviewDocumentPath))
        {
            var document = new Document
            {
                FilePath = viewModel.PreviewDocumentPath,
                FileName = viewModel.PreviewDocumentName,
                FileExtension = viewModel.PreviewFileExtension
            };
            OpenDocument(document, viewModel.PreviewCurrentPage > 0 ? viewModel.PreviewCurrentPage : null);
        }
    }

    private void SearchResultsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        OpenSelectedSearchResult(sender);
    }

    private void OpenSelectedSearchResult(object sender)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem is SearchResult result)
        {
            if (!string.IsNullOrEmpty(result.DocumentPath))
            {
                var fileExtension = System.IO.Path.GetExtension(result.DocumentPath).ToLower();
                var document = new Document
                {
                    FilePath = result.DocumentPath,
                    FileName = result.DocumentName,
                    FileExtension = fileExtension
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
            var fileExtension = !string.IsNullOrEmpty(document.FileExtension) 
                ? document.FileExtension.ToLower() 
                : System.IO.Path.GetExtension(document.FilePath).ToLower();

            // PDF dosyaları için sayfa numarası ile açma
            if (pageNumber.HasValue && pageNumber.Value > 0 && fileExtension == ".pdf")
            {
                OpenPdfAtPage(document.FilePath, pageNumber.Value);
                return;
            }
            // Word dosyaları için sayfa numarası ile açma
            else if (pageNumber.HasValue && pageNumber.Value > 0 && 
                     (fileExtension == ".docx" || fileExtension == ".doc"))
            {
                // Word için COM Interop kullanarak sayfa numarası ile açma
                OpenWordDocumentAtPage(document.FilePath, pageNumber.Value);
                return;
            }
            // PowerPoint dosyaları için slayt numarası ile açma
            else if (pageNumber.HasValue && pageNumber.Value > 0 && 
                     (fileExtension == ".pptx" || fileExtension == ".ppt"))
            {
                OpenPowerPointAtSlide(document.FilePath, pageNumber.Value);
                return;
            }
            else
            {
                // Sayfa numarası yoksa veya desteklenmeyen format ise normal aç
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = document.FilePath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Dosya açılırken hata oluştu: {ex.Message}", 
                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenPdfAtPage(string filePath, int pageNumber)
    {
        try
        {
            // Windows'ta varsayılan PDF okuyucuyu bul
            string? defaultPdfApp = GetDefaultPdfApplication();
            
            if (!string.IsNullOrEmpty(defaultPdfApp))
            {
                // Adobe Reader tespit edildiyse sayfa numarası parametresi ekle
                // /A parametresi mevcut instance'ı kullanır ve sayfayı değiştirir
                // /N parametresi yeni pencere açılmasını engeller
                if (defaultPdfApp.Contains("Acrobat", StringComparison.OrdinalIgnoreCase) ||
                    defaultPdfApp.Contains("AcroRd32", StringComparison.OrdinalIgnoreCase))
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = defaultPdfApp,
                        Arguments = $"/N /A \"page={pageNumber}\" \"{filePath}\"",
                        UseShellExecute = false
                    };
                    Process.Start(processStartInfo);
                    return;
                }
                // Sumatra PDF tespit edildiyse
                else if (defaultPdfApp.Contains("SumatraPDF", StringComparison.OrdinalIgnoreCase))
                {
                    // Sumatra PDF mevcut instance'ı kullanır
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = defaultPdfApp,
                        Arguments = $"\"{filePath}\" -page {pageNumber}",
                        UseShellExecute = false
                    };
                    Process.Start(processStartInfo);
                    return;
                }
                // Foxit Reader tespit edildiyse
                else if (defaultPdfApp.Contains("Foxit", StringComparison.OrdinalIgnoreCase))
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = defaultPdfApp,
                        Arguments = $"\"{filePath}\" /A page={pageNumber}",
                        UseShellExecute = false
                    };
                    Process.Start(processStartInfo);
                    return;
                }
            }

            // Varsayılan PDF okuyucu bulunamadıysa veya desteklenmiyorsa
            // Önce Adobe Reader'ı manuel kontrol et
            var adobePaths = new[]
            {
                @"C:\Program Files\Adobe\Acrobat DC\Acrobat\Acrobat.exe",
                @"C:\Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe",
                @"C:\Program Files\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe"
            };

            foreach (var adobePath in adobePaths)
            {
                if (System.IO.File.Exists(adobePath))
                {
                    // /N parametresi: Yeni pencere açma, mevcut instance'ı kullan
                    // /A parametresi: Mevcut instance'ı kullan ve sayfayı değiştir
                    // page=N: Belirtilen sayfaya git
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = adobePath,
                        Arguments = $"/N /A \"page={pageNumber}\" \"{filePath}\"",
                        UseShellExecute = false
                    };
                    Process.Start(processStartInfo);
                    return;
                }
            }

            // Hiçbiri bulunamazsa varsayılan uygulamayı kullan (sayfa numarası olmadan)
            // Windows varsayılan uygulaması ile aç
            var defaultProcessStartInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };
            Process.Start(defaultProcessStartInfo);
        }
        catch (Exception ex)
        {
            // Hata olursa normal açmayı dene
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex2)
            {
                MessageBox.Show($"PDF dosyası açılırken hata oluştu: {ex2.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private string? GetDefaultPdfApplication()
    {
        try
        {
            // Windows Registry'den varsayılan PDF okuyucuyu bul
            using (var pdfKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@".pdf"))
            {
                if (pdfKey != null)
                {
                    var defaultApp = pdfKey.GetValue("")?.ToString();
                    if (!string.IsNullOrEmpty(defaultApp))
                    {
                        // Default uygulamanın executable yolunu bul
                        using (var appKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($@"{defaultApp}\shell\open\command"))
                        {
                            if (appKey != null)
                            {
                                var command = appKey.GetValue("")?.ToString();
                                if (!string.IsNullOrEmpty(command))
                                {
                                    // Command string'den executable yolunu çıkar
                                    // Örnek: "C:\Program Files\Adobe\Acrobat DC\Acrobat\Acrobat.exe" "%1"
                                    var exePath = command.Trim('"');
                                    if (exePath.Contains(".exe"))
                                    {
                                        var exeIndex = exePath.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
                                        if (exeIndex > 0)
                                        {
                                            exePath = exePath.Substring(0, exeIndex + 4).Trim('"');
                                            if (System.IO.File.Exists(exePath))
                                            {
                                                return exePath;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Registry okuma hatası - sessizce devam et
        }

        return null;
    }

    private void OpenWordDocumentAtPage(string filePath, int pageNumber)
    {
        try
        {
            // Word COM Interop kullanarak sayfa numarası ile açma
            Type wordType = Type.GetTypeFromProgID("Word.Application");
            if (wordType == null)
            {
                // Word yüklü değilse normal aç
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
                return;
            }

            dynamic wordApp = Activator.CreateInstance(wordType);
            wordApp.Visible = true;
            
            // Dosyayı aç
            dynamic document = wordApp.Documents.Open(filePath);
            
            // Sayfa numarasına git
            // Word'de sayfa numarası 1'den başlar
            try
            {
                wordApp.Selection.GoTo(
                    What: 1, // wdGoToPage
                    Which: 1, // wdGoToAbsolute
                    Count: pageNumber
                );
            }
            catch
            {
                // Sayfa numarasına gidilemezse en azından dosyayı aç
            }
        }
        catch
        {
            // COM Interop başarısız olursa normal aç
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Word dosyası açılırken hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void OpenPowerPointAtSlide(string filePath, int slideNumber)
    {
        try
        {
            Type? pptType = Type.GetTypeFromProgID("PowerPoint.Application");
            if (pptType == null)
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
                return;
            }

            dynamic pptApp = Activator.CreateInstance(pptType);
            pptApp.Visible = true;
            
            dynamic presentation = pptApp.Presentations.Open(filePath);
            try
            {
                if (pptApp.ActiveWindow != null)
                {
                    pptApp.ActiveWindow.View.GotoSlide(slideNumber);
                }
            }
            catch
            {
                // En azından sunumu açtı
            }
        }
        catch
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PowerPoint dosyası açılırken hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

