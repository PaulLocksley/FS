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
        Debug.WriteLine("here1");
        this.Appearing += OnPageAppearing;

    }
    
    private void OnPageAppearing(object sender, EventArgs e)
    {
        Debug.WriteLine("here2");
        onLoggedIn(sender, e);
    }
    
    private async void onLoggedIn(object sender, EventArgs e)
    {
        await Task.Delay(10000);
        Debug.WriteLine("here3");
        while (true)
        {
            await Task.Delay(500);
            Debug.WriteLine("here4");
            var k = await LoginForm.EvaluateJavaScriptAsync("window.filesender && window.filesender.client.base_path");
            Debug.WriteLine(k);
            if (k is not null)
            {
                break;
            }
        }
        var get_config = LoginForm.EvaluateJavaScriptAsync(
            "var temp = document.createElement('p');temp.id = 'fake_node';temp.innerText = 'init';document.body.insertBefore(temp, document.body.childNodes[0]);fetch(window.location.href.split('?')[0]+'clidownload.php?config=1').then((response) => response.text()).then((text) => temp.innerText = (text));");
        /*
           Eval seems to fail with """ strings for some reason. Not terible formating:
          var temp = document.createElement('p');
          temp.id = 'fake_node';
          temp.innerText = 'init';
          document.body.insertBefore(temp, document.body.childNodes[0]);
          fetch(window.location.href.split('?')[0]+'clidownload.php?config=1').then((response) => response.text()).then((text) => temp.innerText = (text));
         */
        
        
        var fake_node_text = await LoginForm.EvaluateJavaScriptAsync("document.getElementById('fake_node').innerText");
        while (fake_node_text == "init")// || kkk == "null")
        {
            Thread.Sleep(100);
            fake_node_text = await LoginForm.EvaluateJavaScriptAsync("document.getElementById('fake_node').innerText");
        }
        Thread.Sleep(100);

        var allowed_keys = new string[] { "apikey", "username", "default_transfer_days_valid ", "base_url" };
        foreach (var line in fake_node_text.Split("\\n").Where(s => s.Contains('=')))
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
        }

        Navigation.PopAsync();
    }
}