
public interface IStorageManager
{
    public Task CreateOrderAsync(IFormFileCollection files, string kks, string bucketName, string? description, string role);
    
    public Task DeleteOrderAsync(string? orderKKS, List<string> fileNames, string bucketName);

    public Task<(FileStream stream, string contentType)> GetFile(string fileName, string bucketName);
}