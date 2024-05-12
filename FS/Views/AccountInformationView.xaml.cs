using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.Views;

public partial class AccountInformationView : ContentPage
{
    public AccountInformationView(FileSenderServer fsServer)
    {
        InitializeComponent();
        Title = "Account Information";
        UserId.Text = fsServer.config.Username;
        InstanceName.Text = fsServer.config.SiteName;
        InstanceUrl.Text = fsServer.config.BaseUrl.Replace("/rest.php","").Replace("https://","");
    }

    private async void LogOut(object? sender, EventArgs e)
    {
        bool logoutCheck = await DisplayAlert("Confirm", "Would you like to log out", "Yes", "No");
        Debug.WriteLine("Answer: " + logoutCheck);
        if (!logoutCheck)
        {
            return;
        }
        
        SecureStorage.RemoveAll();
        throw new NotImplementedException("I can't quite workout how to reset shell state safely yet. todo");
    }
}