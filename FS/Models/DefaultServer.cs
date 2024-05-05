namespace FS.Models;

public class DefaultServer(string name, Uri address)
{ 
    public string Name = name ;
    public Uri Address = address;
}