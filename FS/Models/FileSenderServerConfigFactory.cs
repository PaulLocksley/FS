using System.Text.RegularExpressions;

namespace FS.Models;

public static class FileSenderServerConfigFactory
{
    //I know, factory...
    //but async construction is gross.
    public static async Task<FileSenderServerConfig> BuildConfig()
    {
        var baseUrl = await SecureStorage.Default.GetAsync("base_url") ?? throw new InvalidOperationException();
        var username = await SecureStorage.Default.GetAsync("username") ?? throw new InvalidOperationException();
        var apikey = await SecureStorage.Default.GetAsync("apikey") ?? throw new InvalidOperationException();
        var defaultTransferDaysValid =
            int.Parse(await SecureStorage.Default.GetAsync("default_transfer_days_valid") ?? "10");

        var handler = new HttpClientHandler();
#if DEBUG
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
#endif
        var client = new HttpClient(handler);
        var configResponse = await client.GetAsync(new Uri($"{baseUrl[..^8]}filesender-config.js.php"));
        var configText = await configResponse.Content.ReadAsStringAsync();

        Regex regex = new Regex(@"terasender_worker_count\D*(\d+)");
        Match match = regex.Match(configText);
        int workerCount = match.Success ? int.Parse(match.Groups[1].Value) : 1;

        regex = new Regex(@"terasender_worker_max_chunk_retries\D*(\d+)");
        match = regex.Match(configText);
        int workerRetries = match.Success ? int.Parse(match.Groups[1].Value) : 20;

        regex = new Regex(@"terasender_enabled\W*(\w+)");
        match = regex.Match(configText);
        if (match.Success && match.Groups[1].Value == "false")
        {
            workerCount = 1;
        }

        regex = new Regex(@"max_transfer_files\D*(\d+)");
        match = regex.Match(configText);
        int maxFilesCount = match.Success ? int.Parse(match.Groups[1].Value) : 1000;

        regex = new Regex(@"max_transfer_size\D*(\d+)");
        match = regex.Match(configText);
        long maxTransferSize = match.Success ? long.Parse(match.Groups[1].Value) : 107374182400;

        regex = new Regex(@"site_name:\s*'(\S+)''");
        match = regex.Match(configText);
        string siteName = match.Success ? match.Groups[1].Value : "FileSender";

        regex = new Regex(@"upload_chunk_size\D*(\d+)");
        match = regex.Match(configText);
        int chunkSize = match.Success
            ? int.Parse(match.Groups[1].Value)
            : throw new InvalidDataException("No chunk size provided");

        var bannedFileTypes = new List<string>();
        regex = new Regex(@"ban_extension:\s*'(\S+)',");
        match = regex.Match(configText);
        if (match.Success)
        {
            foreach (var extension in match.Groups[1].Value.Split(","))
            {
                bannedFileTypes.Add(extension);
            }
        }

        return new FileSenderServerConfig(baseUrl, username, apikey, chunkSize, siteName, defaultTransferDaysValid,
            workerCount, workerRetries, maxFilesCount, maxTransferSize);
    }
}