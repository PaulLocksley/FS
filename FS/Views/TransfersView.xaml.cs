using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FS.ViewModels;

namespace FS.Views;

public partial class TransfersView : ContentPage
{
    private TransfersViewModel viewModel = new TransfersViewModel();
    public TransfersView()
    {
        InitializeComponent();
        Appearing += OnPageAppearing;
        Title = "Past Transfers";
    }
    
    private void OnPageAppearing(object sender, EventArgs e)
    {
        viewModel.ReloadConfig();
    }
    
    
}