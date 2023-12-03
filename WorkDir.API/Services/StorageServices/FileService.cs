using AutoMapper;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using WorkDir.API.Entities;
using WorkDir.API.Exceptions;
using WorkDir.API.Models.DataModels;
using WorkDir.API.Services.Authentication;
using WorkDir.Domain.Entities;
using WorkDir.Storage.StorageServices;

namespace WorkDir.API.Services.StorageServices;

public interface IFileService
{
    Task<Guid> CreateSubFolder(Guid parentFolderId, string name);
    void CreateRootFolder(Guid userId);
    Task<Guid> UploadFile(Guid parentFolderId, IFormFile file);
    Task<ItemDataDto> GetItemData(Guid fileId);
    Task<FolderDto> ListFolder(Guid folderID);
    Task<FolderDto> ListHomeFolder();
    Task UpdateItemName(Guid itemId, string newName);
    Task DeleteItem(Guid itemId);
}

public class FileService : IFileService
{
    private readonly WorkContext _workContext;
    private readonly IUserContextService _userContext;
    private readonly IAzureStorageService _azureStorageService;
    private readonly IPermissionService _permissionService;
    private readonly IMapper _mapper;

    public FileService(WorkContext workContext, IUserContextService userContext, IAzureStorageService azureStorageService, IPermissionService permissionService, IMapper mapper)
    {
        _workContext = workContext;
        _userContext = userContext;
        _azureStorageService = azureStorageService;
        _permissionService = permissionService;
        _mapper = mapper;
    }

    public void CreateRootFolder(Guid userId)
    {
        var newRootFolder = new Item() { FileName = "home", IsDirectory = true, OwnerId = userId };

        //Add folder to DB
        _workContext.Items.Add(newRootFolder);
        _workContext.SaveChanges();
    }

    private void CheckIfFileExist(Guid folderId, bool isDirectory = true)
    {
        var isFolderExsit = _workContext.Items.Any(item => item.Id == folderId && item.IsDirectory == isDirectory);

        //Check if parent folder exits
        if (!isFolderExsit)
            throw new NotFoundException($"Folder with ID: {folderId} not found");
    }

    private void CheckIfNameIsTaken(Guid parentFolderId, string name, bool isDirectory = false)
    {
        // Check if folder with name already exist
        var folderWithNameAlreadyExist = _workContext.Items.Any(item => item.ParentFolderId == parentFolderId && item.FileName == name && item.IsDirectory == isDirectory);

        if (folderWithNameAlreadyExist)
            throw new FolderAlreadyExistException($"Folder with name:{name} already exist");
    }

    public async Task<Guid> CreateSubFolder(Guid parentFolderId, string name)
    {
        var currentUserID = _userContext.GetUserId;
        var parentFolder = _workContext.Items.FirstOrDefault(item => item.Id == parentFolderId);

        CheckIfFileExist(parentFolderId);
        await _permissionService.OwnerPermissionOr401(currentUserID, parentFolderId);

        CheckIfNameIsTaken(parentFolderId, name, true);

        //Create Item as a folder
        var newFolder = new Item() { ParentFolderId = parentFolderId, FileName = name, IsDirectory = true, OwnerId = currentUserID };

        //Add folder to DB
        await _workContext.Items.AddAsync(newFolder);
        await _workContext.SaveChangesAsync();
        await _workContext.Entry(newFolder).GetDatabaseValuesAsync();

        return newFolder.Id;
    }

    public async Task<Guid> UploadFile(Guid parentFolderId, IFormFile file)
    {
        var currentUserID = _userContext.GetUserId;
        // Validation
        CheckIfFileExist(parentFolderId);

        await _permissionService.OwnerPermissionOr401(currentUserID, parentFolderId);

        CheckIfNameIsTaken(parentFolderId, file.FileName, false);

        //Create Item as a file
        var newItem = new Item() { ParentFolderId = parentFolderId, FileName = file.FileName, IsDirectory = false, OwnerId = currentUserID };

        //Add Item to DB
        await _workContext.Items.AddAsync(newItem);
        await _workContext.SaveChangesAsync();
        await _workContext.Entry(newItem).GetDatabaseValuesAsync();

        _azureStorageService.SaveFile(newItem.Id, file);

        return newItem.Id;
    }

    public async Task<ItemDataDto> GetItemData(Guid fileId)
    {
        var currentUserID = _userContext.GetUserId;

        CheckIfFileExist(fileId, false);

        await _permissionService.AnyPermissionsOr401(currentUserID, fileId);

        var fileInformation = await _workContext.Items.FirstAsync(item => item.Id == fileId);
        //Get file bytes
        var fileContent = _azureStorageService.GetFileData(fileId);

        //Get content Type
        var contentProvider = new FileExtensionContentTypeProvider();
        contentProvider.TryGetContentType(fileInformation.FileName, out string? contentType);
        if (contentType is null)
            contentType = "application/octet-stream";

        return new ItemDataDto() { fileContents = fileContent, contentType = contentType, fileName = fileInformation.FileName };
    }

    private async Task<IEnumerable<ItemDetailsDto>> GetChildrenItem(Guid parentfolderId, bool IsDirectory = true)
    {
        var childrenFolderList = await _workContext.Items.Where(item => item.ParentFolderId == parentfolderId && item.IsDirectory == IsDirectory).ToListAsync();

        var childrenFolderListInfoDto = _mapper.Map<List<ItemDetailsDto>>(childrenFolderList);

        return childrenFolderListInfoDto;
    }

