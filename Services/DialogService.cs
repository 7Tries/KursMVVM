using Avalonia.Controls;
using System.Threading.Tasks;

namespace KursMVVM.Services;

public interface IDialogService
{
    Task<T?> ShowDialogAsync<T>(Window dialog) where T : class;
    Window? GetMainWindow();
}

public class DialogService : IDialogService
{
    public Task<T?> ShowDialogAsync<T>(Window dialog) where T : class
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return Task.FromResult<T?>(null);
        return dialog.ShowDialog<T>(mainWindow);
    }

    public Window? GetMainWindow()
    {
        return App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }
}
