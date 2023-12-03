using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WorkDir.API.Models.DataModels;

public class FolderCreateDtos
{
    [Required]
    [MaxLength(50)]
    [MinLength(3)]
    public required string name { get; set; }
}

public class FileUploadDto
{
    [NotNull]
    [Required]
    public required IFormFile file { get; set; }
}

public class FileUploadInfoDto
{
    public Guid File_id { get; set; }
    public required string path { get; set; }
}

public class ItemDataDto
{
    public byte[] fileContents { get; set; }
    public string? contentType { get; set; }
    public string fileName { get; set; }
}

public class ItemDetailsDto
{
    public Guid id { get; set; }
    public string name { get; set; }
}

public class ParentFolderInfoDto
{
    public string name { get; set; }
    public Guid id { get; set; }
    public Guid owner_id { get; set; }
}

public class FolderDto
{
    public Guid id { get; set; }
    public string name { get; set; }
    public IEnumerable<ParentFolderInfoDto> parents { get; set; }
    public IEnumerable<ItemDetailsDto> dirs { get; set; }
    public IEnumerable<ItemDetailsDto> files { get; set; }
    public bool is_shared { get; set; }
    public IEnumerable<UserListInfoDto> shared_users { get; set; }
}

public class HomeFolderDto : FolderDto
{
    public HelperHomeFolderDto shared { get; set; }
}

//Due to compatibility this extra wrapping is needed
public class HelperHomeFolderDto
{
    public IEnumerable<ItemDetailsDto> dirs { get; set; }
}

public class UpdateNameDto
{
    [MaxLength(100)]
    [MinLength(4)]
    public string name { get; set; }
}