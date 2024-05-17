using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FS.Models;
using FS.ViewModels;

namespace FS.Views;

public partial class AuditLogView : ContentPage
{
    private AuditLogViewModel viewModel;
    public AuditLogView(AuditLogEvent[] auditLog)
    {
        viewModel = new AuditLogViewModel(auditLog);
        InitializeComponent();
        Title = "Audit Log";
        BindingContext = viewModel;
    }
}