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

public partial class SuppliesPageViewModel : ViewModelBase, IRecipient<DataChangedMessage>
{
    private readonly SupplyService _service;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Supply> _supplies = new();

    [ObservableProperty]
    private Supply? _selectedSupply;

    public SuppliesPageViewModel()
    {
        _service = new SupplyService();
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
        Supplies.Clear();
        foreach (var item in list) Supplies.Add(item);
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var dialog = new SupplyWindow { DataContext = new Supply() };
        var result = await _dialogService.ShowDialogAsync<Supply>(dialog);
        if (result != null)
        {
            await _service.AddAsync(result);
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedSupply == null) return;
        var dialog = new SupplyWindow { DataContext = SelectedSupply };
        var result = await _dialogService.ShowDialogAsync<Supply>(dialog);
        if (result != null)
        {
            await _service.UpdateAsync(result);
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedSupply == null) return;
        var box = MessageBoxManager.GetMessageBoxStandard("Подтверждение", "Удалить поставку?", ButtonEnum.YesNo);
        var result = await box.ShowAsync();
        if (result == ButtonResult.Yes)
        {
            await _service.DeleteAsync(SelectedSupply.ContractNumber);
            await LoadAsync();
        }
    }
}
