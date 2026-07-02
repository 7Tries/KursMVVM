using KursMVVM.ViewModels;
using System.Collections.Generic;

namespace KursMVVM.Services;

public class NavigationService
{
    private readonly MainWindowViewModel _mainViewModel;
    private readonly Dictionary<string, ViewModelBase> _pages = new();

    public NavigationService(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    public void RegisterPage(string key, ViewModelBase viewModel)
    {
        _pages[key] = viewModel;
    }

    public void NavigateTo(string key)
    {
        if (_pages.TryGetValue(key, out var page))
        {
            _mainViewModel.CurrentPage = page;
        }
    }
}
