using Microsoft.EntityFrameworkCore;
using WSM.Media.Management.Db;
using WSM.Media.Management.Models;

namespace WSM.Media.Management.Media;

public class FileManager(IMediaDbContext dbContext)
{
    public static string GlobalUrl = "/media/{fileName}";
    public static string GlobalStreamUrl = "/media/{fileName}?stream=true";
    public static string GlobalThumbUrl = "/media/{fileName}?thumb=true";
    public async Task<string> GetUrlAsync(Guid? mediaId)
    {
        var mediaFile = await dbContext.MediaFiles.FirstOrDefaultAsync(e => e.Id == mediaId);
        return GlobalUrl.Replace("{fileName}", mediaFile?.FileName ?? "not-found.png");
    }
    public async Task<string> GetStreamUrlAsync(Guid? mediaId)
    {
        var mediaFile = await dbContext.MediaFiles.FirstOrDefaultAsync(e => e.Id == mediaId);
        return GlobalStreamUrl.Replace("{fileName}", mediaFile?.FileName ?? "not-found.png");
    }
    public async Task<string> GetThumbUrlAsync(Guid? mediaId)
    {
        var mediaFile = await dbContext.MediaFiles.FirstOrDefaultAsync(e => e.Id == mediaId);
        return GlobalThumbUrl.Replace("{fileName}", mediaFile?.FileName ?? "not-found.png");
    }
    public string GetUrl(Guid? mediaId)
    {
        var mediaFile = dbContext.MediaFiles.FirstOrDefault(e => e.Id == mediaId);
        return GlobalUrl.Replace("{fileName}", mediaFile?.FileName ?? "not-found.png");
    }
    public string GetStreamUrl(Guid? mediaId)
    {
        var mediaFile = dbContext.MediaFiles.FirstOrDefault(e => e.Id == mediaId);
        return GlobalStreamUrl.Replace("{fileName}", mediaFile?.FileName ?? "not-found.png");
    }
    public string GetThumbUrl(Guid? mediaId)
    {
        var mediaFile = dbContext.MediaFiles.FirstOrDefault(e => e.Id == mediaId);
        return GlobalThumbUrl.Replace("{fileName}", mediaFile?.FileName ?? "not-found.png");
    }
    public async Task<Guid?> UploadFileAsync<T>(FileRequest fileRequest)
    {
        var dirtyMediaFile = false;
        var entityName = typeof(T).Name;
        var state = GetRequestState(fileRequest);
        if (state == FileRequest.RequestState.Nothing)
            return fileRequest?.FileId;
        MediaFile? mediaFile = null!;
        if (state is FileRequest.RequestState.Delete or FileRequest.RequestState.Edit)
            mediaFile = await dbContext.MediaFiles.FirstOrDefaultAsync(e => e.Id == fileRequest.FileId);
        mediaFile ??= NewMediaFile(fileRequest, entityName);

        if (state == FileRequest.RequestState.Delete)
        {
            dirtyMediaFile = true;
            dbContext.MediaFiles.Remove(mediaFile);
        }
        else if (state == FileRequest.RequestState.Edit)
        {
            await File.WriteAllBytesAsync(mediaFile.FilePath, fileRequest.Bytes);
        }
        else if (state == FileRequest.RequestState.Add)
        {
            await dbContext.MediaFiles.AddAsync(mediaFile);
            await File.WriteAllBytesAsync(mediaFile.FilePath, fileRequest.Bytes);
        }

        if (dirtyMediaFile) await dbContext.SaveChangesAsync();

        return mediaFile.Id;
    }
    private MediaFile NewMediaFile(FileRequest fileRequest, string entityName)
    {
        var fileId = Guid.NewGuid();
        var extension = "." + fileRequest.Extension.TrimStart('.');
        var fileName = "media-file-" + fileId + extension;
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Media", entityName);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        var filePath = Path.Combine(folderPath, fileName);
        return new MediaFile()
        {
            Id = fileId,
            FileName = fileName,
            FilePath = filePath,
            Type = fileRequest.Type
        };
    }

    private static FileRequest.RequestState GetRequestState(FileRequest request)
    {
        if (request == null)
            return FileRequest.RequestState.Nothing;
        if (request.Delete && request.FileId.HasValue)
            return FileRequest.RequestState.Delete;
        if ((request.Bytes?.Count() ?? 0) == 0)
            return FileRequest.RequestState.Nothing;
        if (request.FileId != null && request.FileId.HasValue)
            return FileRequest.RequestState.Edit;
        return FileRequest.RequestState.Add;
    }
}
public interface IMediaFile
{
    long? Id { get; set; }
    MediaFile SetMediaFile(MediaFile file);
}
public static class FileRequestExtensions
{
}