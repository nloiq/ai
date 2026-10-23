using NLOIQ.Application.Content;
using SkiaSharp;
namespace NLOIQ.Infrastructure.Media;
public sealed class ImageOptimizer:IImageOptimizer
{
    public const int MaxBytes=8*1024*1024;
    private static readonly SemaphoreSlim Slots=new(2,2);
    public async Task<OptimizedImage> OptimizeAsync(Stream source,string filename,string contentType,CancellationToken ct=default)
    {
        var extension=Path.GetExtension(filename).ToLowerInvariant();
        var expected=extension switch{".png"=>SKEncodedImageFormat.Png,".jpg" or ".jpeg"=>SKEncodedImageFormat.Jpeg,".webp"=>SKEncodedImageFormat.Webp,_=>throw new InvalidOperationException("Only PNG, JPEG and WebP images are accepted.")};
        if(contentType!=(expected switch{SKEncodedImageFormat.Png=>"image/png",SKEncodedImageFormat.Jpeg=>"image/jpeg",_=>"image/webp"}))throw new InvalidOperationException("File extension and MIME type must agree.");
        await Slots.WaitAsync(ct);
        try {
            using var input=new MemoryStream();var buffer=new byte[65536];int read;
            while((read=await source.ReadAsync(buffer,ct))>0){if(input.Length+read>MaxBytes)throw new InvalidOperationException("Image exceeds 8 MiB.");await input.WriteAsync(buffer.AsMemory(0,read),ct);}
            if(input.Length==0)throw new InvalidOperationException("Empty image.");
            using var data=SKData.CreateCopy(input.ToArray());using var codec=SKCodec.Create(data)??throw new InvalidOperationException("Image cannot be decoded.");
            if(codec.EncodedFormat!=expected||codec.FrameCount>1||codec.Info.Width<1||codec.Info.Height<1||codec.Info.Width>8000||codec.Info.Height>8000||(long)codec.Info.Width*codec.Info.Height>16000000)throw new InvalidOperationException("Image type, frame count or pixel dimensions are unsupported.");
            using var pixels=new SKBitmap(new SKImageInfo(codec.Info.Width,codec.Info.Height,SKColorType.Rgba8888,SKAlphaType.Premul));
            if(codec.GetPixels(pixels.Info,pixels.GetPixels())!=SKCodecResult.Success)throw new InvalidOperationException("Image is incomplete or corrupt.");
            var swap=codec.EncodedOrigin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
            using var oriented=new SKBitmap(swap?pixels.Height:pixels.Width,swap?pixels.Width:pixels.Height);
            using(var canvas=new SKCanvas(oriented)) {
                canvas.Clear(SKColors.Transparent);
                var w=pixels.Width;var h=pixels.Height;
                var matrix=codec.EncodedOrigin switch {
                    SKEncodedOrigin.TopRight=>new SKMatrix(-1,0,w,0,1,0,0,0,1),
                    SKEncodedOrigin.BottomRight=>new SKMatrix(-1,0,w,0,-1,h,0,0,1),
                    SKEncodedOrigin.BottomLeft=>new SKMatrix(1,0,0,0,-1,h,0,0,1),
                    SKEncodedOrigin.LeftTop=>new SKMatrix(0,1,0,1,0,0,0,0,1),
                    SKEncodedOrigin.RightTop=>new SKMatrix(0,-1,h,1,0,0,0,0,1),
                    SKEncodedOrigin.RightBottom=>new SKMatrix(0,-1,h,-1,0,w,0,0,1),
                    SKEncodedOrigin.LeftBottom=>new SKMatrix(0,1,0,-1,0,w,0,0,1),_=>SKMatrix.Identity};
                canvas.SetMatrix(matrix);canvas.DrawBitmap(pixels,0,0,new SKSamplingOptions(SKFilterMode.Nearest));
            }
            var scale=Math.Min(1,2400d/Math.Max(oriented.Width,oriented.Height));var width=Math.Max(1,(int)Math.Round(oriented.Width*scale));var height=Math.Max(1,(int)Math.Round(oriented.Height*scale));
            byte[] Encode(int targetWidth,string format){var targetHeight=Math.Max(1,(int)Math.Round((double)height*targetWidth/width));using var bitmap=new SKBitmap(targetWidth,targetHeight);using var canvas=new SKCanvas(bitmap);canvas.Clear(SKColors.Transparent);using var image=SKImage.FromBitmap(oriented);canvas.DrawImage(image,new SKRect(0,0,targetWidth,targetHeight),new SKSamplingOptions(SKFilterMode.Linear,SKMipmapMode.Linear));using var output=SKImage.FromBitmap(bitmap);using var encoded=output.Encode(format=="png"?SKEncodedImageFormat.Png:SKEncodedImageFormat.Webp,85);return encoded.ToArray();}
            var files=new Dictionary<string,byte[]>{{"main.webp",Encode(width,"webp")},{"thumb.webp",Encode(Math.Min(width,320),"webp")}};
            foreach(var size in new[]{640,1280})if(width>size)files[size+".webp"]=Encode(size,"webp");
            using(var icon=new SKBitmap(64,64)){using var canvas=new SKCanvas(icon);canvas.Clear(SKColors.Transparent);using var image=SKImage.FromBitmap(oriented);var ratio=Math.Min(64d/oriented.Width,64d/oriented.Height);var iw=(float)(oriented.Width*ratio);var ih=(float)(oriented.Height*ratio);canvas.DrawImage(image,new SKRect((64-iw)/2,(64-ih)/2,(64+iw)/2,(64+ih)/2),new SKSamplingOptions(SKFilterMode.Linear));using var output=SKImage.FromBitmap(icon);using var encoded=output.Encode(SKEncodedImageFormat.Png,100);files["icon.png"]=encoded.ToArray();}
            ct.ThrowIfCancellationRequested();return new(width,height,files);
        } finally {Slots.Release();}
    }
}
