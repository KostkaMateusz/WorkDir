using Microsoft.EntityFrameworkCore;
using WorkDir.API.Entities;
using WorkDir.API.Exceptions;
using WorkDir.API.Models.DataModels;
using WorkDir.API.Services.Authentication;
using WorkDir.Domain.Entities;

namespace WorkDir.API.Services;

public interface ISharingService
{
    Task CreateShare(Sharing sharing);
    Task DeleteShare(Sharing sharing);
}

public class SharingService : ISharingService
{
    private readonly WorkContext _workContext;
    private readonly IPermissionService _permissionService;
    private readonly IUserContextService _userContext;

    public SharingService(WorkContext workContext, IUserContextService userContext, IPermissionService permissionService)
    {
        _workContext = workContext;
        _userContext = userContext;
        _permissionService = permissionService;
    }

    public async Task CreateShare(Sharing sharing)
    {
        var currentUserId = _userContext.GetUserId;

        await _permissionService.OwnerPermissionOr401(currentUserId, sharing.shared_folder_id);

        var isShareExistAlready = await _workContext.Shares.AnyAsync(share => share.SharedWithId == sharing.shared_person_id && share.SharedItemId == sharing.shared_folder_id);
        if (isShareExistAlready)
            return;

        var share = new Share() { SharedWithId = sharing.shared_person_id, SharedItemId = sharing.shared_folder_id };

        await _workContext.AddAsync(share);
        await _workContext.SaveChangesAsync();
    }

    public async Task DeleteShare(Sharing sharing)
    {
        var currentUserId = _userContext.GetUserId;

        await _permissionService.OwnerPermissionOr401(currentUserId, sharing.shared_folder_id);

        var share = await _workContext.Shares.FirstOrDefaultAsync(share => share.SharedWithId == sharing.shared_person_id && share.SharedItemId == sharing.shared_folder_id);
        if (share is null)
            throw new NotFoundException("Share not found");

        _workContext.Remove(share);
        await _workContext.SaveChangesAsync();
    }

}
