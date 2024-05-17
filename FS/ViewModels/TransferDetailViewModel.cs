using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FS.Models;

namespace FS.ViewModels;

public partial class TransferDetailViewModel(Transfer transfer,FileSenderServer FSServer) : ObservableObject
{
    [ObservableProperty]
    private Transfer transfer = transfer;

    public FileSenderServer FsServer = FSServer;

}