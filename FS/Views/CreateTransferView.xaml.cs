using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Markup;
using FS.Models;
using FS.Utilities;
using FS.ViewModels;
using Microsoft.Extensions.Primitives;

namespace FS.Views;

public partial class CreateTransferView : ContentPage
{
    private CreateTransferViewModel viewModel;
    
    public CreateTransferView(FileSenderServer fsServer)
    {
        InitializeComponent();
        Title = "Create Transfer";
        viewModel = new CreateTransferViewModel(fsServer);

        
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
                
                if ( viewModel.TotalFileSize + file_task.Result.Length > viewModel.FSServer.config.MaxTransferSize)
                {
                    var toast = Toast.Make($"File {fresult.FileName} ignored, reason: Transfer over max file size.");
                    await toast.Show();
                    continue;
                }
                viewModel.TotalFileSize += file_task.Result.Length;

                if (viewModel.SelectedFiles.Count + 1 > viewModel.FSServer.config.MaxFilesCount)
                {
                    var toast = Toast.Make($"File {fresult.FileName} ignored, reason: Transfer over max file count.");
                    await toast.Show();
                    continue;
                }
                
                viewModel.SelectedFiles[fresult.FullPath] = (fresult.ContentType,file_task,fresult.FullPath,fresult.FileName,file_task.Result.Length,Guid.NewGuid().ToString());
            }
            
            UpdateFileList();

            return pickAndShow;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            var toast = Toast.Make($"The program encountered an error selecting files.\n  {ex}.");
            await toast.Show();
            // The user canceled or something went wrong
        }
        return new List<FileResult>();
    }

    private void UpdateFileList()
    {
        var fileList = new List<IView>();
        foreach (var file in viewModel.SelectedFiles)
        {
            var container = new HorizontalStackLayout();
            viewModel.FileListIndex[file.Key] = container.Id;
            container.AutomationId = container.Id.ToString();
            var name = new Label();
            name.Text = file.Value.FileName;
            name.WidthRequest = 150;
            name.LineBreakMode = LineBreakMode.CharacterWrap;
            name.FontSize = 12;
            name.HorizontalOptions = LayoutOptions.Start;
            container.Add(name);

            var fileSize = new Label();
            fileSize.Text = FileSize.getHumanFileSize(file.Value.FileSize);
            fileSize.WidthRequest = 50;
            fileSize.Margin = new Thickness(10, 0, 0, 0);
            container.Add(fileSize);

            var deleteButton = new Button();
            deleteButton.Text = "\u2796";
            deleteButton.BackgroundColor = Colors.DarkGray;
            deleteButton.FontAttributes = FontAttributes.Bold;
            
            //deleteButton.FontSize = 10;
            deleteButton.CommandParameter = file.Key;
            deleteButton.Clicked += DeleteFile;
            deleteButton.HeightRequest = 15;
            deleteButton.WidthRequest = 15;
            deleteButton.HorizontalOptions = LayoutOptions.End;
            deleteButton.Margin = new Thickness(40,5,0,5);
                                
            container.Add(deleteButton);
            container.HorizontalOptions = LayoutOptions.Fill;
                
            fileList.Add(container);
        }
        MainThread.BeginInvokeOnMainThread(() =>
        {
            FileContainer.Children.Clear();
            foreach (var view in fileList)
            {
                FileContainer.Children.Add(view);
            }
            
            //SelectFilesBtn.Text = viewModel.SelectedFiles.Select(x => x.Value.FileName).Aggregate("", (x, y) => $"{x} {y}");
        });
        
    }

    private async void DeleteFile(object? sender, EventArgs eventArgs)
    {
        System.Reflection.PropertyInfo pi = sender.GetType().GetProperty("CommandParameter");
        string key = (string)(pi.GetValue(sender, null));
        
        if (!viewModel.SelectedFiles.ContainsKey(key)) return;
        
        viewModel.SelectedFiles.Remove(key);
        var viewID = viewModel.FileListIndex[key];
        viewModel.FileListIndex.Remove(key);
        var view = FileContainer.Children.First(x => x.AutomationId == viewID.ToString());
        MainThread.BeginInvokeOnMainThread(() => {  FileContainer.Remove(view); });
    }

    private async void SendFiles(object? sender, EventArgs eventArgs)
    {
        viewModel.TransferCancellationToken = new CancellationTokenSource();
        var trans = viewModel.SendTransfer(EmailInput.Text,SubjectInput.Text,DescriptionInput.Text,viewModel.TransferCancellationToken.Token);
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
                    FileSize,string fileID)>();
        });
        var toast = Toast.Make($"Transfer complete.");
        toast.Show();
        UpdateFileList();
    }
}