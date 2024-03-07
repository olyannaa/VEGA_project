
public interface IStorageManager
{
    public Task CreateOrderAsync(IFormFileCollection files, string? description, string? orderKKS);
}