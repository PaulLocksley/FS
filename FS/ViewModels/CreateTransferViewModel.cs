
using CommunityToolkit.Mvvm.ComponentModel;

namespace FS.ViewModels;
public partial class CreateTransferViewModel : ObservableObject
{
    public FileSenderServer FSServer;

    [ObservableProperty]
    private IDictionary<string, (String MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize)> selectedFiles = new Dictionary<string,  (String MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize)>();

    [ObservableProperty]
    private string recipient;
    
    [ObservableProperty]
    private string subject;

    [ObservableProperty]
    private string description;

    [ObservableProperty] 
    private long totalFileSize;
    
    public CreateTransferViewModel(FileSenderServer fsServer)
    {
        FSServer = fsServer;
        Recipient = "";
        Subject = "";
        Description = "";
    }
    
    public Task SendTransfer(string recipient2, string subject2, string description2)
    {
        return FSServer.SendTransfer(new string[]{recipient2},
            subject2,
            description2,
            SelectedFiles.Select(x => x.Value).ToArray());
    }

    public bool IsValidTransferState()
    {
        return true;
    }
    
}