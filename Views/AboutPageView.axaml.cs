using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KursMVVM.Views;

public partial class AboutPageView : UserControl
{
    public AboutPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
