using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS;

public partial class LoginView : ContentPage
{
    public LoginView()
    {
        InitializeComponent();
    }
    
    private async void onLoggedIn(object sender, EventArgs e)
    {
      
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
        foreach (var line in fake_node_text.Split("\n").Where(s => s.Contains('=')))
        {
            var tmp_vals = line.Split('=');
            if (tmp_vals.Length != 2)
            {
                throw new ArgumentOutOfRangeException($"Config line causing issues: {line}");
            }
            await SecureStorage.Default.SetAsync(tmp_vals[0].Trim(), tmp_vals[1].Trim());
        }
        
        var folder = FileSystem.Current.AppDataDirectory;

    }
}