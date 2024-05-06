
using CommunityToolkit.Mvvm.ComponentModel;
using FS.Models;

namespace FS.ViewModels;
public class CreateTransferViewModel : ObservableObject
{
    public FileSenderServer FSServer;

    public CreateTransferViewModel(FileSenderServer fsServer)
    {
        FSServer = fsServer;
    }
}