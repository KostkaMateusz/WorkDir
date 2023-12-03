using System.ComponentModel.DataAnnotations;

namespace WorkDir.API.Models.DataModels;

public class UserLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}
public class UserRegisterDto
{
    [EmailAddress]
    [MaxLength(50)]
    [MinLength(6)]
    public required string Email { get; set; }
    [MaxLength(50)]
    [MinLength(6)]
    public required string Password { get; set; }
}

public class UserRegisterResponseDto
{
    public Guid id { get; set; }
    public string email { get; set; }
    public DateTime created_at { get; set; }
}

public class UserInfoDto
{
    public Guid id { get; set; }
    public string email { get; set; }
    public DateTime created_at { get; set; }
}

public class UserListInfoDto
{
    public Guid id { get; set; }
    public string email { get; set; }
}
