using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using FS.Models;
using Debug = System.Diagnostics.Debug;

namespace FS;

public class FileSenderServer
{
    private SemaphoreSlim _semaphore;
    private CountdownEvent _countdown = new CountdownEvent(1);
    private int _initialCount = 0;
    private int _currentCount = 1;
    
    public FileSenderServerConfig config;

    public FileSenderServer(FileSenderServerConfig config)
    {
        this.config = config;
        _semaphore = new SemaphoreSlim(config.WorkerCount);
    }
    
 
    
    
    List<string> Flatten(IDictionary<string, string> data)
    {
        var sb = new List<String>();
        foreach (var e in data)
        {
            sb.Add($"{e.Key}={e.Value}");
        }
        sb.Sort();
        return sb; //sb.Aggregate(" ",(y,x) => $"{y}&{x}").ToString();
    }

    async Task<HttpResponseMessage> Call(HttpVerb method,string path, IDictionary<string,string> queryParams, IDictionary<string,object>? content, byte[]? rawContent, Dictionary<string,string> options,int tryCount = 0)
    {
        try
        {
            queryParams["remote_user"] = config.Username;
            queryParams["timestamp"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();



            var o = Flatten(queryParams);
            var qs = o.Count > 0 ? o.Count == 1 ? o[0] : o.Aggregate((x, y) => $"{x}&{y}") : "";
            Console.WriteLine(qs);
            List<Byte> signedBytes = Encoding.ASCII.GetBytes(
                $"{method.ToString().ToLower()}&{Regex.Replace(config.BaseUrl, "https?://", "")}{path}?{qs}").ToList();

            var content_type = options.ContainsKey("Content-Type") ? options["Content-Type"] : "application/json";
            Console.WriteLine($"Content type is {content_type}");
            byte[]? inputContent = null;

            if (content is not null && content_type == "application/json")
            {
                inputContent = JsonSerializer.SerializeToUtf8Bytes(content);
                var kkk = Encoding.UTF8.GetString(inputContent);
                //Console.WriteLine(kkk);

                signedBytes.Add(Convert.ToByte('&'));
                signedBytes.AddRange(inputContent);
            }
            else if (rawContent is not null)
            {
                inputContent = rawContent;
                signedBytes.Add(Convert.ToByte('&')); //this may be a problem
                signedBytes.AddRange(inputContent);
            }

            //Console.WriteLine(Encoding.ASCII.GetString(signedBytes.ToArray()));
            var binaryKey = Encoding.ASCII.GetBytes(config.Apikey); //this is fucked
            using (var bKeyKey = new HMACSHA1(binaryKey))
            {
                var hashBytes = bKeyKey.ComputeHash(signedBytes.ToArray());
                queryParams["signature"] = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            var handler = new HttpClientHandler();
            
            #if DEBUG
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            #endif

            var client = new HttpClient(handler);
            client.BaseAddress = new Uri(config.BaseUrl);
            //Console.WriteLine(Encoding.ASCII.GetString(signedBytes.ToArray()));
            //Console.WriteLine("a");

            var queryString = HttpUtility.ParseQueryString(string.Empty);

            foreach (var kvp in queryParams)
            {
                queryString[kvp.Key] = kvp.Value;
            }

            var url = $"{config.BaseUrl}{path}?{queryString}";

            client.DefaultRequestHeaders.Add("Accept", "application/json");
            //client.DefaultRequestHeaders.Add("Content-Type",content_type);
            var put_content = new ByteArrayContent(inputContent ?? []);
            put_content.Headers.Add("Content-Type", content_type);
            HttpResponseMessage? response = null;
            if (method == HttpVerb.Get)
            {
                return await client.GetAsync(url);
            }

            ;
            if (method == HttpVerb.Post)
            {
                //response = await client.PostAsync(url,new ByteArrayContent(inputContent ?? []));
                return await client.PostAsync(url, put_content);
            }

            if (method == HttpVerb.Put)
            {
                var request = await client.PutAsync(url, put_content);
                request.EnsureSuccessStatusCode();
                return request;
            }

            if (method == HttpVerb.Delete)
            {
                var request = await client.DeleteAsync(url);
                request.EnsureSuccessStatusCode();
                return request;
            }

            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            if (tryCount > 5)
            {
                throw new Exception($"""
                                                  Failed to send message more than limited tries.
                                                  Inner Exception: {e.Message}
                                                  """);
            }

            await Task.Delay(10000);
            return await Call(method, path, queryParams.Where(x => x.Key != "signature").ToDictionary(), content, rawContent, options, tryCount + 1);
        }
    }

    public async Task<Transfer> CreateTransfer(string[] recepients, string subject, string message,
        (String MimeType, Task<Stream> FileStream, String FullPath, String FileName, long FileSize, string fileID)[] files)
    {
        
        Debug.WriteLine($"starting test transfer");
        if (files.Length == 0)
        {
            throw new InvalidDataException("Files list is empty, cannot create transfer");
        }

        var transferContent = new Dictionary<string, object>();
        transferContent["from"] = config.Username;
        
        var backTrace = new Dictionary<string, (string MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize)>();
        
        var filesObject = new List<Dictionary<string,string>>();
        foreach (var f_info in files)
        {
            var tmpFile = new Dictionary<string, string>();
            tmpFile["name"] = CleanFileName(f_info.FileName);
            tmpFile["size"] = f_info.FileSize.ToString();
            tmpFile["mime_type"] = f_info.MimeType;

            tmpFile["cid"] = f_info.fileID;
            filesObject.Add(tmpFile);
        }

        transferContent["files"] = filesObject.ToArray();//;
        transferContent["recipients"] = recepients;
        transferContent["subject"] = subject;
        transferContent["message"] = message;
        transferContent["expires"] = DateTimeOffset.UtcNow.AddDays(config.DefaultTransferDaysValid).ToUnixTimeSeconds();
        transferContent["aup_checked"] = 1;
        
        var optionsDict = new Dictionary<string, int>
        {
            ["get_a_link"] = 0
        };
        transferContent["options"] = optionsDict;

        var createTransferCall = await Call(HttpVerb.Post, "/transfer", new Dictionary<string, string>(), transferContent, null,
            new Dictionary<string, string>());
        Debug.WriteLine($"Sent transfer request");
        createTransferCall.EnsureSuccessStatusCode();
        var transferResponseText = await createTransferCall.Content.ReadAsStringAsync();
        Debug.WriteLine($"Transfer response {transferResponseText}");
        
        var transferResponse = JsonSerializer.Deserialize<Transfer>(transferResponseText);
        if (transferResponse is null)
        {
            throw new ApplicationException("Server failed to validate transfer");
        }
        Debug.WriteLine($"Transfer obj {transferResponse}");
        return transferResponse;
    }
    public async Task SendTransfer(IDictionary<string,Task<Stream>>files, Transfer transferResponse, CancellationToken cancellationToken)
    {
        cancellationToken.Register(() =>
        {
            DeleteTransfer(transferResponse);//todo, think about this.
        });

        try
        {
            _countdown = new CountdownEvent(transferResponse.Files.Aggregate(0,
                (i, f) => i + (int)Math.Ceiling(f.Size / (decimal)config.ChunkSize)));
            _initialCount = transferResponse.Files.Aggregate(0,
                (i, f) => i + (int)Math.Ceiling(f.Size / (decimal)config.ChunkSize));
            _currentCount = _initialCount;

            foreach (var f in transferResponse.Files)
            {
                for (long i = 0; i < f.Size; i += config.ChunkSize)
                {
                    await _semaphore.WaitAsync();
                    ThreadPool.QueueUserWorkItem(SendChunk,
                        new object[] { files[f.Cid], i, f, transferResponse.Roundtriptoken });
                }
            }

            Console.WriteLine(_countdown.InitialCount);
            Console.WriteLine(_countdown.CurrentCount);
            _countdown.Wait();
            foreach (var f in transferResponse.Files)
            {
                var fileCompleteCall = await Call(HttpVerb.Put,
                    $"/file/{f.Id}",
                    new Dictionary<string, string>
                    {
                        { "key", f.Uid.ToString() },
                        { "roundtriptoken", transferResponse.Roundtriptoken }
                    },
                    new Dictionary<string, object> { { "complete", true } },
                    null, new Dictionary<string, string>());
                fileCompleteCall.EnsureSuccessStatusCode();
            }

            var transferCompleteCall = await Call(HttpVerb.Put,
                $"/transfer/{transferResponse.Id}",
                new Dictionary<string, string>
                {
                    { "key", transferResponse.Files[0].Uid.ToString() },
                },
                new Dictionary<string, object> { { "complete", true } },
                null, new Dictionary<string, string>());
            transferCompleteCall.EnsureSuccessStatusCode();
            var transferCompleteText = await transferCompleteCall.Content.ReadAsStringAsync();
            Debug.WriteLine(transferCompleteText);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            DeleteTransfer(transferResponse);
        }
    }
    
    public async Task DeleteTransfer(Transfer transfer)
    {
        Debug.WriteLine("Starting delete");
        var p = await Call(HttpVerb.Delete, 
            $"/transfer/{transfer.Id}",
            new Dictionary<string, string>{{"key",transfer.Files.First().Uid.ToString()}},
            null,null,new Dictionary<string, string>()
        );
        Debug.WriteLine(await p.Content.ReadAsStringAsync());
        return;
    }
    
    public async Task<Transfer[]> GetAllTransfers()
    {
        var transfersRequest = await Call(HttpVerb.Get, "/transfer", new Dictionary<string, string>(), null, null,
            new Dictionary<string, string>());
        var responseText = await transfersRequest.Content.ReadAsStringAsync();
        var transfers = JsonSerializer.Deserialize<Transfer[]>(responseText);
        if (transfers is null)
        {
            throw new ApplicationException("Server failed to get transfers");
        }
        return transfers;
    }

    public static string CleanFileName(string name)
    {
        return Regex.Replace(name,@"[^ \/\p{L}\p{N}_\.,;:!@#$%^&*)(\]\[_-]+","");
    }

    private async void SendChunk(object state)
    {
        try
        {
            object[] parameters = (object[])state;
            Task<Stream> fileStream = (Task<Stream>)parameters[0];
            long offset = (long)parameters[1];
            TransferFile f = (TransferFile)parameters[2];
            string roundtriptoken = (string)parameters[3];

            var f_content = new byte[config.ChunkSize];

            lock (fileStream)
            {
                var readTask = fileStream.Result;
                readTask.Seek(offset, 0);
                var readSize = readTask.Read(f_content, 0, config.ChunkSize);

                if (readSize != config.ChunkSize)
                {
                    f_content = f_content.Take(readSize).ToArray();
                }
            }

            var t = await Call(HttpVerb.Put,
                $"/file/{f.Id}/chunk/{offset}",
                new Dictionary<string, string>
                {
                    { "key", f.Uid.ToString() },
                    { "roundtriptoken", roundtriptoken }
                },
                null,
                f_content,
                new Dictionary<string, string> { { "Content-Type", "application/octet-stream" } });
            t.EnsureSuccessStatusCode();
        }
        finally
        {
            // Release semaphore
            _semaphore.Release();
            Interlocked.Decrement(ref _currentCount);
            _countdown.Signal();
        }
    }



    public Double getProgressPercent()
    {
        var k = (double)_currentCount / (double)_initialCount - 1;
        var l = k * -1;
        return Math.Clamp(Math.Round(l,4),0,1);
    }
}