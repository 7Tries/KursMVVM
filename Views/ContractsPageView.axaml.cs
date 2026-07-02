using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KursMVVM.Views;

public partial class ContractsPageView : UserControl
{
    public ContractsPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
