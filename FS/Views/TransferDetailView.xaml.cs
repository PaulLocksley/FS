using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FS.Models;
using FS.Utilities;
using FS.ViewModels;

namespace FS.Views;

public partial class TransferDetailView : ContentPage
{
    private TransferDetailViewModel viewModel;
    public TransferDetailView(Transfer transfer)
    {
        InitializeComponent();
        viewModel = new TransferDetailViewModel(transfer);
        BindingContext = viewModel;
        Title = transfer.Subject ?? string.Empty;

        /*var downloadUrl = transfer.Recipients.First().DownloadUrl.ToString();
        var vstack = new VerticalStackLayout();
        var scrollView = new ScrollView();
        scrollView.Content = vstack;
        
        vstack.Children.Add(new Label
        {
            Text = transfer.ViewRecipients
            
        });

        foreach (var file in transfer.Files)
        {
            vstack.Add(new HorizontalStackLayout
            {
                new Label
                {
                    Text = file.Name
                },
                new Label
                {
                    Text = FileSize.getHumanFileSize(file.Size),
                    FontSize = 10
                }
            });
        }
        
        
        Content = scrollView;
        */
        
    }
}