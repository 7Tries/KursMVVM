using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using KursMVVM.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;

namespace KursMVVM.Views;

public partial class ContractWindow : Window
{
    public ContractWindow()
    {
        InitializeComponent();
    }

    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is Contract con)
        {
            if (string.IsNullOrWhiteSpace(con.ContractNumber))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Номер договора не может быть пустым", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (con.EmployeeId <= 0)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Код сотрудника должен быть больше 0", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(con.OrganizationName))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Организация не может быть пустой", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (con.Date == default || con.Date > DateTime.Now)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Дата заключения не может быть в будущем", ButtonEnum.Ok).ShowAsync();
                return;
            }
            Close(con);
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
