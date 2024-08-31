namespace FS.Models;

public class ServerEncryptionOptions
{
    public bool PasswordMixedCaseRequired { get; set; }
    public bool PasswordNumbersRequired { get; set; }
    public bool PasswordSpecialRequired { get; set; }
    public int IvLength { get; set; }
    public int PasswordHashIterations { get; set; }
    public SupportedCryptTypes CryptType { get; set; }
    public SupportedHashTypes HashName { get; set; }
    public bool UploadChunkBase64Mode { get; set; }
}