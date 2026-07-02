using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using KursMVVM.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace KursMVVM.Views;

public partial class EmployeeWindow : Window
{
    public EmployeeWindow()
    {
        InitializeComponent();
    }

    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is Employee emp)
        {
            if (emp.DepartmentCode <= 0)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Код отдела должен быть больше 0", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(emp.FullName))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Ф.И.О. не может быть пустым", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(emp.Position))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Должность не может быть пустой", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (emp.Salary <= 0)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Оклад должен быть больше 0", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (emp.Bonus < 0)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Премия не может быть отрицательной", ButtonEnum.Ok).ShowAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(emp.Month))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Месяц не может быть пустым", ButtonEnum.Ok).ShowAsync();
                return;
            }
            Close(emp);
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
