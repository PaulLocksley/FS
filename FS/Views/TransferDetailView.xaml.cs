using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using FS.Models;
using FS.ViewModels;

namespace FS.Views;

public partial class TransferDetailView : ContentPage
{
    private TransferDetailViewModel viewModel;
    private readonly IFileSaver fileSaver;
    public TransferDetailView(Transfer transfer, FileSenderServer FSServer)
    {
        
        InitializeComponent();
        this.fileSaver = FileSaver.Default;
        viewModel = new TransferDetailViewModel(transfer,FSServer);
        BindingContext = viewModel;
        Title = transfer.Subject ?? string.Empty;

    }

    public async void SaveFile(object sender, EventArgs args)
    {
        try
        {
            //todo: this is logged as the recipient, work out a way to use download.php with our auth token.
            var file = (TransferFile)((ItemTappedEventArgs)args).Item;
            var startUrl = viewModel.Transfer.Recipients.First().DownloadUrl.ToString();
            var downloadUrl = $"{startUrl.Replace("?s=download&", "download.php?")}&files_ids={file.Id}";
            //from transfer
            //https://fs.locksley.dev/filesender/?s=download&token=ad4ac5be-a8f3-472e-b32c-de83e2494504
            
            //from page
            //https://fs.locksley.dev/filesender/download.php?token=ad4ac5be-a8f3-472e-b32c-de83e2494504&files_ids=1977

            var downloadClient = new HttpClient();
            var stream = await downloadClient.GetStreamAsync(new Uri(
                downloadUrl));
            var fileSaverResult = await fileSaver.SaveAsync(file.Name, stream);
            fileSaverResult.EnsureSuccess();
            await Toast.Make($"File is saved: {fileSaverResult.FilePath}").Show();
        }
        catch (Exception e)
        {
            await Toast.Make($"File failed to download").Show();
            Debug.WriteLine("Failed to save file.");
            Debug.WriteLine(e);
        }
    }
    public async void SaveTransfer(object sender, EventArgs args)
    {
        //todo: confirm zip settings on server before implementation.
        await Toast.Make($"Not implemented.").Show();

    }
    
    public async void ShowAuditLog(object sender, EventArgs args)
    {
        var auditLog = await viewModel.FsServer.getAuditLog(viewModel.Transfer.Id);
        Navigation.PushAsync(new AuditLogView(auditLog));
    }
    
}