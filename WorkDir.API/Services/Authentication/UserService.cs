using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkDir.API.Entities;
using WorkDir.API.Exceptions;
using WorkDir.API.Models.DataModels;
using WorkDir.Domain.Entities;


namespace WorkDir.API.Services.Authentication;

public interface IUserService
{
    Task<User> RegisterUser(UserRegisterDto registerUserDto);
    Task<string> GenerateJwt(UserLoginDto loginUserDto);
    Task<IEnumerable<User>> GetAllUsers();
    Task<User> ReadUserInfo();
    Task Update(UserRegisterDto registerUserDto);
    Task DeleteUser();
}

public class UserService : IUserService
{
    private readonly WorkContext _workContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly AuthenticationSettings _authenticationSettings;
    private readonly IUserContextService _userContextService;

    public UserService(WorkContext workContext, IPasswordHasher<User> passwordHasher, AuthenticationSettings authenticationSettings, IUserContextService userContextService)
    {
        _workContext = workContext;
        _passwordHasher = passwordHasher;
        _authenticationSettings = authenticationSettings;
        _userContextService = userContextService;
    }

    public async Task<User> RegisterUser(UserRegisterDto registerUserDto)
    {
        var newUser = new User()
        {
            Email = registerUserDto.Email
        };

        var hashedPassword = _passwordHasher.HashPassword(newUser, registerUserDto.Password);
        newUser.PasswordHash = hashedPassword;
        _workContext.Users.Add(newUser);
        await _workContext.SaveChangesAsync();

        await _workContext.Entry(newUser).GetDatabaseValuesAsync();

        return newUser;
    }

    public async Task<string> GenerateJwt(UserLoginDto loginUserDto)
    {
        var user = await _workContext.Users.FirstOrDefaultAsync(u => u.Email == loginUserDto.Email);

        if (user is null)
            throw new BadRequestException("Invalid username or password");


        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginUserDto.Password);

        if (result == PasswordVerificationResult.Failed)
            throw new BadRequestException("Invalid username or password");


        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(_authenticationSettings.JwtExpireDays);

        var token = new JwtSecurityToken(_authenticationSettings.JwtIssuer,
            _authenticationSettings.JwtIssuer,
            claims,
            expires: expires,
            signingCredentials: cred);

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);

    }

    public async Task<User> ReadUserInfo()
    {
        var user = await _workContext.Users.FirstAsync(user => user.UserId == _userContextService.GetUserId);

        return user;
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        var users = await _workContext.Users.Where(u => true).ToListAsync();

        return users;
    }

    public async Task Update(UserRegisterDto registerUserDto)
    {
        var user = await _workContext.Users.FirstAsync(user => user.UserId == _userContextService.GetUserId);

        user.Email = registerUserDto.Email;

        var hashedPassword = _passwordHasher.HashPassword(user, registerUserDto.Password);
        user.PasswordHash = hashedPassword;

        await _workContext.SaveChangesAsync();
    }

    public async Task DeleteUser()
    {
        var user = await _workContext.Users.Include(u => u.Items).ThenInclude(i => i.Shares).FirstAsync(user => user.UserId == _userContextService.GetUserId);

        _workContext.Users.Remove(user);
        await _workContext.SaveChangesAsync();
    }

}
