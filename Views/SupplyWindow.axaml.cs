using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using KursMVVM.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace KursMVVM.Views;

public partial class SupplyWindow : Window
{
    public SupplyWindow()
    {
        InitializeComponent();
    }

    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is Supply sup)
        {
            if (string.IsNullOrWhiteSpace(sup.ContractNumber))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Номер договора не может быть пустым", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(sup.EquipmentType))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Тип оборудования не может быть пустым", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (sup.EmployeeId <= 0)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Код сотрудника должен быть больше 0", ButtonEnum.Ok).ShowAsync();
                return;
            }
            Close(sup);
        }
        else
        {
            Close(null);
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
