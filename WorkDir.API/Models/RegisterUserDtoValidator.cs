using FluentValidation;
using WorkDir.API.Entities;
using WorkDir.API.Models.DataModels;

namespace WorkDir.API.Models;

public class RegisterUserDtoValidator : AbstractValidator<UserRegisterDto>
{
    public RegisterUserDtoValidator(WorkContext workContext)
    {
        RuleFor(RegisterUserDto => RegisterUserDto.Password).MinimumLength(6);

        RuleFor(RegisterUserDto => RegisterUserDto.Email).NotEmpty().EmailAddress();

        RuleFor(RegisterUserDto => RegisterUserDto.Email).Custom((value, context) =>
            {
                var emailInUse = workContext.Users.Any(u => u.Email == value);
                if (emailInUse)
                {
                    context.AddFailure("Email", "That email is taken");
                }
            });
    }
}
