using System.Text.RegularExpressions;
namespace NLOIQ.Infrastructure.Media;
public sealed partial class MediaFileStore(string root)
{
    private readonly string basePath=Path.GetFullPath(root);
    [GeneratedRegex(@"\A[a-f0-9]{32}/main\.webp\z")]
    private static partial Regex LocalKey();
    private string Folder(string key) {
        if(!LocalKey().IsMatch(key))throw new InvalidOperationException("Invalid media storage key.");
        var path=Path.GetFullPath(Path.Combine(basePath,key[..32]));
        if(!path.StartsWith(basePath+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Media path is outside storage.");return path;
    }
    public async Task WriteAsync(string key,IReadOnlyDictionary<string,byte[]> files,CancellationToken ct)
    {
        var folder=Folder(key);Directory.CreateDirectory(folder);
        foreach(var (name,bytes) in files){if(name is not ("main.webp" or "thumb.webp" or "640.webp" or "1280.webp" or "icon.png"))throw new InvalidOperationException("Invalid media variant.");await File.WriteAllBytesAsync(Path.Combine(folder,name),bytes,ct);}
    }
    public string? Find(string key,string size) {
        var name=size switch{"full"=>"main.webp","thumb"=>"thumb.webp","640"=>"640.webp","1280"=>"1280.webp","icon"=>"icon.png",_=>null};if(name is null)return null;
        var path=Path.Combine(Folder(key),name);return File.Exists(path)?path:null;
    }
    public void Delete(string key){var folder=Folder(key);if(Directory.Exists(folder))Directory.Delete(folder,true);}
}
