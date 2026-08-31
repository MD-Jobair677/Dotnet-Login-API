public class ImageNameService : IImageNameService
{
    public string GenerateImageName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);

        var date = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        return $"{date}_{Guid.NewGuid()}{extension}";
    }

    public string GetImageExtension(string fileName)
    {
        return Path
                     .GetExtension(fileName)
                     .Replace(".", "")
                     .ToLower();
    }
}