using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging.Abstractions;

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
            var extensions = match.Groups[1].Value.Split(",");
            foreach (var extension in extensions)
            {
                bannedFileTypes.Add(extension);
            }
        }
        ServerEncryptionOptions? encryptionDetails = new ServerEncryptionOptions();
        try
        {
            regex = new Regex(@"encryption_password_must_have_upper_and_lower_case: (\w+)");
            match = regex.Match(configText);
            bool passwordMixedCaseRequired = match.Success && match.Groups[1].Value == "true";
            encryptionDetails.PasswordMixedCaseRequired = passwordMixedCaseRequired;

            // Example regex pattern for password_numbers_required
            regex = new Regex(@"encryption_password_must_have_numbers: (\w+)");
            match = regex.Match(configText);
            bool passwordNumbersRequired = match.Success && match.Groups[1].Value == "true";
            encryptionDetails.PasswordNumbersRequired = passwordNumbersRequired;

            // Example regex pattern for password_special_required
            regex = new Regex(@"encryption_password_must_have_special_characters: (\w+)");
            match = regex.Match(configText);
            bool passwordSpecialRequired = match.Success && match.Groups[1].Value == "true";
            encryptionDetails.PasswordSpecialRequired = passwordSpecialRequired;

            // Example regex pattern for iv_len
            regex = new Regex(@"crypto_iv_len\D+(\d+)");
            match = regex.Match(configText);
            int ivLen = match.Success ? int.Parse(match.Groups[1].Value) : 0;
            encryptionDetails.IvLength = ivLen;

            // Example regex pattern for password_hash_iterations
            regex = new Regex(@"encryption_password_hash_iterations_new_files\D+(\d+)");
            match = regex.Match(configText);
            int passwordHashIterations = match.Success ? int.Parse(match.Groups[1].Value) : 0;
            encryptionDetails.PasswordHashIterations = passwordHashIterations;

            // Example regex pattern for crypt_type
            regex = new Regex(@"crypto_crypt_name: '(.+)'");
            match = regex.Match(configText);
            SupportedCryptTypes cryptType = match.Success
                ? (SupportedCryptTypes)Enum.Parse(typeof(SupportedCryptTypes), match.Groups[1].Value.Replace("-", ""))
                : default;
            encryptionDetails.CryptType = cryptType;

            // Example regex pattern for hash_name
            regex = new Regex(@"crypto_hash_name: '(.+)'");
            match = regex.Match(configText);
            SupportedHashTypes hashName = match.Success
                ? (SupportedHashTypes)Enum.Parse(typeof(SupportedHashTypes), match.Groups[1].Value.Replace("-", ""))
                : default;
            encryptionDetails.HashName = hashName;

            // Example regex pattern for upload_chunk_base64_mode
            regex = new Regex(@"encryption_encode_encrypted_chunks_in_base64_during_upload: (\w+)");
            match = regex.Match(configText);
            bool uploadChunkBase64Mode = match.Success && match.Groups[1].Value == "true";
            encryptionDetails.UploadChunkBase64Mode = uploadChunkBase64Mode;
            if(uploadChunkBase64Mode)
            {   //todo.
                throw (new NotImplementedException("UploadChunkBase64Mode is not implemented"));
            }
        }
        catch (Exception e)
        {
            encryptionDetails = null;
            Debug.WriteLine($"Failed to capture encryption details: \n{e}");
        }

        return new FileSenderServerConfig(baseUrl, username, apikey, chunkSize, siteName, defaultTransferDaysValid,
            workerCount, workerRetries, maxFilesCount, maxTransferSize,encryptionDetails,bannedFileTypes);
    }
}