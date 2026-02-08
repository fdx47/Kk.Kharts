using Kk.Kharts.Maui.ViewModels;
#if WINDOWS
using Microsoft.UI.Xaml.Input;
using Windows.System;
using NativePage = Microsoft.UI.Xaml.Controls.Page;
#endif

namespace Kk.Kharts.Maui.Views;

public partial class ChartPage : ContentPage
{
#if WINDOWS
    private NativePage? _nativePage;
#endif

    public ChartPage(ChartViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
#if WINDOWS
        HandlerChanged += OnHandlerChanged;
#endif
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ChartViewModel vm)
        {
            await vm.InitializeAsync();
        }
#if WINDOWS
        AttachNativeHandlers();
#endif
    }

#if WINDOWS
    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        AttachNativeHandlers();
    }

    private void AttachNativeHandlers()
    {
        if (_nativePage is not null)
        {
            _nativePage.KeyDown -= OnNativePageKeyDown;
            _nativePage = null;
        }

        if (Handler?.PlatformView is NativePage page)
        {
            _nativePage = page;
            _nativePage.KeyDown += OnNativePageKeyDown;
        }
    }

    private void OnNativePageKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (BindingContext is not ChartViewModel vm) return;

        if (e.Key == VirtualKey.Left && vm.PreviousDeviceCommand.CanExecute(null))
        {
            vm.PreviousDeviceCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == VirtualKey.Right && vm.NextDeviceCommand.CanExecute(null))
        {
            vm.NextDeviceCommand.Execute(null);
            e.Handled = true;
        }
    }
#endif
}
