using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KursMVVM.Services;

namespace KursMVVM.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private object? _currentPage;

    [ObservableProperty]
    private string _title = "Система учёта поставок ПО";

    public MainWindowViewModel()
    {
        _navigationService = new NavigationService(this);
        _navigationService.RegisterPage("Employees", new EmployeesPageViewModel());
        _navigationService.RegisterPage("Contracts", new ContractsPageViewModel());
        _navigationService.RegisterPage("Supplies", new SuppliesPageViewModel());
        _navigationService.RegisterPage("Organizations", new OrganizationsPageViewModel());
        _navigationService.RegisterPage("Reports", new ReportsPageViewModel());
        _navigationService.RegisterPage("Graphs", new GraphsPageViewModel());
        _navigationService.RegisterPage("ImportExport", new ImportExportPageViewModel());
        _navigationService.RegisterPage("About", new AboutPageViewModel());
        _navigationService.NavigateTo("Employees");
    }

    [RelayCommand]
    private void Navigate(string pageKey)
    {
        _navigationService.NavigateTo(pageKey);
    }
}
