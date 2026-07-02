using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KursMVVM.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace KursMVVM.ViewModels;

public partial class ImportExportPageViewModel : ViewModelBase
{
    private readonly ImportExportService _service;

    [ObservableProperty]
    private ObservableCollection<string> _logMessages = new();

    [ObservableProperty]
    private string _statusText = "Готов к работе";

    public ImportExportPageViewModel()
    {
        _service = new ImportExportService();
    }

    [RelayCommand]
    private async Task ImportCsvAsync()
    {
        var files = await PickFilesAsync("CSV", "*.csv");
        if (files == null || files.Count == 0) return;
        foreach (var file in files)
        {
            var path = file.Path.LocalPath;
            var name = Path.GetFileName(path).ToLower();
            List<ImportError> errors;
            if (name.Contains("сотрудник"))
                errors = await _service.ImportEmployeesFromCsvAsync(path);
            else if (name.Contains("договор"))
                errors = await _service.ImportContractsFromCsvAsync(path);
            else if (name.Contains("поставк"))
                errors = await _service.ImportSuppliesFromCsvAsync(path);
            else if (name.Contains("организац"))
                errors = await _service.ImportOrganizationsFromCsvAsync(path);
            else
            {
                Log($"Неизвестный файл: {name}");
                continue;
            }
            Log($"Импорт {name}: {(errors.Count == 0 ? "OK" : $"{errors.Count} ошибок")}");
            foreach (var e in errors) Log(e.ToString());
        }
    }

    [RelayCommand]
    private async Task ImportJsonAsync()
    {
        var files = await PickFilesAsync("JSON", "*.json");
        if (files == null || files.Count == 0) return;
        foreach (var file in files)
        {
            var errors = await _service.ImportFromJsonAsync(file.Path.LocalPath);
            Log($"Импорт JSON: {(errors.Count == 0 ? "OK" : $"{errors.Count} ошибок")}");
        }
    }

    [RelayCommand]
    private async Task ImportXmlAsync()
    {
        var files = await PickFilesAsync("XML", "*.xml");
        if (files == null || files.Count == 0) return;
        foreach (var file in files)
        {
            var errors = await _service.ImportFromXmlAsync(file.Path.LocalPath);
            Log($"Импорт XML: {(errors.Count == 0 ? "OK" : $"{errors.Count} ошибок")}");
        }
    }

    [RelayCommand]
    private async Task ImportExcelAsync()
    {
        var files = await PickFilesAsync("Excel", "*.xlsx", "*.xls");
        if (files == null || files.Count == 0) return;
        foreach (var file in files)
        {
            var errors = await _service.ImportFromExcelAsync(file.Path.LocalPath);
            Log($"Импорт Excel: {(errors.Count == 0 ? "OK" : $"{errors.Count} ошибок")}");
            foreach (var e in errors) Log(e.ToString());
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        var file = await SaveFileAsync("CSV", "*.csv");
        if (file == null) return;
        await _service.ExportEmployeesToCsvAsync(file.Path.LocalPath);
        Log($"Экспорт CSV: {file.Name}");
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        var file = await SaveFileAsync("Excel", "*.xlsx");
        if (file == null) return;
        await _service.ExportEmployeesToExcelAsync(file.Path.LocalPath);
        Log($"Экспорт Excel: {file.Name}");
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        var file = await SaveFileAsync("PDF", "*.pdf");
        if (file == null) return;
        await _service.ExportEmployeesToPdfAsync(file.Path.LocalPath);
        Log($"Экспорт PDF: {file.Name}");
    }

    [RelayCommand]
    private void ClearLog()
    {
        LogMessages.Clear();
    }

    private void Log(string msg)
    {
        LogMessages.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
        StatusText = msg;
    }

    private async Task<IReadOnlyList<IStorageFile>?> PickFilesAsync(string type, params string[] patterns)
    {
        var top = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime d ? d.MainWindow : null);
        if (top == null) return null;
        return await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = $"Выберите {type}",
            AllowMultiple = true,
            FileTypeFilter = new[] { new FilePickerFileType(type) { Patterns = patterns } }
        });
    }

    private async Task<IStorageFile?> SaveFileAsync(string type, string pattern)
    {
        var top = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime d ? d.MainWindow : null);
        if (top == null) return null;
        return await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = $"Сохранить {type}",
            FileTypeChoices = new[] { new FilePickerFileType(type) { Patterns = new[] { pattern } } }
        });
    }
}
