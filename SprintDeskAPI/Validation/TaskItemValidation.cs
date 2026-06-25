using FluentValidation;
using SprintDeskAPI.DTOs;

namespace SprintDeskAPI.Validation;

public class CreateTaskValid : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValid()
    {
        RuleFor(dto => dto.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MinimumLength(3)
            .WithMessage("Title must be at least 3 characters long")
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters");
        
        RuleFor(dto => dto.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MinimumLength(3)
            .WithMessage("Description must be at least 3 characters long")
            .MaximumLength(100)
            .WithMessage("Description cannot exceed 100 characters");
        
        RuleFor(dto => dto.DueDate)
            .NotEmpty()
            .WithMessage("DueDate is required")
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("DueDate must be after DateTime.Now");
    }
}