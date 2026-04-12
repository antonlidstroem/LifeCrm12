namespace LifeCrm.Core.Entities;

public class NewsletterAttachment : TenantEntity
{
    public Guid NewsletterId      { get; set; }
    public string FileName        { get; set; } = string.Empty;
    public string ContentType     { get; set; } = string.Empty;
    public byte[] FileBytes       { get; set; } = Array.Empty<byte>();
    public long FileSizeBytes     { get; set; }
    public Newsletter? Newsletter { get; set; }
}
