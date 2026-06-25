using FluentValidation;
using SprintDeskAPI.DTOs;

namespace SprintDeskAPI.Validation;

public class RegisterValid : AbstractValidator<RegisterDto>
{
    public RegisterValid()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Please enter your full name")
            .MinimumLength(3)
            .WithMessage("Full name must be at least 3 characters long")
            .MaximumLength(50)
            .WithMessage("Full name cannot exceed 50 characters");
        
        RuleFor(x => x.Email)
            .EmailAddress()
            .NotEmpty()
            .WithMessage("Email is required");
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Email is required")
            .MinimumLength(6)
            .WithMessage("Email needs to be at least 6 characters long")
            .MaximumLength(100)
            .WithMessage("Email needs to be less than 100 characters");
    }
}
public class LoginValid : AbstractValidator<LoginDto>
{
    public LoginValid()
    {
        RuleFor(login => login.Email)
            .EmailAddress()
            .NotEmpty()
            .WithMessage("Email is required");

        RuleFor(login => login.Password)
            .NotEmpty()
            .WithMessage("Email is required")
            .MinimumLength(6)
            .WithMessage("Email needs to be at least 6 characters long");
    }
}