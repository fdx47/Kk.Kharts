using Kk.Kharts.Maui.ViewModels;

namespace Kk.Kharts.Maui.Views;

public partial class DevicesPage : ContentPage
{
    private readonly DevicesViewModel _viewModel;

    public DevicesPage(DevicesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
}
