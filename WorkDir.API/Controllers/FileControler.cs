using Microsoft.AspNetCore.Mvc;
using WorkDir.API.Models.DataModels;
using WorkDir.API.Services.StorageServices;

namespace WorkDir.API.Controllers;


[ApiController]
public class FileControler : ControllerBase
{
    private readonly IFileService _fileService;

    public FileControler(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("folder/{parentFolderId}")]
    public async Task<IActionResult> CreateFolder([FromRoute] Guid parentFolderId, [FromBody] FolderCreateDtos folderName)
    {
        var folderId = await _fileService.CreateSubFolder(parentFolderId, folderName.name);

        // Must return custom json response for compatibility reasons
        var customJsonResponse = $$"""{"new folder id":{{folderId}}}""";

        return Content(customJsonResponse, "application/json", System.Text.Encoding.UTF8);
    }

    [HttpGet("folder/{folderId}")]
    public async Task<IActionResult> ListFolder([FromRoute] Guid folderId)
    {
        var folderContent = await _fileService.ListFolder(folderId);

        return Ok(folderContent);
    }

    [HttpGet("folder/")]
    public async Task<IActionResult> ListHomeFolder()
    {
        var folderContent = await _fileService.ListHomeFolder();

        return Ok(folderContent);
    }

    [HttpPut("folder/{folderId}")]
    public async Task<IActionResult> ChangeItemName([FromRoute] Guid folderId, [FromBody] UpdateNameDto updateNameDto)
    {
        await _fileService.UpdateItemName(folderId, updateNameDto.name);

        return Ok();
    }

    [HttpDelete("folder/{folderId}")]
    public async Task<IActionResult> DeleteItem([FromRoute] Guid folderId)
    {
        await _fileService.DeleteItem(folderId);

        return NoContent();
    }


    [HttpPost("file/upload/{parentFolderId}")]
    public async Task<IActionResult> UploadFile([FromRoute] Guid parentFolderId, [FromForm] FileUploadDto fileUploadDto)
    {
        var fileId = await _fileService.UploadFile(parentFolderId, fileUploadDto.file);

        // Must return custom json response for compatibility reasons
        var customJsonResponse = $$"""{"File_id":{{fileId}},"path":{{fileUploadDto.file.FileName}}}""";

        var response = Content(customJsonResponse, "application/json", System.Text.Encoding.UTF8);
        response.StatusCode = 201;

        return response;

    }


    [HttpGet("file/download/{fileID}")]
    public async Task<IActionResult> GetFile([FromRoute] Guid fileID)
    {
        var fileContent = await _fileService.GetItemData(fileID);

        return File(fileContent.fileContents, fileContent.contentType, fileContent.fileName);
    }




}
