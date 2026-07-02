using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KursMVVM.Views;

public partial class GraphsPageView : UserControl
{
    public GraphsPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
