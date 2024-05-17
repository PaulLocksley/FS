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
    public TransferDetailView(Transfer transfer)
    {
        
        InitializeComponent();
        this.fileSaver = FileSaver.Default;
        viewModel = new TransferDetailViewModel(transfer);
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
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            //from transfer
            //https://fs.locksley.dev/filesender/?s=download&token=ad4ac5be-a8f3-472e-b32c-de83e2494504
            
            //from page
            //https://fs.locksley.dev/filesender/download.php?token=ad4ac5be-a8f3-472e-b32c-de83e2494504&files_ids=1977

            var downloadClient = new HttpClient();
            var stream = await downloadClient.GetStreamAsync(new Uri(
                downloadUrl));

            var fileSaverResult = await fileSaver.SaveAsync(file.Name, stream, cancellationToken);
            fileSaverResult.EnsureSuccess();
            await Toast.Make($"File is saved: {fileSaverResult.FilePath}").Show(cancellationToken);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to save file.");
            Debug.WriteLine(e);
        }
    }
    public async void SaveTransfer(object sender, EventArgs args)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        //from transfer
        //https://fs.locksley.dev/filesender/?s=download&token=ad4ac5be-a8f3-472e-b32c-de83e2494504
        
        //from page
        //https://fs.locksley.dev/filesender/download.php?token=ad4ac5be-a8f3-472e-b32c-de83e2494504&files_ids=1977

        var o = new HttpClient();
        var stream = await o.GetStreamAsync(new Uri(
            "https://fs.locksley.dev/filesender/download.php?token=ad4ac5be-a8f3-472e-b32c-de83e2494504&files_ids=1977"));
        
        var fileSaverResult = await fileSaver.SaveAsync("test.pdf", stream, cancellationToken);
        fileSaverResult.EnsureSuccess();
        await Toast.Make($"File is saved: {fileSaverResult.FilePath}").Show(cancellationToken);
    }
    
}