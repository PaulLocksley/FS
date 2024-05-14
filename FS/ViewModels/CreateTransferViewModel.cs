
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FS.Models;

namespace FS.ViewModels;
public partial class CreateTransferViewModel : ObservableObject
{
    public FileSenderServer FSServer;

    [ObservableProperty]
    private IDictionary<string, (String MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize, string fileID)> selectedFiles = 
        new Dictionary<string,  (String MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize, string fileID)>();

    
    
    [ObservableProperty]
    private string recipient;
    
    [ObservableProperty]
    private string subject;

    [ObservableProperty]
    private string description;

    [ObservableProperty] 
    private long totalFileSize;

    [ObservableProperty] private bool transferActive = false;

    [ObservableProperty]
    private IDictionary<string, Guid> fileListIndex = new Dictionary<string, Guid>();

    private Transfer? activeTransfer;
    public CancellationTokenSource TransferCancellationToken = new CancellationTokenSource();
    public CreateTransferViewModel(FileSenderServer fsServer)
    {
        FSServer = fsServer;
        Recipient = "";
        Subject = "";
        Description = "";
    }

    [RelayCommand]
    private void CancelTransfer()
    {
        TransferCancellationToken.Cancel();
    }
    public async Task SendTransfer(string recipient2, string subject2, string description2,CancellationToken cancellationToken)
    {

        
        activeTransfer = await FSServer.CreateTransfer(new string[] { recipient2 },
            subject2,
            description2,
            SelectedFiles.Select(x => x.Value).ToArray());

        var cidDictionary = SelectedFiles.ToDictionary(x => x.Value.fileID, x => x.Value.FileStream);
        await FSServer.SendTransfer(cidDictionary,activeTransfer,cancellationToken);
        return;
    }

    public bool IsValidTransferState()
    {
        return true;
    }
    
}