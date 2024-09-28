
using System.Diagnostics;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;
using FS.Models;
using FS.ViewModels;

namespace FS;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class LoginView : ContentPage
{
    private Grid LoginViewGrid = new Grid();
    private WebView LoginForm = new WebView();
    private VerticalStackLayout serverList = new VerticalStackLayout();
    private LoginViewModel viewModel = new LoginViewModel();
    public LoginView()
    {
        BindingContext = viewModel;
        foreach (var server in viewModel.DefaultPublicServers)
        {
            var b = new Button();
            b.Text = server.Name;
            b.WidthRequest = 400;
            b.CommandParameter = server.Address;
            b.Clicked += SelectedServer;
            serverList.Add(b);
        }

        var customServer = new Button();
        customServer.Text = "Enter Custom URL";
        customServer.WidthRequest = 400;
        customServer.Clicked += SelectCustomServer;
        serverList.Add(customServer);
        
        serverList.Spacing = 10;
        
        var loginStateLabel = new Label
        {
            FontSize = 30,
            HorizontalTextAlignment = TextAlignment.Center,
            FontAttributes = FontAttributes.Bold
        }.Bind(Label.TextProperty, nameof(viewModel.LoginState));

        var welcomeBanner = new Label();
        welcomeBanner.Text = "Select your server";
        welcomeBanner.HorizontalOptions = LayoutOptions.Center;
        welcomeBanner.FontSize = 30;
        welcomeBanner.FontAttributes = FontAttributes.Bold;
        var serverListScrolView = new ScrollView
        {
            Content  = serverList,
            VerticalOptions = LayoutOptions.Fill,
        };

        LoginViewGrid = new Grid
        {
            RowDefinitions = Rows.Define(
                (Row.Heading, 50),
                (Row.Body, Star)),
            Children =
            {
                loginStateLabel
                    .Row(Row.Heading),
                serverListScrolView.Row(Row.Body)
            }
        };
        Content = LoginViewGrid;


    }

    private async void SelectCustomServer(object sender, EventArgs e)
    {
        try
        {
            string result = await DisplayPromptAsync("Server Url", "What is the servers web address?");
            var url = new UriBuilder(result).Uri;
            NavigateToServer(url);
        }
        catch (Exception ex)
        {
            await Toast.Make($"Failed to navigate to custom server ${ex}").Show();
        }
    }

    private async void SelectedServer(object sender, EventArgs e)
    {
        System.Reflection.PropertyInfo pi = sender.GetType().GetProperty("CommandParameter");
        Uri address = (Uri)(pi.GetValue(sender, null));
        NavigateToServer(address!);
    }

    private async void NavigateToServer(Uri address)
    {
        
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            serverList.Children.Clear();
            
            LoginForm.Source = address;
            LoginForm.HeightRequest = Window.Height;
            LoginForm.WidthRequest = Window.Width;
            serverList.Children.Add(LoginForm);
        });
        YoinkConfigFromWebView(null,null);
    }
    
    private void OnPageAppearing(object sender, EventArgs e)
    {
        YoinkConfigFromWebView(sender, e);
    }
    
    private async void YoinkConfigFromWebView(object sender, EventArgs e)
    {
        viewModel.LoginState = "Waiting for Login...";
        //Wait for user to login.
        Debug.WriteLine("Starting yoink!");
        await Task.Delay(1500);
        try
        {
            string? loginCheckString = null;
            while (true)
            {
                viewModel.LoginState = viewModel.LoginState.Length > 20 ? "Waiting for Login" : viewModel.LoginState + ".";
                await Task.Delay(500);
                //This is gross https://github.com/dotnet/maui/issues/20288
#if IOS || MACCATALYST
                var resultios =
                    await (LoginForm.Handler.PlatformView as WebKit.WKWebView).EvaluateJavaScriptAsync(
                        "window.filesender && window.filesender.client.base_path");
                loginCheckString = resultios?.ToString() ?? null;
                loginCheckString = loginCheckString is null or "<null>" ? null : loginCheckString;
#else
            loginCheckString =
 await LoginForm.EvaluateJavaScriptAsync("window.filesender && window.filesender.client.base_path");

#endif

                if (loginCheckString is not null)
                {
                    break;
                }
            }

            //User is now logged in.
            viewModel.LoginState = "Fetching API Token...";
            string settupConfigLocation =
                "var temp = document.createElement('p');temp.id = 'fake_node';temp.innerText = 'init';temp.style.fontSize = '0px';document.body.appendChild(temp);\"\"";
            string getConfigJS = "fetch(window.location.href.split('?')[0]+'clidownload.php?config=1').then((response) => { if (response.ok){return response.text()}else{return fetch(window.location.href.split('?')[0]+'rest.php/user/@me/filesender-python-client-configuration-file').then((response) => response.text())}}).then((text) => temp.innerText = (text)); \"\"";
#if IOS || MACCATALYST
            var tmp1 = await (LoginForm.Handler.PlatformView as WebKit.WKWebView).EvaluateJavaScriptAsync(settupConfigLocation);
            var tmp2 = await (LoginForm.Handler.PlatformView as WebKit.WKWebView).EvaluateJavaScriptAsync(getConfigJS);
#else
            var tmp3 = await LoginForm.EvaluateJavaScriptAsync(settupConfigLocation);
            var tmp4 = await LoginForm.EvaluateJavaScriptAsync(getConfigJS);

#endif




            var fake_node_text = "init";
            while (fake_node_text == "init") // || kkk == "null")
            {
                viewModel.LoginState = viewModel.LoginState.Length > 22 ? "Fetching API Token" : viewModel.LoginState + ".";
                await Task.Delay(500);

#if IOS || MACCATALYST
                var resultiostmp =
                    await (LoginForm.Handler.PlatformView as WebKit.WKWebView).EvaluateJavaScriptAsync(
                        "document.getElementById('fake_node').innerText");
                fake_node_text = resultiostmp.ToString();
#else
            fake_node_text = await LoginForm.EvaluateJavaScriptAsync("document.getElementById('fake_node').innerText");
#endif

            }

            await Task.Delay(100);
            viewModel.LoginState = "Success, Saving API Token...";
            var allowed_keys = new string[] { "apikey", "username", "default_transfer_days_valid ", "base_url" };
            var apiKeySet = false;
            foreach (var line in fake_node_text.Split(new char[]{'\\','\n'}).Where(s => s.Contains('=')))
            {
                var tmp_vals = line.Split('=');
                var key = tmp_vals[0].Trim();
                //on windows the js returns "\n" as a literal string breaking the above. 
                //Ios and Mac return \n as the newline char causing this difference.
                if (key.Length > 1 && key[0] == 'n')
                {
                    key = key.Remove(0, 1);
                }
                
                if (tmp_vals.Length != 2 || !allowed_keys.Contains(key))
                {
                    Debug.WriteLine($"Skipping line: {line}");
                    continue;
                    throw new ArgumentOutOfRangeException($"Config line causing issues: {line}");
                }

                var val = tmp_vals[1].Trim();
                if (val == "")
                {
                    continue;
                }

                Debug.WriteLine($"Trying to write {key} = {val}");
                await SecureStorage.Default.SetAsync(key, val);
                if (key == "apikey")
                {
                    apiKeySet = true;
                }
            }

            if (apiKeySet)
            {
                Navigation.PopAsync();
            }
            else
            {
                viewModel.LoginState =
                    "Could not get valid api key. Ensure your FileSender account has a Secret under 'My Profile'";
            }
        }
        catch (Exception ee)
        {
            Debug.WriteLine($"error {ee}");
        }

    }
    
    enum Row {Heading, Body}
}