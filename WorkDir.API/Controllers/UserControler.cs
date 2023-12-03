using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkDir.API.Exceptions;
using WorkDir.API.Models.DataModels;
using WorkDir.API.Services.Authentication;
using WorkDir.API.Services.StorageServices;

namespace WorkDir.API.Controllers;


[ApiController]
[Route("user")]
public class UserControler : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<UserRegisterDto> _registerUserDtoValidator;
    private readonly IMapper _mapper;
    private readonly IFileService _fileService;

    public UserControler(IUserService userService, IValidator<UserRegisterDto> registerUserDtoValidator, IMapper mapper, IFileService fileService)
    {
        _userService = userService;
        _registerUserDtoValidator = registerUserDtoValidator;
        _mapper = mapper;
        _fileService = fileService;
    }


    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login([FromBody] UserLoginDto loginUserDto)
    {
        string token = await _userService.GenerateJwt(loginUserDto);
        var returnObject = new { access_token = token, token_type = "bearer", email = loginUserDto.Email };
        return Ok(returnObject);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> RegisterUser([FromBody] UserRegisterDto registerUserDto)
    {
        var validationResult = await _registerUserDtoValidator.ValidateAsync(registerUserDto);

        if (!validationResult.IsValid)
            throw new ValidationErrorException(validationResult.ToString());

        var newUser = await _userService.RegisterUser(registerUserDto);

        _fileService.CreateRootFolder(newUser.UserId);

        var newUserDto = _mapper.Map<UserRegisterResponseDto>(newUser);

        return Ok(newUserDto);
    }

    [HttpGet()]
    [Authorize]
    public async Task<ActionResult> UserInfo()
    {
        var userInfo = await _userService.ReadUserInfo();
        var userInfoDto = _mapper.Map<UserInfoDto>(userInfo);
        return Ok(userInfoDto);
    }

    [HttpGet("all")]
    [Authorize]
    public async Task<ActionResult> AllUsers()
    {
        var userInfoList = await _userService.GetAllUsers();
        var userListInfoDto = _mapper.Map<List<UserListInfoDto>>(userInfoList);
        return Ok(userListInfoDto);
    }

    [Authorize]
    [HttpPut()]
    public async Task<ActionResult> UpdateUser([FromBody] UserRegisterDto registerUserDto)
    {
        var validationResult = await _registerUserDtoValidator.ValidateAsync(registerUserDto);

        if (!validationResult.IsValid)
            throw new ValidationErrorException(validationResult.ToString());

        await _userService.Update(registerUserDto);

        return Ok(registerUserDto);
    }

    [Authorize]
    [HttpDelete()]
    public async Task<ActionResult> DeleteUser()
    {
        await _userService.DeleteUser();

        return NoContent();
    }
}