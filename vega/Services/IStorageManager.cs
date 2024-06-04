
public interface IStorageManager
{
    public Task CreateOrderAsync(IFormFileCollection files, string kks, string bucketName, string? description, string role);
    
    public Task DeleteOrderAsync(string? orderKKS, List<string> fileNames, string bucketName);

    public Task UploadFileAsync(Stream fileStream, string directory, string bucketName, string contentType, string name, string? role = null);

    public Task<string> SaveTempFileAsync(Stream fileStream, string fileName);

    public bool TryDeleteTempFile(string filePath);

    public Task<(FileStream stream, string contentType)> GetFile(string fileName, string bucketName);
}