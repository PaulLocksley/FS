using System.Diagnostics;
using System.Globalization;
using System.Text;
using Encoding = System.Text.Encoding;

namespace FS;

public partial class MainPage : ContentPage
{
    int count = 0;
    private string o = "non loaded";
    private FileSenderServer FSServer = new FileSenderServer();
    public IDictionary<string, (String MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize)> SelectedFiles = new Dictionary<string,  (String MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize)>();
    public MainPage()
    {
        InitializeComponent();
        //FilesListView = new ListView();
        FilesListView.ItemsSource = SelectedFiles.Keys;
        CounterCount.Text = FSServer.countdown.CurrentCount.ToString();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;
        var k = PickAndShow();
     
        //CounterBtn.Text = SelectedFiles.Select(x => x.Key).Aggregate("", (x, y) => $"{x} {y}"); 

        /*if (count == 1)
            CounterBtn.Text = $"Clicked {count} time {o}";
        else
            CounterBtn.Text = $"Clicked {count} times {o}";*/
        SemanticScreenReader.Announce(CounterBtn.Text);
    }
    
    public async Task<IEnumerable<FileResult>> PickAndShow()
    {
        try
        {
            var result = await FilePicker.Default.PickMultipleAsync(new PickOptions());
            foreach (var fresult in result)
            {
                
                var file_task = fresult.OpenReadAsync();
                redo.Text = file_task.Result.Length.ToString();
                Console.WriteLine(file_task.Status);
                SelectedFiles[fresult.FullPath] = (fresult.ContentType,file_task,fresult.FullPath,fresult.FileName,file_task.Result.Length);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    FilesListView.ItemsSource = SelectedFiles.Keys;
                    CounterBtn.Text = SelectedFiles.Select(x => x.Key).Aggregate("", (x, y) => $"{x} {y}");
                });
            }
            return result;
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
        }

        //CounterBtn.Text = SelectedFiles.Select(x => x.Key).Aggregate("", (x, y) => $"{x} {y}"); 
        return null;
    }

    private async void SendFiles(object? sender, EventArgs eventArgs)
    {
        var trans = FSServer.testTransfer(SelectedFiles.Select(x => x.Value).ToArray());
        MainThread.BeginInvokeOnMainThread(() => {SendFilesBtn.Text = "Sending";});
        while (!trans.IsCompleted)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CounterCount.Text = FSServer.getProgressPercent().ToString(CultureInfo.CurrentCulture);
                CounterProgress.ProgressTo(FSServer.getProgressPercent() ,
                    100, Easing.Linear);
            });
            await Task.Delay(100);
        }
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SendFilesBtn.Text = "Complete";
            CounterBtn.Text = "Select Files";
            SelectedFiles =
                new Dictionary<string, (String MimeType, Task<Stream> FileStream, String FullPath, String FileName, long
                    FileSize)>();
        });

    }
}