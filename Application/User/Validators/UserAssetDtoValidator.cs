using BulkMail.Application.DTOs;
using FluentValidation;

namespace BulkMail.Application.User.Validators
{
    public class UserAssetDtoValidator : AbstractValidator<UserAssetDto>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxSizeBytes = 2 * 1024 * 1024; // 2MB

        public UserAssetDtoValidator()
        {
            RuleFor(x => x.Path)
                .Must(f => f is null || AllowedExtensions.Contains(
                        System.IO.Path.GetExtension(f.FileName).ToLowerInvariant()))
                .WithMessage("Only .jpg, .jpeg, .png and .webp images are allowed.")
                .Must(f => f is null || f.Length > 0)
                .WithMessage("The uploaded file cannot be empty.")
                .Must(f => f is null || f.Length <= MaxSizeBytes)
                .WithMessage("The uploaded file cannot exceed 2MB.");
        }
    }
}
