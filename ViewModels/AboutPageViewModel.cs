using CommunityToolkit.Mvvm.ComponentModel;

namespace KursMVVM.ViewModels;

public partial class AboutPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _appName = "Система учёта поставок ПО";

    [ObservableProperty]
    private string _version = "1.0.0";

    [ObservableProperty]
    private string _description = "Курсовая работа по дисциплине \"Программирование\". Приложение для автоматизации учёта сотрудников, договоров, поставок и организаций.";
}
