
public interface IStorageManager
{
    public Task CreateOrderAsync(IFormFileCollection files, string kks, string? description, string role);
    
    public Task DeleteOrderAsync(string? orderKKS, List<string> fileNames);
}