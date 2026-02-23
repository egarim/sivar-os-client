using Microsoft.UI.Xaml;
using SivarOs.Views;

namespace SivarOs;

public partial class App : Application
{
    private Window? _window;

    public App() => InitializeComponent();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new Window();
        _window.Content = new LoginPage();
        _window.Activate();
    }

    public static void NavigateTo(UIElement page)
    {
        if (Current is App app && app._window != null)
            app._window.Content = page;
    }
}
