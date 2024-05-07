using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FS.Models;

namespace FS.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] private List<DefaultServer> defaultPublicServers;

    public LoginViewModel()
    {
        defaultPublicServers = new List<DefaultServer>
        {
            new DefaultServer("AARnet FileSender", new Uri("https://filesender.edu.au")),
            new DefaultServer("My Dev server", new Uri("https://fs.locksley.dev/filesender")),
        };

    }


    [RelayCommand]
    private void testttttt(Uri address)
    {
        Debug.WriteLine($"A{address}");
    }
}