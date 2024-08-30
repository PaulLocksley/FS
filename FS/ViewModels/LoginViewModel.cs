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

    [ObservableProperty]
    private string loginState;
    public LoginViewModel()
    {
        defaultPublicServers = new List<DefaultServer>
        {
            new DefaultServer("AARnet FileSender", new Uri("https://filesender.edu.au")),
            new DefaultServer("My Dev server", new Uri("https://fs.locksley.dev/filesender/")),
        };
            loginState = "Select Your Server";

    }

}