using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.Views;

public partial class AccountInformationView : ContentPage
{
    public AccountInformationView(FileSenderServer fsServer)
    {
        var VStack = new VerticalStackLayout();
        var email = new Label();
        email.Text = fsServer.config.Username;
        VStack.Children.Add(email);
        InitializeComponent();
        Content = new Grid
        {
            Children =
            {
                VStack

            }
        };

        Title = "Account Information";
    }
}