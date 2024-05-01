using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using FS.Models;

namespace FS;

public class FileSenderServer
{
    private string baseUrl = "https://192.168.20.138/filesender/rest.php";
    private string username = "john@locksley.dev";
    private string apikey = "6822185aa4ad908bf9c7da9dcabdb57d0d7cb71edbf87d3da4a8a7c936c10f29";
    private bool insecure = true;
    private int ChunkSize = 5242880;

    
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

    async Task<HttpResponseMessage> Call(HttpVerb method,string path, IDictionary<string,string> queryParams, IDictionary<string,object>? content, byte[]? rawContent, Dictionary<string,string> options)
    {
        
        queryParams["remote_user"] = username;
        queryParams["timestamp"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();



        var o = Flatten(queryParams);
        var qs = o.Count > 0 ? o.Count == 1 ? o[0] : o.Aggregate((x, y) => $"{x}&{y}") : "";
        Console.WriteLine(qs);
        List<Byte> signedBytes = Encoding.ASCII.GetBytes(
            $"{method.ToString().ToLower()}&{Regex.Replace(baseUrl,"https?://","")}{path}?{qs}").ToList();

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
        }else if (rawContent is not null)
        {
            inputContent = rawContent;
            signedBytes.Add(Convert.ToByte('&')); //this may be a problem
            signedBytes.AddRange(inputContent);
        }
        //Console.WriteLine(Encoding.ASCII.GetString(signedBytes.ToArray()));
        var binaryKey = Encoding.ASCII.GetBytes(apikey); //this is fucked
        using(var bKeyKey = new HMACSHA1(binaryKey))
        {
            var hashBytes = bKeyKey.ComputeHash(signedBytes.ToArray());
            queryParams["signature"] = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        var handler = new HttpClientHandler(); 
        if(insecure)
        {
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator; 
        }
        
        var client = new HttpClient(handler);
        client.BaseAddress = new Uri(baseUrl);
        //Console.WriteLine(Encoding.ASCII.GetString(signedBytes.ToArray()));
        //Console.WriteLine("a");
        
        var queryString = HttpUtility.ParseQueryString(string.Empty);
        
        foreach (var kvp in queryParams)
        {
            queryString[kvp.Key] = kvp.Value;
        } 
        
        var url = $"{baseUrl}{path}?{queryString}";
        
        client.DefaultRequestHeaders.Add("Accept","application/json");
        //client.DefaultRequestHeaders.Add("Content-Type",content_type);
        var put_content = new ByteArrayContent(inputContent ?? []);
        put_content.Headers.Add("Content-Type",content_type);
        HttpResponseMessage? response = null;
        if(method == HttpVerb.Get)
        {
            return await client.GetAsync(url);
        };
        if (method == HttpVerb.Post)
        {
            //response = await client.PostAsync(url,new ByteArrayContent(inputContent ?? []));
            return await client.PostAsync(url,put_content);
        }

        if (method == HttpVerb.Put)
        {
            var request = await client.PutAsync(url,put_content);
            request.EnsureSuccessStatusCode();
            return request;
        }
        throw new NotImplementedException();
    }

    public async void testTransfer((String MimeType,Task<Stream> FileStream, String FullPath, String FileName, long FileSize)[] files)
    {
        Debug.WriteLine($"starting test transfer");

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


        foreach (var f in za.Files)
        {
            var f_path = backTrace[f.Cid].FullPath;
            var f_size = backTrace[f.Cid].FileSize;
            var tasks = new List<Task>();
            for (long i = 0; i < f_size; i += ChunkSize)
            {
                Debug.WriteLine($"working on {i}");

                var f_content = new byte[ChunkSize];
                var readTask = backTrace[f.Cid].FileStream.Result;
                var readSize = readTask.Read(f_content, 0, ChunkSize);

                if (readSize != ChunkSize)
                {
                    f_content = f_content.Take(readSize).ToArray();
                }

                var t = Call(HttpVerb.Put,
                    $"/file/{f.Id}/chunk/{i}",
                    new Dictionary<string, string>
                    {
                        { "key", f.Uid.ToString() },
                        { "roundtriptoken", za.Roundtriptoken }
                    },
                    null,
                    f_content,
                    new Dictionary<string, string> { { "Content-Type", "application/octet-stream" } });
                Debug.WriteLine($"task status for {i} {t.Status}");
                tasks.Add(t);
                //var kkp = await pac.Content.ReadAsStringAsync();
                //Console.WriteLine(kkp);
                Debug.WriteLine($"reading {i}");
            }

            Debug.WriteLine("Waiting for tasks");
            await Task.WhenAll(tasks);
            Debug.WriteLine("Done with tasks");
            var ppac = await Call(HttpVerb.Put,
                $"/file/{f.Id}",
                new Dictionary<string, string>
                {
                    { "key", f.Uid.ToString() },
                    { "roundtriptoken", za.Roundtriptoken }
                },
                new Dictionary<string, object> { { "complete", true } },
                null, new Dictionary<string, string>());
            var kkkpppp = await ppac.Content.ReadAsStringAsync();
            Console.WriteLine(kkkpppp);
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
}