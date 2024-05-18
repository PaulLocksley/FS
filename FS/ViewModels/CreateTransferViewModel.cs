
using System.Diagnostics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FS.Models;

namespace FS.ViewModels;
public partial class CreateTransferViewModel : ObservableObject
{
    public FileSenderServer FsServer;

    public HashSet<CreateTransferFile> SelectedFiles;

    
    [ObservableProperty]
    private string recipient;
    
    [ObservableProperty]
    private string subject;

    [ObservableProperty]
    private string description;

    [ObservableProperty] 
    private long totalFileSize;

    [ObservableProperty]
    private bool transferActive;

    [ObservableProperty]
    private IDictionary<string, Guid> fileListIndex = new Dictionary<string, Guid>();

    private Transfer? activeTransfer;
    public CancellationTokenSource TransferCancellationToken = new CancellationTokenSource();
    public CreateTransferViewModel(FileSenderServer fsServer)
    {
        FsServer = fsServer;
        Recipient = "";
        Subject = "";
        Description = "";
        TransferActive = false;
        SelectedFiles = [];
    }

    [RelayCommand]
    public void CancelTransfer()
    {
        TransferCancellationToken.Cancel();
    }

    public void AddFile(CreateTransferFile file)
    {
        SelectedFiles.Add(file);
    }
    
    public async Task SendTransfer(string recipient2, string subject2, string description2,CancellationToken cancellationToken)
    {

        TransferActive = true;

        activeTransfer = await FsServer.CreateTransfer(new string[] { recipient2 },
            subject2,
            description2,
            SelectedFiles.ToArray());

        var cidDictionary = SelectedFiles.ToDictionary(x => x.FileId, x => x.FileStream);
        await FsServer.SendTransfer(cidDictionary,activeTransfer,cancellationToken);
        TransferActive = false;
        return;
    }

    public bool IsValidTransferState()
    {
        return true;
    }
    
}