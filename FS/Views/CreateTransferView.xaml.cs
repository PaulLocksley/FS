using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FS.Models;
using FS.ViewModels;

namespace FS.Views;

public partial class CreateTransferView : ContentPage
{
    private string o = "non loaded";
    private CreateTransferViewModel viewModel;
    public IDictionary<string, (String MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize)> SelectedFiles = new Dictionary<string,  (String MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize)>();
    
    public CreateTransferView(FileSenderServer fsServer)
    {
        InitializeComponent();
        viewModel = new CreateTransferViewModel(fsServer);
        //FilesListView = new ListView();
        FilesListView.ItemsSource = SelectedFiles.Keys;
    }

    private void SelectFiles(object sender, EventArgs e)
    {
        //todo: look at the send and fire thing from that one async talk. 
        PickAndShow();
    }
    
    public async Task<IEnumerable<FileResult>> PickAndShow()
    {
        try
        {
            var result = await FilePicker.Default.PickMultipleAsync(new PickOptions());
            var pickAndShow = result as FileResult[] ?? result.ToArray();
            foreach (var fresult in pickAndShow)
            {
                var file_task = fresult.OpenReadAsync();
                //redo.Text = file_task.Result.Length.ToString();
                Console.WriteLine(file_task.Status);
                SelectedFiles[fresult.FullPath] = (fresult.ContentType,file_task,fresult.FullPath,fresult.FileName,file_task.Result.Length);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    FilesListView.ItemsSource = SelectedFiles.Keys;
                    SelectFilesBtn.Text = SelectedFiles.Select(x => x.Key).Aggregate("", (x, y) => $"{x} {y}");
                });
            }
            return pickAndShow;
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
        }
        return new List<FileResult>();
    }

    private async void SendFiles(object? sender, EventArgs eventArgs)
    {
        var trans = viewModel.FSServer.SendTransfer(new string[]{EmailInput.Text},
            SubjectInput.Text,
            DescriptionInput.Text,
            SelectedFiles.Select(x => x.Value).ToArray());
        
        MainThread.BeginInvokeOnMainThread(() => {SendFilesBtn.Text = "Sending";});
        while (!trans.IsCompleted)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CounterCount.Text = viewModel.FSServer.getProgressPercent().ToString(CultureInfo.CurrentCulture);
                CounterProgress.ProgressTo(viewModel.FSServer.getProgressPercent() ,
                    100, Easing.Linear);
            });
            await Task.Delay(100);
        }
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SendFilesBtn.Text = "Complete";
            SelectFilesBtn.Text = "Select Files";
            SelectedFiles =
                new Dictionary<string, (String MimeType, Task<Stream> FileStream, String FullPath, String FileName, long
                    FileSize)>();
        });

    }
}