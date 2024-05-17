using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FS.Models;
using FS.ViewModels;

namespace FS.Views;

public partial class TransfersView : ContentPage
{
    private TransfersViewModel viewModel;
    public TransfersView(FileSenderServer FSServer)
    {
        viewModel = new TransfersViewModel(FSServer);
        InitializeComponent();
        BindingContext = viewModel;
        Appearing += OnPageAppearing;
        Title = "Past Transfers";
    }
    

    private void OnPageAppearing(object sender, EventArgs e)
    {
        if (viewModel.Transfers.Length == 0)
        {
            viewModel.RefreshData();
        }
    }
    async void InspectTransfer(object? sender, ItemTappedEventArgs itemTappedEventArgs)
    {
        var transfer = ((ListView)sender).SelectedItem as Transfer;
        if (transfer is not null)
        {
            var page = new TransferDetailView(transfer,viewModel.FSServer);
            Debug.WriteLine(Navigation.NavigationStack.Count);
            await Navigation.PushAsync(page);
            //await Navigation.PushAsync(page);
        }
    }
    
    
}