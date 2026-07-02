using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KursMVVM.Views;

public partial class OrganizationsPageView : UserControl
{
    public OrganizationsPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
