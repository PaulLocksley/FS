namespace FS.Utilities;

public static class FileSize
{

    public static string getHumanFileSize(long fileSize)
    {

// Array of suffixes for file sizes
        string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

// Starting with bytes
        int suffixIndex = 0;

// Convert the file size to a human-readable format
        while (fileSize >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            fileSize /= 1024;
            suffixIndex++;
        }

        return $"{fileSize} {suffixes[suffixIndex]}";
    }
}