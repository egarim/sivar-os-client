using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SivarOs.Services;

namespace SivarOs.Views;

public sealed partial class LoginPage : Page
{
    private readonly MatrixClient _client = new();

    public LoginPage() => InitializeComponent();

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Visibility = Visibility.Collapsed;
        BtnLogin.IsEnabled = false;
        LoadingRing.IsActive = true;

        _client.HomeServerUrl = TxtHomeServer.Text.TrimEnd('/');

        var user = await _client.LoginAsync(TxtUsername.Text.Trim(), TxtPassword.Password);

        LoadingRing.IsActive = false;
        BtnLogin.IsEnabled = true;

        if (user == null)
        {
            TxtError.Text = "Login failed. Check your username, password, and homeserver URL.";
            TxtError.Visibility = Visibility.Visible;
            return;
        }

        App.NavigateTo(new MainPage(_client));
    }
}
