using CommunityToolkit.Mvvm.ComponentModel;
using FS.Models;

namespace FS.ViewModels;

public partial class TransferDetailViewModel(Transfer transfer) : ObservableObject
{
    [ObservableProperty]
    private Transfer transfer = transfer;
    
    
}