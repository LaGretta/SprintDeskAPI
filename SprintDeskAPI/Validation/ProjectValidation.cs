using FluentValidation;
using SprintDeskAPI.DTOs;

namespace SprintDeskAPI.Validation;

public class CreateProjectValid : AbstractValidator<CreateProjectDto>
{
    public CreateProjectValid()
    {
        RuleFor(n => n.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MinimumLength(3)
            .WithMessage("Name must be at least 3 characters long")
            .MaximumLength(50)
            .WithMessage("Name must be less than 50 characters long");
        
        RuleFor(n => n.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MinimumLength(3)
            .WithMessage("Description must be at least 3 characters long")
            .MaximumLength(100)
            .WithMessage("Description must be less than 100 characters long");
    }
}