namespace WSM.Media.Management.Models;

public class MediaFile
{
    public Guid Id { get; set; }
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public long? RelatedToId { get; set; }
    public MediaFileType Type { get; set; }
    public MediaTargetType TargetType { get; set; }
}
