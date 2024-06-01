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
            //todo: this is logged as the recipient, work out a way to use download.php with our auth token.
            var file = (TransferFile)((ItemTappedEventArgs)args).Item;
            Download(new []{file});
    }
    public async void SaveTransfer(object sender, EventArgs args)
    {
        Download(viewModel.Transfer.Files);
        await Toast.Make($"Not implemented.").Show();
    }

    private async void Download(TransferFile[] files)
    {
        try
        {
        var request = await viewModel.FsServer.GetDownloadStream(files.Select(x => x.Id.ToString()).ToArray());
        var tmpFileName = files.Length == 1 ? files[0].Name : $"{viewModel.Transfer.Subject ?? "Transfer"}.zip";
        var fileSaverResult = await fileSaver.SaveAsync(tmpFileName, request);
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
    
    public async void ShowAuditLog(object sender, EventArgs args)
    {
        var auditLog = await viewModel.FsServer.getAuditLog(viewModel.Transfer.Id);
        Navigation.PushAsync(new AuditLogView(auditLog));
    }
    
}