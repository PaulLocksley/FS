using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using FS.Models;
using FS.ViewModels;

namespace FS.Views;

public partial class CreateTransferView : ContentPage
{
    private CreateTransferViewModel viewModel;
    
    public CreateTransferView(FileSenderServer fsServer)
    {
        InitializeComponent();
        viewModel = new CreateTransferViewModel(fsServer);

        //FilesListView = new ListView();
        FilesListView.ItemsSource = viewModel.SelectedFiles.Keys;
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
            Console.WriteLine("Breakhere.");
            foreach (var fresult in pickAndShow)
            {
                var file_task = fresult.OpenReadAsync();
                if (file_task.Result.Length == 0)
                {
                    var toast = Toast.Make($"File {fresult.FileName} ignored, reason: 0 bytes long.");
                    await toast.Show();
                    //todo: better solution for cloud files.
                    continue;
                }

                if (viewModel.FSServer.config.BannedFileTypes.Contains(fresult.FileName.Split('.').Last()))
                {
                    var toast = Toast.Make($"File {fresult.FileName} ignored, reason: banned Filetype.");
                    await toast.Show();
                    continue;
                }

                if (viewModel.FSServer.config.MaxTransferSize > viewModel.TotalFileSize + file_task.Result.Length)
                {
                    var toast = Toast.Make($"File {fresult.FileName} ignored, reason: Transfer over max file size.");
                    await toast.Show();
                    continue;
                }

                if (viewModel.FSServer.config.MaxFilesCount > viewModel.SelectedFiles.Count + 1)
                {
                    var toast = Toast.Make($"File {fresult.FileName} ignored, reason: Transfer over max file count.");
                    await toast.Show();
                    continue;
                }
                viewModel.TotalFileSize += file_task.Result.Length;
                
                Console.WriteLine(file_task.Status);
                viewModel.SelectedFiles[fresult.FullPath] = (fresult.ContentType,file_task,fresult.FullPath,fresult.FileName,file_task.Result.Length);
            }
            MainThread.BeginInvokeOnMainThread(() =>
            {
                FilesListView.ItemsSource = viewModel.SelectedFiles.Keys;
                SelectFilesBtn.Text = viewModel.SelectedFiles.Aggregate("", (x, y) => $"{x} {y.Value.FileName} {y.Value.FileSize}");
                //SelectFilesBtn.Text = viewModel.SelectedFiles.Select(x => x.Value.FileName).Aggregate("", (x, y) => $"{x} {y}");
            });
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
        var trans = viewModel.SendTransfer(EmailInput.Text,SubjectInput.Text,DescriptionInput.Text);
        
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
            viewModel.SelectedFiles =
                new Dictionary<string, (String MimeType, Task<Stream> FileStream, String FullPath, String FileName, long
                    FileSize)>();
        });

    }
}