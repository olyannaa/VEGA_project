using System.IO.Pipelines;
using System.Net.Http.Headers;
using Minio;
using Minio.DataModel.Args;
using vega;
public class StorageManager : IStorageManager
{
    private readonly IMinioClient _minioClient;

    public StorageManager(string host, string access, string secret)
    {
        _minioClient = new MinioClient().WithEndpoint(host)
                                        .WithCredentials(access, secret)
                                        .WithSSL(false)
                                        .Build();
    }

    public async Task CreateOrderAsync(IFormFileCollection files, OrderModel order)
    { 
        if (order.OrderKKS == null)
        {
            throw new NullReferenceException();
        }
        foreach (IFormFile file in files)
        {
            var fileStream = file.OpenReadStream();
            await UploadFileAsync(fileStream, order.OrderKKS, file.ContentType, file.FileName, order.Role);
        }

        await InitMetaAsync(order.Description, order.OrderKKS);
    }

    public async Task DeleteOrderAsync(string? orderKKS, List<string> fileNames)
    {
        if (orderKKS == null)
        {
            throw new NullReferenceException();
        }
        var removeArgs = new RemoveObjectsArgs()
            .WithBucket("vega-orders-bucket")
            .WithObjects(fileNames);
        await _minioClient.RemoveObjectsAsync(removeArgs).ConfigureAwait(false);
    }

    private async Task InitMetaAsync(string? description, string orderKKS, string meta = "meta.txt")
    {
        var filePath = Path.Combine(Path.GetTempPath(), meta);

            using (StreamWriter sw = File.CreateText(filePath))
            {           
                sw.WriteLine(description);
                sw.Close();
            }
            var fileStream = new FileStream(filePath, FileMode.Open);
            await UploadFileAsync(fileStream, orderKKS, "text/txt", meta);
            fileStream.Close();
            File.Delete(filePath);
    }

    private async Task UploadFileAsync(Stream fileStream, string directory, string contentType, string name, string? role = null)
    {
        var updRole = role != null ? role + '/' : null;
        var putObjectArgs = new PutObjectArgs()
            .WithBucket("vega-orders-bucket")
            .WithObject($"{directory}/{updRole}{name}")
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
        fileStream.Close();
    }
}