using System.Diagnostics;

namespace FS.ViewModels;

public class TransfersViewModel
{
    public FileSenderServer FSServer;
    public async Task ReloadConfig()
    {
        var k = await FSServer.GetAllTransfers();
        Debug.WriteLine(k.Length);
        return;
    }

    public TransfersViewModel(FileSenderServer fsServer)
    {
        FSServer = fsServer;
    }
}