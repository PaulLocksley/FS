
using System.Diagnostics;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Markup;
using FS.Models;
using FS.ViewModels;

namespace FS;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class LoginView : ContentPage
{
    private VerticalStackLayout LoginViewStack = new VerticalStackLayout();
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
            b.CommandParameter = server.Address;
            b.Clicked += SelectedServer;
            //b.Command = viewModel.testtttttCommand;
            serverList.Add(b);
        }

        var loginStateLabel = new Label
        {
            FontSize = 30,
        }.Bind(Label.TextProperty, nameof(viewModel.LoginState));//, BindingMode.OneWay);
        var customServer = new Button();
        customServer.Text = "Enter Custom URL";
        customServer.Clicked += SelectCustomServer;
        serverList.Add(customServer);
        
        serverList.Spacing = 10;
        var welcomeBanner = new Label();
        welcomeBanner.Text = "Select your server";
        welcomeBanner.HorizontalOptions = LayoutOptions.Center;
        welcomeBanner.FontSize = 30;
        welcomeBanner.FontAttributes = FontAttributes.Bold;
        LoginViewStack.Children.Add(welcomeBanner);
        LoginViewStack.Children.Add(loginStateLabel);
        LoginViewStack.Children.Add(serverList);
        Content = new Grid
        {
            Children =
            {
                LoginViewStack

            }
        };
        //InitializeComponent();
        
        //BindingContext = 

        //this.Appearing += OnPageAppearing;
        

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
        MainThread.BeginInvokeOnMainThread(() =>
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
        await Task.Delay(5000);
        try
        {
            string? loginCheckString = null;
            while (true)
            {
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

                viewModel.LoginState = "Fetching API Token...";
                Debug.WriteLine(loginCheckString);
                if (loginCheckString is not null)
                {
                    break;
                }
            }

            //User is now logged in.
            string settupConfigLocation =
                "var temp = document.createElement('p');temp.id = 'fake_node';temp.innerText = 'init';temp.style.fontSize = '0px';document.body.appendChild(temp);\"\"";
            string getConfigJS = "fetch(window.location.href.split('?')[0]+'clidownload.php?config=1').then((response) => response.text()).then((text) => temp.innerText = (text));\"\"";
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
                await Task.Delay(100);

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
                var warning = new Label
                {
                    Text = "Could not get valid api key. Ensure your FileSender account has a Secret under 'My Profile'"

                };
                LoginViewStack.Children.Insert(0, warning);
                LoginViewStack.Children.RemoveAt(1);
            }
        }
        catch (Exception ee)
        {
            Debug.WriteLine($"error {ee}");
        }

    }
}