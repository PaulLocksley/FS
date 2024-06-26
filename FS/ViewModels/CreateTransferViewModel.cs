﻿
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FS.Models;

namespace FS.ViewModels;
public partial class CreateTransferViewModel : ObservableObject
{
    public FileSenderServer FsServer;

    public HashSet<CreateTransferFile> SelectedFiles;


    [ObservableProperty]
    private string recipient;

    [ObservableProperty]
    private string subject;

    [ObservableProperty]
    private string description;

    [ObservableProperty] 
    private long totalFileSize;

    [ObservableProperty]
    private bool transferActive;

    [ObservableProperty]
    private IDictionary<string, Guid> fileListIndex = new Dictionary<string, Guid>();

    [ObservableProperty]
    private bool isValidTransferState;
    
    private Transfer? activeTransfer;
    public CancellationTokenSource TransferCancellationToken = new CancellationTokenSource();
    public CreateTransferViewModel(FileSenderServer fsServer)
    {
        FsServer = fsServer;
        Recipient = "";
        Subject = "";
        Description = "";
        TransferActive = false;
        SelectedFiles = [];
        IsValidTransferState = false;
    }



    [RelayCommand]
    public void CancelTransfer()
    {
        TransferCancellationToken.Cancel();
    }

    public void AddFile(CreateTransferFile file)
    {
        SelectedFiles.Add(file);
        ValidateTransfer();
    }
    
    public async Task SendTransfer(CancellationToken cancellationToken)
    {
        TransferActive = true;

        activeTransfer = await FsServer.CreateTransfer(Recipient.Replace(',',' ').Split(" "),
            Subject,
            Description,
            SelectedFiles.ToArray());

        var cidDictionary = SelectedFiles.ToDictionary(x => x.FileId, x => x.FileStream);
        await FsServer.SendTransfer(cidDictionary,activeTransfer,cancellationToken);
        TransferActive = false;
        return;
    }
    
    [RelayCommand]
    public void ValidateTransfer()
    {
        IsValidTransferState = SelectedFiles.Count > 0 && ValidateRecipientFiled();
        return;
    }

    private EmailAddressAttribute emailTool =  new EmailAddressAttribute();
    private bool ValidateRecipientFiled()
    {
        return Recipient.Replace(',',' ').Split(" ").All(x => emailTool.IsValid(x));
    }
}