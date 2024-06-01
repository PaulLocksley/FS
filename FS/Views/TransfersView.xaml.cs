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

    private async void SortTransfers(object sender, EventArgs e)
    {        
        
        string action = await DisplayActionSheet("Sort List By:", "Cancel", null, 
            "Created Date (Asc)", "Created Date (Desc)",
            "Expiry Date (Asc)", "Expiry Date (Desc)",
            "Transfer Size (Asc)", "Transfer Size (Desc)");
        (TransferSortType sortType, bool desc) sortType = action switch
        {
            "Created Date (Asc)" => (TransferSortType.Creation,false),
            "Created Date (Desc)" => (TransferSortType.Creation,true),
            "Expiry Date (Asc)" => (TransferSortType.Expiry,false),
            "Expiry Date (Desc)" => (TransferSortType.Expiry,true),
            "Transfer Size (Asc)" => (TransferSortType.Size,false),
            _ => (TransferSortType.Size,true)
        };
        viewModel.ReSort(sortType);
    }
    
}