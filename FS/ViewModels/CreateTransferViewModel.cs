
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
    private string password;
    
    [ObservableProperty]
    private bool isValidTransferState;

    [ObservableProperty]
    private bool isInvalidPassword;
    
    [ObservableProperty]
    private bool isInvalidEmails;
    
    [ObservableProperty] 
    private DateTime transferExpiryDate;
    
    [ObservableProperty] 
    private DateTime transferMinExpiryDate;
    
    [ObservableProperty] 
    private DateTime transferMaxExpiryDate;

    [ObservableProperty] 
    public bool encryptionEnabled;
    
    private Transfer? activeTransfer;
    public CancellationTokenSource TransferCancellationToken = new CancellationTokenSource();
    public CreateTransferViewModel(FileSenderServer fsServer)
    {
        FsServer = fsServer;
        EncryptionEnabled = fsServer.config.EncryptionOptions is null;
        Password = "";
        Recipient = "";
        Subject = "";
        Description = "";
        TransferActive = false;
        SelectedFiles = [];
        IsValidTransferState = false;
        TransferMaxExpiryDate = DateTime.Today + TimeSpan.FromDays(fsServer.config.MaxTransferDaysValid);
        TransferMinExpiryDate = DateTime.Now;
        TransferExpiryDate = DateTime.Now + TimeSpan.FromDays(fsServer.config.DefaultTransferDaysValid);
        IsInvalidEmails = false;
        IsInvalidPassword = false;
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
        var transferOptions = new List<TransferOptions>();
        activeTransfer = await FsServer.CreateTransfer(Recipient.Replace(',',' ').Split(" "),
            Subject,
            Description,
            password,
            transferOptions,
            SelectedFiles.ToArray());

        var cidDictionary = SelectedFiles.ToDictionary(x => x.FileId, x => x.FileStream);
        await FsServer.SendTransfer(cidDictionary,activeTransfer,cancellationToken);
        TransferActive = false;
        return;
    }
    
    [RelayCommand]
    public void ValidateTransfer()
    {
        IsInvalidPassword = !ValidatePassword();
        IsInvalidEmails = !ValidateRecipientFiled();
        IsValidTransferState = SelectedFiles.Count > 0 &&  !IsInvalidEmails && !IsInvalidPassword;
        return;
    }

    public bool ValidatePassword()
    {
        if(Password.Length == 0 || FsServer.config.EncryptionOptions is null)
        {
            return true;
        }
        
        return !((FsServer.config.EncryptionOptions.PasswordNumbersRequired && !Password.Any(char.IsNumber)) ||
                 (FsServer.config.EncryptionOptions.PasswordSpecialRequired &&
                    !Password.Any(c => !char.IsNumber(c) && !char.IsLetter(c))) ||
                 (FsServer.config.EncryptionOptions.PasswordMixedCaseRequired 
                    && !(Password.Any(char.IsUpper) && Password.Any(char.IsLower))) ||
                 (Password.Length < FsServer.config.EncryptionOptions.PasswordMinLength));
    }

    private EmailAddressAttribute emailTool =  new EmailAddressAttribute();
    private bool ValidateRecipientFiled()
    {
        return Recipient.Trim().Replace(',',' ').Split(" ").All(x => emailTool.IsValid(x));
    }
}