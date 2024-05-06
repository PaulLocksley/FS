
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
            Text = "Nooooooo"
        };

        VStack.Children.Add(k);
        this.Appearing += OnPageAppearing;

    }
    private void OnPageAppearing(object sender, EventArgs e)
    {
        LoadConfig();
    }

    private async void LoadConfig()
    {
        try
        {
            var config = await FileSenderServerConfigFactory.BuildConfig();
            var fsServer = new FileSenderServer(config);
            Debug.WriteLine("loaded");
            /*MainThread.BeginInvokeOnMainThread(() =>
            {
                VStack.Children.Clear();
                VStack.Children.Add(new CreateTransfereView(fsServer));
            });*/
            Application.Current.MainPage = new CreateTransferView(fsServer);

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