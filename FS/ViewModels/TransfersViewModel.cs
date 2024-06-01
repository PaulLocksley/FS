using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FS.Models;

namespace FS.ViewModels;


public partial class TransfersViewModel : ObservableObject
{
    public FileSenderServer FSServer;
    
    [ObservableProperty]
    public Transfer[] transfers = [];

    [ObservableProperty] private bool isRefreshing = false;

    [RelayCommand]
    public async Task RefreshData()
    {
        IsRefreshing = true;
        Transfers = await FSServer.GetAllTransfers();
        IsRefreshing = false;
    }
    public TransfersViewModel(FileSenderServer fsServer)
    {
        FSServer = fsServer;
    }

    public void ReSort((TransferSortType sortType, bool desc)sortOptions)
    {
        IEnumerable<Transfer> sortedTransfers = sortOptions.sortType switch
        {
            TransferSortType.Creation => sortOptions.desc ? Transfers.OrderByDescending(x => x.Created.UnixTime) : Transfers.OrderBy(x => x.Created.UnixTime),
            TransferSortType.Expiry => sortOptions.desc ? Transfers.OrderByDescending(x => x.Expiry.UnixTime) : Transfers.OrderBy(x => x.Expiry.UnixTime),
            TransferSortType.Size => sortOptions.desc ? Transfers.OrderByDescending(x => x.FormatedTotalSizeNumeber) : Transfers.OrderBy(x => x.FormatedTotalSizeNumeber),
            _ => Transfers
        };
        Transfers = sortedTransfers.ToArray();
    }
}