
public interface IStorageManager
{
    public void CreateOrder(IFormFileCollection files, string kks, string bucketName, string? description, string role);
    
    public void DeleteFilesAsync(List<string> fileNames, string bucketName);

    public void UploadFileAsync(Stream fileStream, string directory, string bucketName, string contentType, string name, string? role = null);

    public Task<string> SaveTempFileAsync(Stream fileStream, string fileName);

    public bool TryDeleteTempFile(string filePath);

    public Task<(FileStream stream, string contentType)> GetFile(string fileName, string bucketName);
}