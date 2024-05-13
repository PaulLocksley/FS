using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using FS.Models;

namespace FS.ViewModels;


public partial class TransfersViewModel : ObservableObject
{
    public FileSenderServer FSServer;
    
    [ObservableProperty]
    public Transfer[] transfers = [];
    public async Task ReloadConfig()
    {
        Transfers = await FSServer.GetAllTransfers();

    }

    public TransfersViewModel(FileSenderServer fsServer)
    {
        FSServer = fsServer;
    }
    public TransfersViewModel()
    {
        FSServer = new FileSenderServer(new FileSenderServerConfig("",
            "","",23,"",232323,1,12,1,1,new List<string>()));
    }
}