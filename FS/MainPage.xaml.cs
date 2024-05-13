
using System.Diagnostics;
using FS.Models;
using FS.Views;

namespace FS;

public partial class MainPage : ContentPage
{
    private string o = "non loaded";
    public MainPage()
    {
        InitializeComponent();
        var k = new Label
        {
            Text = "Loading..."
        };

        VStack.Children.Add(k);
        this.Appearing += OnPageAppearing;

        var login_button = new Button();
        login_button.Text = "Login";
        login_button.Clicked += Login;
        VStack.Children.Add(login_button);
    }
    private void OnPageAppearing(object sender, EventArgs e)
    {
        LoadConfig();
    }
    private void Login(object sender, EventArgs e)
    {
        Navigation.PushAsync(new LoginView());
    }

    private async void LoadConfig()
    {
        try
        {
            var config = await FileSenderServerConfigFactory.BuildConfig();
            var fsServer = new FileSenderServer(config);
            Debug.WriteLine("loaded main page");
            App.Current.MainPage = new LoggedInView(fsServer);

        }
        catch(Exception e)
        {
            Debug.WriteLine($"Failed with {e}");
            
            //VStack.Children.Clear();
            Navigation.PushAsync(new LoginView());
            //VStack.Children.Add(new LoginView());
        }
    }
    
}