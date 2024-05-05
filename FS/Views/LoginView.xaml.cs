using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS;

public partial class LoginView : ContentPage
{
    public LoginView()
    {
        InitializeComponent();
        this.Appearing += OnPageAppearing;

    }
    
    private void OnPageAppearing(object sender, EventArgs e)
    {
        YoinkConfigFromWebView(sender, e);
    }
    
    private async void YoinkConfigFromWebView(object sender, EventArgs e)
    {
        //Wait for user to login.
        await Task.Delay(5000);
        while (true)
        {
            await Task.Delay(500);
            var k = await LoginForm.EvaluateJavaScriptAsync("window.filesender && window.filesender.client.base_path");
            Debug.WriteLine(k);
            if (k is not null)
            {
                break;
            }
        }
        
        //User is now logged in.
        var get_config = LoginForm.EvaluateJavaScriptAsync(
            "var temp = document.createElement('p');temp.id = 'fake_node';temp.innerText = 'init';temp.style.fontSize = '0px';document.body.appendChild(temp);fetch(window.location.href.split('?')[0]+'clidownload.php?config=1').then((response) => response.text()).then((text) => temp.innerText = (text));");
        /*
           Eval seems to fail with """ strings for some reason. Not terible formating:
          var temp = document.createElement('p');
          temp.id = 'fake_node';
          temp.innerText = 'init';
          document.body.appendChild(temp);
          fetch(window.location.href.split('?')[0]+'clidownload.php?config=1').then((response) => response.text()).then((text) => temp.innerText = (text));
         */
        
        
        var fake_node_text = await LoginForm.EvaluateJavaScriptAsync("document.getElementById('fake_node').innerText");
        while (fake_node_text == "init")// || kkk == "null")
        {
            await Task.Delay(100);
            fake_node_text = await LoginForm.EvaluateJavaScriptAsync("document.getElementById('fake_node').innerText");
        }
        await Task.Delay(100);

        var allowed_keys = new string[] { "apikey", "username", "default_transfer_days_valid ", "base_url" };
        var apiKeySet = false;
        foreach (var line in fake_node_text.Split(@"\n").Where(s => s.Contains('=')))
        {
            var tmp_vals = line.Split('=');
            var key = tmp_vals[0].Trim();

            if (tmp_vals.Length != 2 || !allowed_keys.Contains(key))
            {
                Debug.WriteLine($"Skipping line: {line}");
                continue;
                throw new ArgumentOutOfRangeException($"Config line causing issues: {line}");
            }
            var val = tmp_vals[1].Trim();
            if (val == "") { continue; }
            Debug.WriteLine($"Trying to write {key} = {val}");
            await SecureStorage.Default.SetAsync(key, val);
            if (key == "apikey") { apiKeySet = true; }
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
            LoginViewStack.Children.Insert(0,warning);
            LoginViewStack.Children.RemoveAt(1);
        }
        
        
    }
}