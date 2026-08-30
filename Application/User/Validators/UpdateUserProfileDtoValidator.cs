using BulkMail.Application.DTOs;
using FluentValidation;

namespace BulkMail.Application.User.Validators
{
    public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
    {
        private static readonly string[] AllowedGenders = { "Male", "Female", "Other" };

        public UpdateUserProfileDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.LastName)
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
                .Matches(@"^\+?[0-9\s\-()]{7,20}$")
                .When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("A valid phone number is required.");

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");

            RuleFor(x => x.Gender)
                .Must(g => g is null || AllowedGenders.Contains(g, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Gender must be Male, Female or Other.");

            RuleFor(x => x.DateOfBirth)
                .Must((dto, dob) => dob is null || dob.Value.Date <= DateTime.UtcNow.Date)
                .WithMessage("Date of birth cannot be in the future.");

            RuleFor(x => x.Bio)
                .MaximumLength(1000).WithMessage("Bio cannot exceed 1000 characters.");
        }
    }
}
