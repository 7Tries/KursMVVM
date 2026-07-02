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

public partial class ContractsPageViewModel : ViewModelBase, IRecipient<DataChangedMessage>
{
    private readonly ContractService _service;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Contract> _contracts = new();

    [ObservableProperty]
    private Contract? _selectedContract;

    public ContractsPageViewModel()
    {
        _service = new ContractService();
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
        Contracts.Clear();
        foreach (var item in list) Contracts.Add(item);
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var dialog = new ContractWindow { DataContext = new Contract() };
        var result = await _dialogService.ShowDialogAsync<Contract>(dialog);
        if (result != null)
        {
            await _service.AddAsync(result);
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedContract == null) return;
        var dialog = new ContractWindow { DataContext = SelectedContract };
        var result = await _dialogService.ShowDialogAsync<Contract>(dialog);
        if (result != null)
        {
            await _service.UpdateAsync(result);
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedContract == null) return;
        var box = MessageBoxManager.GetMessageBoxStandard("Подтверждение", "Удалить договор?", ButtonEnum.YesNo);
        var result = await box.ShowAsync();
        if (result == ButtonResult.Yes)
        {
            await _service.DeleteAsync(SelectedContract.ContractNumber);
            await LoadAsync();
        }
    }
}
