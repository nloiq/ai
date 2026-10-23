using SkiaSharp;
using NLOIQ.Application.Content;
using NLOIQ.Infrastructure.Media;
using NLOIQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using MySql.Data.MySqlClient;
static void Check(bool valid,string message){if(!valid)throw new Exception(message);}
byte[] Fixture(int width,int height,SKEncodedImageFormat format){using var bitmap=new SKBitmap(width,height);using var canvas=new SKCanvas(bitmap);canvas.Clear(new SKColor(20,39,31,128));using var paint=new SKPaint{Color=new SKColor(216,173,100)};canvas.DrawRect(0,0,width/2,height/2,paint);using var image=SKImage.FromBitmap(bitmap);using var data=image.Encode(format,95);return data.ToArray();}
var optimizer=new ImageOptimizer();
async Task Reject(byte[] bytes,string filename,string type){try{await optimizer.OptimizeAsync(new MemoryStream(bytes),filename,type);throw new Exception("Invalid image accepted.");}catch(InvalidOperationException){}}
var png=Fixture(3000,1500,SKEncodedImageFormat.Png);var result=await optimizer.OptimizeAsync(new MemoryStream(png),"wide.png","image/png");
Check(result.Width==2400&&result.Height==1200,"Resize limits failed.");
foreach(var name in new[]{"main.webp","thumb.webp","640.webp","1280.webp","icon.png"})Check(result.Files.ContainsKey(name),"Variant missing: "+name);
using(var decoded=SKBitmap.Decode(result.Files["icon.png"]))Check(decoded.Width==64&&decoded.Height==64,"Browser icon dimensions incorrect.");
using(var decoded=SKBitmap.Decode(result.Files["main.webp"]))Check(decoded.GetPixel(2200,1000).Alpha<255,"Transparent background was lost.");
Console.WriteLine("PASS: bounded resize, responsive WebP variants, PNG favicon and transparency");
var jpeg=Fixture(120,80,SKEncodedImageFormat.Jpeg);var exif=Convert.FromHexString("45786966000049492A0008000000010012010300010000000600000000000000");var oriented=new List<byte>{255,216,255,225,0,(byte)(exif.Length+2)};oriented.AddRange(exif);oriented.AddRange(jpeg[2..]);
var rotated=await optimizer.OptimizeAsync(new MemoryStream(oriented.ToArray()),"orientation.jpg","image/jpeg");Check(rotated.Width==80&&rotated.Height==120,"EXIF orientation not applied.");using(var codec=SKCodec.Create(new MemoryStream(rotated.Files["main.webp"])))Check(codec.EncodedOrigin==SKEncodedOrigin.TopLeft,"EXIF metadata was retained.");Console.WriteLine("PASS: EXIF orientation applied and metadata removed");
await Reject(png,"wrong.jpg","image/jpeg");await Reject(png,"wrong.png","image/jpeg");await Reject(System.Text.Encoding.UTF8.GetBytes("<svg onload='alert(1)'/>"),"bad.svg","image/svg+xml");await Reject(System.Text.Encoding.UTF8.GetBytes("<html>not an image</html>"),"bad.png","image/png");await Reject(png[..40],"truncated.png","image/png");await Reject(new byte[ImageOptimizer.MaxBytes+1],"large.png","image/png");await Reject(Fixture(8001,1,SKEncodedImageFormat.Png),"wide-limit.png","image/png");await Reject(Fixture(5000,3201,SKEncodedImageFormat.Png),"pixel-limit.png","image/png");
Console.WriteLine("PASS: file/MIME spoofing, active content, truncation and byte-limit rejection");
var small=await optimizer.OptimizeAsync(new MemoryStream(Fixture(64,32,SKEncodedImageFormat.Png)),"small.png","image/png");Check(small.Width==64&&small.Height==32&&!small.Files.ContainsKey("640.webp"),"Small image was upscaled.");
var store=new MediaFileStore(Path.Combine(Environment.CurrentDirectory,"artifacts","media-checks"));await store.WriteAsync(new string('a',32)+"/main.webp",small.Files,CancellationToken.None);Check(store.Find(new string('a',32)+"/main.webp","full") is not null,"Local storage write failed.");store.Delete(new string('a',32)+"/main.webp");try{store.Delete("../outside/main.webp");throw new Exception("Path escape accepted.");}catch(InvalidOperationException){}Console.WriteLine("PASS: no upscaling and storage path containment");
using var local=JsonDocument.Parse(File.ReadAllText("src/NLOIQ.Web/appsettings.Local.json"));var connection=local.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString()!;
if(!new MySqlConnectionStringBuilder(connection).Database.Equals("WebSiteNLOIQDb",StringComparison.OrdinalIgnoreCase))throw new Exception("Dedicated development database required.");
var options=new DbContextOptionsBuilder<NloDbContext>().UseMySQL(connection).Options;long assetId=0;
await using(var db=new NloDbContext(options)){
 var actor=await db.Users.Select(x=>x.Id).FirstAsync();var library=new MediaLibrary(db,optimizer,store,NullLogger<MediaLibrary>.Instance);
 try{
  assetId=await library.UploadAsync(new MemoryStream(Fixture(640,360,SKEncodedImageFormat.Png)),"phase8-check.png","image/png","Media check","اختبار الوسائط",actor,"Media integration check");
  Check(!await MediaAccess.IsPublicAsync(db,assetId),"Unattached upload was public.");var row=(await library.ListAsync("phase8-check.png")).Single(x=>x.Id==assetId);
  await library.UpdateAltAsync(assetId,row.Version,"Updated hint","نص بديل محدث",actor,"Media integration check");
  try{await library.UpdateAltAsync(assetId,row.Version,"Stale","قديم",actor,"Media integration check");throw new Exception("Stale alt accepted.");}catch(ContentConflictException){}
  Check((await library.ListAsync("phase8-check.png")).Single(x=>x.Id==assetId).AltEn=="Updated hint","Alt did not persist.");
  var key=(await db.MediaAssets.SingleAsync(x=>x.Id==assetId)).StorageKey;await library.DeleteAsync(assetId,actor,"Media integration check");assetId=0;Check(store.Find(key,"full") is null,"Deleted upload file remained.");
  Check(await db.AuditLogs.AnyAsync(x=>x.Action=="UploadMedia"&&x.ActorUserId==actor),"Upload audit missing.");
 }finally{if(assetId!=0)await library.DeleteAsync(assetId,actor,"Media integration check cleanup");}
}
Console.WriteLine("PASS: real media upload, private storage, persisted hints, stale-form rejection, deletion and audit history");
