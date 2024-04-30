using System.Text;
using Encoding = System.Text.Encoding;

namespace FS;

public partial class MainPage : ContentPage
{
    int count = 0;
    private string o = "non loaded";
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;
        PickAndShow();
        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time {o}";
        else
            CounterBtn.Text = $"Clicked {count} times {o}";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
    public async Task<FileResult> PickAndShow()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions());
            if (result != null)
            {
                    using var stream = await result.OpenReadAsync();
                    var j = new FileInfo(result.FullPath);
                    var m = new byte[j.Length];
                    stream.Read(m);
                    o = Encoding.ASCII.GetString(m);
            }

            return result;
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
        }

        return null;
    }
}