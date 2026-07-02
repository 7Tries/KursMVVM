using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KursMVVM.Messages;
using KursMVVM.Models;
using KursMVVM.Services;
using KursMVVM.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace KursMVVM.ViewModels;

public partial class EmployeesPageViewModel : ViewModelBase, IRecipient<DataChangedMessage>
{
    private readonly EmployeeService _service;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    [ObservableProperty]
    private Employee? _selectedEmployee;

    public EmployeesPageViewModel()
    {
        _service = new EmployeeService();
        _dialogService = new DialogService();
        _ = LoadAsync();
        Messenger.Register<DataChangedMessage>(this, (r, m) => _ = LoadAsync());
    }

    public void Receive(DataChangedMessage message)
    {
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await _service.GetAllAsync();
        Employees.Clear();
        foreach (var item in list) Employees.Add(item);
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var dialog = new EmployeeWindow { DataContext = new Employee() };
        var result = await _dialogService.ShowDialogAsync<Employee>(dialog);
        if (result != null)
        {
            await _service.AddAsync(result);
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedEmployee == null) return;
        var dialog = new EmployeeWindow { DataContext = SelectedEmployee };
        var result = await _dialogService.ShowDialogAsync<Employee>(dialog);
        if (result != null)
        {
            await _service.UpdateAsync(result);
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedEmployee == null) return;
        var box = MessageBoxManager.GetMessageBoxStandard("Подтверждение", "Удалить сотрудника?", ButtonEnum.YesNo);
        var result = await box.ShowAsync();
        if (result == ButtonResult.Yes)
        {
            await _service.DeleteAsync(SelectedEmployee.Id);
            await LoadAsync();
        }
    }
}
