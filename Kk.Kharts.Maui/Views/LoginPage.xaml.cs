using Kk.Kharts.Maui.ViewModels;

namespace Kk.Kharts.Maui.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