    private async Task<IEnumerable<ParentFolderInfoDto>> GetAllParents(Guid FolderId)
    {
        var parentsList = new List<Item>();

        var childFolder = await _workContext.Items.FirstAsync(item => item.Id == FolderId && item.IsDirectory == true);

        while (true)
        {
            var parentFolder = await _workContext.Items.FirstOrDefaultAsync(item => item.Id == childFolder.ParentFolderId && item.IsDirectory == true);
            if (parentFolder is null)
                break;

            parentsList.Add(parentFolder);
            childFolder = parentFolder;
        }

        var parentsListDto = _mapper.Map<List<ParentFolderInfoDto>>(parentsList);

        //List should be reversed
        parentsListDto.Reverse();
        return parentsListDto;
    }

    private async Task<IEnumerable<ItemDetailsDto>> ListSharedFolder()
    {
        var currentUserID = _userContext.GetUserId;

        var shares = await _workContext.Shares.Include(share => share.SharedItem).Where(share => share.SharedWithId == currentUserID).ToListAsync();

        var sharedFolders = shares.Select(share => share.SharedItem).ToList();

        var sharedFoldersDto = _mapper.Map<List<ItemDetailsDto>>(sharedFolders);

        return sharedFoldersDto;
    }

    public async Task<FolderDto> ListFolder(Guid folderID)
    {
        var currentUserID = _userContext.GetUserId;
        var homeFolder = await _workContext.Items.FirstAsync(item => item.FileName == "home" && item.OwnerId == currentUserID && item.IsDirectory == true);

        if (folderID == homeFolder.Id)
            return await ListHomeFolder();
        else
            return await ListFolderBase(folderID);
    }

    private async Task<FolderDto> ListFolderBase(Guid folderID)
    {
        var currentUserID = _userContext.GetUserId;

        await _permissionService.AnyPermissionsOr401(currentUserID, folderID);
        CheckIfFileExist(folderID, true);
        var childrenFolder = await GetChildrenItem(folderID, true);
        var childrenFiles = await GetChildrenItem(folderID, false);

        // if only share permission trim parents to highest share and add home folder to list
        IEnumerable<ParentFolderInfoDto> parentsFolders;
        var folder = await _workContext.Items.Include(item => item.Shares).ThenInclude(s => s.SharedWith).FirstAsync(item => item.Id == folderID);

        if (await _permissionService.SharePermission(currentUserID, folderID) is true)
        {
            var sharesPermissionParents = await _permissionService.GetParentsWithSharePermission(currentUserID, folderID);

            //get home folder
            var homeFolder = await _workContext.Items.FirstAsync(item => item.FileName == "home" && item.OwnerId == currentUserID && item.IsDirectory == true);
            sharesPermissionParents.Insert(0, homeFolder);
            sharesPermissionParents.Remove(folder);
            parentsFolders = _mapper.Map<List<ParentFolderInfoDto>>(sharesPermissionParents);
        }
        else
            parentsFolders = await GetAllParents(folderID);


        //Share Part
        var isShared = folder.Shares.Any();
        var sharedUsers = folder.Shares.Select(s => s.SharedWith);
        var sharedUserDto = _mapper.Map<List<UserListInfoDto>>(sharedUsers);

        var folderContentInfo = new FolderDto() { id = folder.Id, name = folder.FileName, parents = parentsFolders, dirs = childrenFolder, files = childrenFiles, is_shared = isShared, shared_users = sharedUserDto };

        return folderContentInfo;
    }

    public async Task<FolderDto> ListHomeFolder()
    {
        var currentUserID = _userContext.GetUserId;
        var homeFolder = await _workContext.Items.FirstAsync(item => item.FileName == "home" && item.OwnerId == currentUserID && item.IsDirectory == true);

        var homeFolderContent = await ListFolderBase(homeFolder.Id);
        var sharedFolderList = await ListSharedFolder();

        var homeFolderDto = _mapper.Map<HomeFolderDto>(homeFolderContent);

        var helper = new HelperHomeFolderDto() { dirs = sharedFolderList };

        homeFolderDto.shared = helper;

        return homeFolderDto;
    }

    public async Task UpdateItemName(Guid itemId, string newName)
    {
        var currentUserID = _userContext.GetUserId;

        CheckIfFileExist(itemId);

        await _permissionService.OwnerPermissionOr401(currentUserID, itemId);

        var itemToUpdate = await _workContext.Items.FirstOrDefaultAsync(item => item.Id == itemId);

        itemToUpdate.FileName = newName;

        await _workContext.SaveChangesAsync();
    }

    public async Task DeleteItem(Guid itemId)
    {
        var currentUserID = _userContext.GetUserId;

        await _permissionService.OwnerPermissionOr401(currentUserID, itemId);

        var itemToDelete = await _workContext.Items.Include(item => item.Shares).FirstOrDefaultAsync(item => item.Id == itemId);
        if (itemToDelete is null)
            throw new NotFoundException("Item not found");

        _workContext.Items.Remove(itemToDelete);
        await _workContext.SaveChangesAsync();
    }

}
