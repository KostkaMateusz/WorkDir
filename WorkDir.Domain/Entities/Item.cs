namespace WorkDir.Domain.Entities;

public class Item
{
    public Guid Id { get; set; }
    public Guid? ParentFolderId { get; set; }
    public required string FileName { get; set; }
    public bool IsDirectory { get; set; }
    public User User { get; set; }
    public Guid OwnerId { get; set; }
    public IList<Share> Shares { get; set; } = new List<Share>();
}
