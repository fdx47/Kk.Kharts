using Kk.Kharts.Maui.ViewModels;

namespace Kk.Kharts.Maui.Views;

public partial class DeviceDetailPage : ContentPage
{
    public DeviceDetailPage(DeviceDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
