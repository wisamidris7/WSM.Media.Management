using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WSM.Media.Management.Db;

namespace WSM.Media.Management.Media;

public class MediaMiddleware : IMiddleware
{
    private readonly IMediaDbContext _dbContext;

    public MediaMiddleware(IMediaDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var regex = new Regex(@"^/media/(?<fileName>[^/]+)$", RegexOptions.IgnoreCase);
        var match = regex.Match(context.Request.Path.Value);

        var streamEnabled = context.Request.Query
            .FirstOrDefault(e => e.Key.Equals("stream", StringComparison.OrdinalIgnoreCase))
            .Value
            .FirstOrDefault();
        var isStreamRequest = streamEnabled?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

        var thumbEnabled = context.Request.Query
            .FirstOrDefault(e => e.Key.Equals("thumb", StringComparison.OrdinalIgnoreCase))
            .Value
            .FirstOrDefault();
        var isThumbRequest = thumbEnabled?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

        if (match.Success)
        {
            string fileName = match.Groups["fileName"].Value;

            var mediaFile = await _dbContext.MediaFiles
                .Where(e => e.FileName == fileName)
                .FirstOrDefaultAsync();

            if (mediaFile != null)
            {
                var filePath = mediaFile.FilePath;

                var contentType = GetContentType(filePath);

                if (isStreamRequest)
                {
                    var fileInfo = new FileInfo(filePath);
                    context.Response.ContentType = contentType;
                    context.Response.Headers["Accept-Ranges"] = "bytes";

                    var rangeHeader = context.Request.Headers["Range"].ToString();
                    if (rangeHeader != null && rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
                    {
                        var range = rangeHeader.Substring("bytes=".Length);
                        var rangeParts = range.Split('-');
                        var start = long.Parse(rangeParts[0]);
                        var end = rangeParts.Length > 1 ? long.Parse(rangeParts[1]) : fileInfo.Length - 1;

                        context.Response.StatusCode = StatusCodes.Status206PartialContent;
                        context.Response.Headers["Content-Range"] = $"bytes {start}-{end}/{fileInfo.Length}";
                        context.Response.Headers["Content-Length"] = (end - start + 1).ToString();

                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            fileStream.Position = start;
                            await fileStream.CopyToAsync(context.Response.Body);
                        }
                    }
                    else
                    {
                        context.Response.Headers["Content-Length"] = fileInfo.Length.ToString();
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            await fileStream.CopyToAsync(context.Response.Body);
                        }
                    }
                }
                else if (isThumbRequest && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    var maxSize = 350;
                    var thumbnailStream = await GenerateThumbnailAsync(filePath, maxSize);
                    context.Response.ContentType = contentType;
                    await thumbnailStream.CopyToAsync(context.Response.Body);
                }
                else
                {
                    context.Response.Headers["Cache-Control"] = "public, max-age=604800"; // Cache for 7 days
                    context.Response.ContentType = contentType;
                    await context.Response.SendFileAsync(filePath);
                }

                return;
            }
        }

        await next(context);
    }

    private string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".ogg" => "video/ogg",
            ".webp" => "image/webp",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
    }

    private async Task<Stream> GenerateThumbnailAsync(string filePath, int maxSize)
    {
        using (var image = Image.Load(filePath))
        {
            var width = image.Width;
            var height = image.Height;

            if (width > height)
            {
                height = (int)(height * (maxSize / (float)width));
                width = maxSize;
            }
            else
            {
                width = (int)(width * (maxSize / (float)height));
                height = maxSize;
            }

            image.Mutate(x => x.Resize(width, height));

            var stream = new MemoryStream();
            await image.SaveAsJpegAsync(stream);
            stream.Position = 0;
            return stream;
        }
    }

}
