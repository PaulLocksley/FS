using System.Text.RegularExpressions;

namespace FS.Models;

public class FileSenderServerConfig(
    string baseUrl,
    string username,
    string apikey,
    int chunkSize,
    string siteName,
    int defaultTransferDaysValid,
    int maxTransferDaysValid,
    int workerCount,
    int workerRetries,
    int maxFilesCount,
    long maxTransferSize,
    ServerEncryptionOptions? encryptionOptions,
    List<string> bannedFileTypes)
{
    public string BaseUrl = baseUrl;
    public string Username = username;
    public string Apikey = apikey;
    public int ChunkSize = chunkSize;
    public string SiteName = siteName;
    public int DefaultTransferDaysValid = defaultTransferDaysValid;
    public int MaxTransferDaysValid = maxTransferDaysValid;
    public int WorkerCount = workerCount;
    public int WorkerRetries = workerRetries;
    public int MaxFilesCount = maxFilesCount;
    public long MaxTransferSize = maxTransferSize;
    public IList<string> BannedFileTypes = bannedFileTypes;
    public ServerEncryptionOptions? EncryptionOptions = encryptionOptions;
}