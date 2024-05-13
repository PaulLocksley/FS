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
}