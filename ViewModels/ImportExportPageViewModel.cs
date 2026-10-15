using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KursMVVM.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace KursMVVM.ViewModels;

public partial class ImportExportPageViewModel : ViewModelBase
{
    private readonly ImportExportService _service;

    [ObservableProperty]
    private ObservableCollection<string> _availableTargets = new()
    {
        "Сотрудники",
        "Договоры",
        "Поставки",
        "Организации",
        "Все данные (БД целиком)",
        "Отчёт: Сводка по сотрудникам",
        "Отчёт: Сводка по поставкам",
        "Отчёт: Сводка по договорам",
        "Отчёт: Неактивные сотрудники",
        "Отчёт: География поставок"
    };

    [ObservableProperty]
    private string _selectedTarget = "Сотрудники";

    [ObservableProperty]
    private ObservableCollection<string> _availableFormats = new()
    {
        "Excel (.xlsx)",
        "CSV (.csv)",
        "PDF (.pdf)",
        "JSON (.json)",
        "XML (.xml)"
    };

    [ObservableProperty]
    private string _selectedFormat = "Excel (.xlsx)";

    [ObservableProperty]
    private ObservableCollection<string> _logMessages = new();

    [ObservableProperty]
    private string _statusText = "Готов к работе";

    [ObservableProperty]
    private bool _isBusy = false;

    public ImportExportPageViewModel()
    {
        _service = new ImportExportService();
        Log("Модуль импорта/экспорта инициализирован.");
    }

    // ===================== IMPORT COMMANDS =====================

    [RelayCommand]
    private async Task ImportCsvAsync()
    {
        var files = await PickFilesAsync("CSV", "*.csv");
        if (files == null || files.Count == 0) return;
        IsBusy = true;
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
                Log($"[CSV] Не удалось определить тип сущности по имени файла: {name}");
                continue;
            }
            Log($"[CSV] Импорт '{name}': {(errors.Count == 0 ? "Успешно завершён без ошибок" : $"{errors.Count} ошибок при обработке")}");
            foreach (var e in errors) Log($"  ⚠ {e}");
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task ImportJsonAsync()
    {
        var files = await PickFilesAsync("JSON", "*.json");
        if (files == null || files.Count == 0) return;
        IsBusy = true;
        foreach (var file in files)
        {
            var errors = await _service.ImportFromJsonAsync(file.Path.LocalPath);
            Log($"[JSON] Импорт '{file.Name}': {(errors.Count == 0 ? "Успешно импортировано" : $"{errors.Count} ошибок")}");
            foreach (var e in errors) Log($"  ⚠ {e}");
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task ImportXmlAsync()
    {
        var files = await PickFilesAsync("XML", "*.xml");
        if (files == null || files.Count == 0) return;
        IsBusy = true;
        foreach (var file in files)
        {
            var errors = await _service.ImportFromXmlAsync(file.Path.LocalPath);
            Log($"[XML] Импорт '{file.Name}': {(errors.Count == 0 ? "Успешно импортировано" : $"{errors.Count} ошибок")}");
            foreach (var e in errors) Log($"  ⚠ {e}");
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task ImportExcelAsync()
    {
        var files = await PickFilesAsync("Excel", "*.xlsx", "*.xls");
        if (files == null || files.Count == 0) return;
        IsBusy = true;
        foreach (var file in files)
        {
            var errors = await _service.ImportFromExcelAsync(file.Path.LocalPath);
            Log($"[Excel] Импорт '{file.Name}': {(errors.Count == 0 ? "Все листы успешно загружены" : $"{errors.Count} ошибок")}");
            foreach (var e in errors) Log($"  ⚠ {e}");
        }
        IsBusy = false;
    }

    // ===================== UNIFIED EXPORT COMMAND =====================

    [RelayCommand]
    private async Task ExecuteCustomExportAsync()
    {
        var (typeName, ext) = GetFormatDetails(SelectedFormat);
        var safeTargetName = SelectedTarget.Replace(":", "").Replace(" ", "_");
        var defaultFileName = $"{safeTargetName}_{DateTime.Now:yyyyMMdd_HHmm}.{ext}";

        var file = await SaveFileAsync(typeName, $"*.{ext}", defaultFileName);
        if (file == null) return;

        IsBusy = true;
        try
        {
            int count = await _service.ExportAsync(SelectedTarget, SelectedFormat, file.Path.LocalPath);
            Log($"[Экспорт] '{SelectedTarget}' в формат {SelectedFormat}: сохранено {count} записей в файл '{file.Name}'");
        }
        catch (Exception ex)
        {
            Log($"[Ошибка экспорта] {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Quick export commands
    [RelayCommand]
    private async Task ExportToExcelDirectAsync()
    {
        SelectedFormat = "Excel (.xlsx)";
        await ExecuteCustomExportAsync();
    }

    [RelayCommand]
    private async Task ExportToCsvDirectAsync()
    {
        SelectedFormat = "CSV (.csv)";
        await ExecuteCustomExportAsync();
    }

    [RelayCommand]
    private async Task ExportToPdfDirectAsync()
    {
        SelectedFormat = "PDF (.pdf)";
        await ExecuteCustomExportAsync();
    }

    [RelayCommand]
    private async Task ExportToJsonDirectAsync()
    {
        SelectedFormat = "JSON (.json)";
        await ExecuteCustomExportAsync();
    }

    [RelayCommand]
    private void ClearLog()
    {
        LogMessages.Clear();
        StatusText = "Лог очищен";
    }

    private void Log(string msg)
    {
        LogMessages.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
        StatusText = msg;
    }

    private (string typeName, string extension) GetFormatDetails(string format)
    {
        if (format.Contains("csv", StringComparison.OrdinalIgnoreCase)) return ("CSV файл", "csv");
        if (format.Contains("pdf", StringComparison.OrdinalIgnoreCase)) return ("PDF документ", "pdf");
        if (format.Contains("json", StringComparison.OrdinalIgnoreCase)) return ("JSON файл", "json");
        if (format.Contains("xml", StringComparison.OrdinalIgnoreCase)) return ("XML файл", "xml");
        return ("Книга Excel", "xlsx");
    }

    private async Task<IReadOnlyList<IStorageFile>?> PickFilesAsync(string type, params string[] patterns)
    {
        var top = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime d ? d.MainWindow : null);
        if (top == null) return null;
        return await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = $"Выберите файл(ы) для импорта ({type})",
            AllowMultiple = true,
            FileTypeFilter = new[] { new FilePickerFileType(type) { Patterns = patterns } }
        });
    }

    private async Task<IStorageFile?> SaveFileAsync(string type, string pattern, string? suggestedFileName = null)
    {
        var top = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime d ? d.MainWindow : null);
        if (top == null) return null;
        return await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = $"Экспорт: Сохранить как {type}",
            SuggestedFileName = suggestedFileName,
            FileTypeChoices = new[] { new FilePickerFileType(type) { Patterns = new[] { pattern } } }
        });
    }
}
