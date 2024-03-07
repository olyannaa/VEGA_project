
public interface IStorageManager
{
    public Task CreateOrderAsync(IFormFileCollection files, OrderModel order);
    
    public Task DeleteOrderAsync(string? orderKKS, List<string> fileNames);
}