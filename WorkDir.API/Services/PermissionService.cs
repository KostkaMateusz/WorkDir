using Microsoft.EntityFrameworkCore;
using WorkDir.API.Entities;
using WorkDir.API.Exceptions;
using WorkDir.Domain.Entities;
namespace WorkDir.API.Services;

public interface IPermissionService
{
    Task AnyPermissionsOr401(Guid userId, Guid itemId);
    Task OwnerPermissionOr401(Guid userId, Guid itemId);
    Task SharePermissionOr401(Guid userId, Guid itemId);
    Task<bool> SharePermission(Guid userId, Guid itemId);
    Task<List<Item>> GetParentsWithSharePermission(Guid userId, Guid itemId);
}

public class PermissionService : IPermissionService
{
    private readonly WorkContext _workContext;

    public PermissionService(WorkContext workContext)
    {
        _workContext = workContext;
    }

    public async Task<bool> OwnerPermissions(Guid userId, Guid itemId)
    {
        var ownerPermission = await _workContext.Items.AnyAsync(item => item.Id == itemId && item.OwnerId == userId);

        return ownerPermission;
    }
    public async Task<bool> SharePermission(Guid userId, Guid itemId)
    {
        IEnumerable<Item> join = await GetParentsWithSharePermission(userId, itemId);

        // return if any parent has share permission
        return join.Any();
    }

    public async Task<List<Item>> GetParentsWithSharePermission(Guid userId, Guid itemId)
    {
        var sharesPermissions = await _workContext.Shares.Where(share => share.SharedWithId == userId).ToListAsync();

        var parentsList = new List<Item>();
        var childItem = await _workContext.Items.FirstOrDefaultAsync(item => item.Id == itemId);
        if (childItem is null)
            throw new NotFoundException($"Item with id:{itemId} not found");

        parentsList.Add(childItem);

        while (true)
        {
            var parentFolder = await _workContext.Items.FirstOrDefaultAsync(item => item.Id == childItem.ParentFolderId && item.IsDirectory == true);
            if (parentFolder is null)
                break;

            parentsList.Add(parentFolder);
            childItem = parentFolder;
        }

        //Get all folder with permission 
        var join = from parent in parentsList join share in sharesPermissions on parent.Id equals share.SharedItemId select parent;
        return join.ToList();
    }

    public async Task OwnerPermissionOr401(Guid userId, Guid itemId)
    {
        var ownerPermission = await OwnerPermissions(userId, itemId);

        if (ownerPermission is false)
            throw new ForbidException("You are not authorized");
    }

    public async Task SharePermissionOr401(Guid userId, Guid itemId)
    {
        var sharePermission = await SharePermission(userId, itemId);
        if (sharePermission is false)
            throw new ForbidException("You are not authorized");
    }

    public async Task AnyPermissionsOr401(Guid userId, Guid itemId)
    {
        var anyPermissions = await OwnerPermissions(userId, itemId) || await SharePermission(userId, itemId);
        if (anyPermissions is false)
            throw new ForbidException("You are not authorized");
    }

}
