using Kk.Kharts.Maui.Views;

namespace Kk.Kharts.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("deviceDetail", typeof(DeviceDetailPage));
        Routing.RegisterRoute("chart", typeof(ChartPage));
    }
}
