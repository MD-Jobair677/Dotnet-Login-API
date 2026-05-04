public static class FileUploadHelper
{
    public static string UploadImage(IFormFile file, string folderName)
    {
        if (file == null) return null;

        // dynamic folder path
        var folderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            folderName
        );

        // create folder if not exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

        var filePath = Path.Combine(folderPath, fileName);

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