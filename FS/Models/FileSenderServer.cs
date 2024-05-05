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

    public async Task testTransfer((String MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize)[] files)
    {
        Debug.WriteLine($"starting test transfer");
        if (files.Length == 0)
        {
            return;
        }
        var v = await Call(HttpVerb.Get,"/transfer", new Dictionary<string, string>(),null,null, new Dictionary<string, string>());
        var boddy = await v.Content.ReadAsStringAsync();
        Console.WriteLine(boddy);
        Console.WriteLine("Hello, World!");
        var tc = new Dictionary<string, object>();
        tc["from"] = "john@locksley.dev";
        
        var backTrace = new Dictionary<string, (string MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize)>();
        var ilegalChars = Path.GetInvalidPathChars();
        
        var filesObject = new List<Dictionary<string,string>>();
        foreach (var f_info in files)
        {
            var tmpFile = new Dictionary<string, string>();
            tmpFile["name"] = CleanFileName(f_info.FileName);
            tmpFile["size"] = f_info.FileSize.ToString();
            tmpFile["mime_type"] = f_info.MimeType;

            tmpFile["cid"] = Guid.NewGuid().ToString();
            backTrace[tmpFile["cid"]] = f_info;
            filesObject.Add(tmpFile);
        }
        var to = new List<String>();
        to.Add("paul@locksley.dev");

        tc["files"] = filesObject.ToArray();//;
        tc["recipients"] = to.ToArray();
        tc["subject"] = "Memes and shit";
        tc["message"] = "new and old memes, but only good";
        tc["expires"] = DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeSeconds();
        tc["aup_checked"] = 1;
        var k = new Dictionary<string, int>();
        k["get_a_link"] = 0;
        tc["options"] = k;

        var o = await Call(HttpVerb.Post, "/transfer", new Dictionary<string, string>(), tc, null,
            new Dictionary<string, string>());
        Debug.WriteLine($"Sent transfer request");
        o.EnsureSuccessStatusCode();
        var kk = await o.Content.ReadAsStringAsync();
        Debug.WriteLine($"Transfer response {kk}");

        var za =  JsonSerializer.Deserialize<Transfer>(kk);
        if (za is null)
        {
            throw new ApplicationException("Server failed to validate transfer");
        }
        Debug.WriteLine($"Transfer obj {za}");

        _countdown = new CountdownEvent(za.Files.Aggregate(0,(i,f) => i + (int)Math.Ceiling(f.Size / (decimal)config.ChunkSize)));
        _initialCount = za.Files.Aggregate(0, (i, f) => i + (int)Math.Ceiling(f.Size / (decimal)config.ChunkSize));
        _currentCount = _initialCount;

        foreach (var f in za.Files)
        {
            for (long i = 0; i < f.Size; i += config.ChunkSize)
            {
                await _semaphore.WaitAsync();
                ThreadPool.QueueUserWorkItem(SendChunk,
                    new object[] { backTrace[f.Cid!].FileStream, i, f, za.Roundtriptoken });
            }
        }
        Console.WriteLine(_countdown.InitialCount);
        Console.WriteLine(_countdown.CurrentCount);
        _countdown.Wait();
        foreach (var f in za.Files){
            var ppac = await Call(HttpVerb.Put,
                $"/file/{f.Id}",
                new Dictionary<string, string>
                {
                    { "key", f.Uid.ToString() },
                    { "roundtriptoken", za.Roundtriptoken }
                },
                new Dictionary<string, object> { { "complete", true } },
                null, new Dictionary<string, string>());
            ppac.EnsureSuccessStatusCode();
        }

        var jac = await Call(HttpVerb.Put,
            $"/transfer/{za.Id}",
            new Dictionary<string, string>
            {
                { "key", za.Files[0].Uid.ToString() },
            },
            new Dictionary<string, object> { { "complete", true } },
            null, new Dictionary<string, string>());
        var kkpp = await jac.Content.ReadAsStringAsync();

        Console.WriteLine(kkpp);
        Console.WriteLine("Aaa");
    }

    public static string CleanFileName(string name)
    {
        return Regex.Replace(name,@"[^ \/\p{L}\p{N}_\.,;:!@#$%^&*)(\]\[_-]+","");
    }

    private async void SendChunk(object state)
    {
        // Wait for semaphore
        //await semaphore.WaitAsync();

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
        return l;
    }
}