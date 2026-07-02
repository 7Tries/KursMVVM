using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using KursMVVM.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace KursMVVM.Views;

public partial class OrganizationWindow : Window
{
    public OrganizationWindow()
    {
        InitializeComponent();
    }

    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is Organization org)
        {
            if (string.IsNullOrWhiteSpace(org.ContractNumber))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Номер договора не может быть пустым", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(org.CountryCode) || org.CountryCode.Length != 2)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Код страны должен состоять из 2 символов", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(org.City))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Город не может быть пустым", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(org.Address))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Адрес не может быть пустым", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(org.Phone))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Телефон не может быть пустым", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(org.Email) || !org.Email.Contains("@"))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Неверный e-mail", ButtonEnum.Ok).ShowAsync();
                return;
            }
            Close(org);
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
