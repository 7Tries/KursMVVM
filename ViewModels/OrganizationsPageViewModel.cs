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

public partial class OrganizationsPageViewModel : ViewModelBase, IRecipient<DataChangedMessage>
{
    private readonly OrganizationService _service;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Organization> _organizations = new();

    [ObservableProperty]
    private Organization? _selectedOrganization;

    public OrganizationsPageViewModel()
    {
        _service = new OrganizationService();
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
        Organizations.Clear();
        foreach (var item in list) Organizations.Add(item);
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var dialog = new OrganizationWindow { DataContext = new Organization() };
        var result = await _dialogService.ShowDialogAsync<Organization>(dialog);
        if (result != null)
        {
            await _service.AddAsync(result);
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedOrganization == null) return;
        var dialog = new OrganizationWindow { DataContext = SelectedOrganization };
        var result = await _dialogService.ShowDialogAsync<Organization>(dialog);
        if (result != null)
        {
            await _service.UpdateAsync(result);
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedOrganization == null) return;
        var box = MessageBoxManager.GetMessageBoxStandard("Подтверждение", "Удалить организацию?", ButtonEnum.YesNo);
        var result = await box.ShowAsync();
        if (result == ButtonResult.Yes)
        {
            await _service.DeleteAsync(SelectedOrganization.Id);
            await LoadAsync();
        }
    }
}
