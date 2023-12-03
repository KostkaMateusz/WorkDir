namespace WorkDir.Domain.Entities;

public class Share
{
    public User SharedWith { get; set; }
    public Guid SharedWithId { get; set; }
    public Item SharedItem { get; set; }
    public Guid SharedItemId { get; set; }
}