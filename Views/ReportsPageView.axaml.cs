using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KursMVVM.Views;

public partial class ReportsPageView : UserControl
{
    public ReportsPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
