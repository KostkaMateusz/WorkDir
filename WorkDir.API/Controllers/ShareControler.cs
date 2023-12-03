using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkDir.API.Models.DataModels;
using WorkDir.API.Services;

namespace WorkDir.API.Controllers;

[ApiController]
[Route("share")]
public class ShareControler : ControllerBase
{
    private readonly ISharingService _sharingService;

    public ShareControler(ISharingService sharingService)
    {
        _sharingService = sharingService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateShare([FromBody] Sharing sharing)
    {
        await _sharingService.CreateShare(sharing);
        return Ok(new { success = true });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteShare([FromBody] Sharing sharing)
    {
        await _sharingService.DeleteShare(sharing);

        return NoContent();
    }


}
