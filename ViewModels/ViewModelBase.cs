using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace KursMVVM.ViewModels;

public abstract class ViewModelBase : ObservableRecipient
{
    protected ViewModelBase() : base(WeakReferenceMessenger.Default)
    {
    }
}
