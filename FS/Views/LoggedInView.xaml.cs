﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.Views;

public partial class LoggedInView : TabbedPage
{
    public LoggedInView(FileSenderServer fsServer)
    {
        InitializeComponent();
        
        //var tabbedPage = new TabbedPage();
        TabGroup.Children.Add(new CreateTransferView(fsServer));
        var transfers = new NavigationPage(new TransfersView(fsServer));
        transfers.Title = "Transfers";
        TabGroup.Children.Add(transfers);
        TabGroup.Children.Add(new AccountInformationView(fsServer));

    }
}