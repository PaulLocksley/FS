using CommunityToolkit.Mvvm.ComponentModel;

namespace FS.Models;

public class CreateTransferFile : IEquatable<CreateTransferFile>
{
    public string MimeType { get; set; }
    public Task<Stream> FileStream { get; set; }
    public string FullPath{ get; }

    public string FileName { get; set; }
    public long FileSize { get; set; }
    public string FileId { get; set; }
            
    public byte[]? FileIV { get; set; }
    public string? FileAead { get; set; }

    public CreateTransferFile(string mimeType, Task<Stream> fileStream, string fullPath, string fName, long fileSize, string fileId)
    {
        MimeType = mimeType;
        FileStream = fileStream;
        FullPath = fullPath;
        FileName = fName;
        FileSize = fileSize;
        FileId = fileId;
        FileIV = null;
        FileAead = null;
    }


    public bool Equals(CreateTransferFile? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return FullPath == other.FullPath;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((CreateTransferFile)obj);
    }

    public override int GetHashCode()
    {
        return FullPath.GetHashCode();
    }
}