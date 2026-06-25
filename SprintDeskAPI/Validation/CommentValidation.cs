using FluentValidation;
using SprintDeskAPI.DTOs;

namespace SprintDeskAPI.Validation;

public class CreateCommentValid : AbstractValidator<CreateCommentDto>
{
    public CreateCommentValid()
    {
        RuleFor(n => n.Text)
        .NotEmpty()
        .WithMessage("The text field is required.")
        .MinimumLength(3)
        .WithMessage("The text needs to be at least 3 characters long")
        .MaximumLength(100)
        .WithMessage("The text needs to be less than 100 characters");
    }
}
