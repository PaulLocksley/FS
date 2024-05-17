using CommunityToolkit.Mvvm.ComponentModel;
using FS.Models;

namespace FS.ViewModels;

public partial class AuditLogViewModel : ObservableObject
{
    [ObservableProperty] private AuditLogEvent[] auditLogs;


    public AuditLogViewModel(AuditLogEvent[] auditLogs)
    {
        this.auditLogs = auditLogs;
    }
}