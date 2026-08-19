public static class FileUploadHelper
{
      public static string UploadImage(
        IFormFile file,
        string folderName,
        string? customFileName = null,
        string[]? allowedExtensions = null,
        long? maxSizeInMB = null
    )
    {
        if (file == null) return null;

        // ===================== VALIDATION =====================

        // 1. File size check (optional)
        if (maxSizeInMB.HasValue)
        {
            var maxSizeBytes = maxSizeInMB.Value * 1024 * 1024;

            if (file.Length > maxSizeBytes)
            {
                throw new Exception($"File size exceeded {maxSizeInMB} MB limit");
            }
        }

        // 2. Extension check (optional)
        if (allowedExtensions != null && allowedExtensions.Length > 0)
        {
            var extension = Path
                .GetExtension(file.FileName)
                .ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception("Invalid file type");
            }
        }

        // ===================== FOLDER =====================

        var folderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            folderName
        );

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // ===================== FILE NAME =====================

        var fileName = customFileName
            ?? Guid.NewGuid() + Path.GetExtension(file.FileName);

        var filePath = Path.Combine(folderPath, fileName);

        // ===================== SAVE FILE =====================

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return $"/uploads/{folderName}/{fileName}";
    }

    public static void DeleteImage(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath)) return;

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            imagePath.TrimStart('/')
        );

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }




}