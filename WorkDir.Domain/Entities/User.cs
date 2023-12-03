namespace WorkDir.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime? CreationDate { get; set; } = DateTime.Now;
    public IList<Item> Items { get; set; } = new List<Item>();
    public IList<Share> Shares { get; set; }= new List<Share>();
}