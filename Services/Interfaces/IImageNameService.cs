public interface IImageNameService
{
    string GenerateImageName(string originalFileName);
    string GetImageExtension(string fileName);

}

