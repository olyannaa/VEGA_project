using System.IO.Pipelines;
using Minio;
using Minio.DataModel.Args;
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

    public async Task CreateOrderAsync(IFormFileCollection files, string? description, string? orderKKS)
    {
        if (orderKKS == null)
        {
            throw new NullReferenceException();
        }
        foreach (IFormFile file in files)
        {
            var fileStream = file.OpenReadStream();
            await UploadFileAsync(fileStream, orderKKS, file.ContentType, file.FileName);
        }

        if (description != null)
        {
            await InitMetaAsync(description, orderKKS);
        }  
    }

    
    private async Task InitMetaAsync(string description, string orderKKS, string meta = "meta.txt")
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

    private async Task UploadFileAsync(Stream fileStream, string directory, string contentType, string name)
    {
        var putObjectArgs = new PutObjectArgs()
            .WithBucket("vega-bucket")
            .WithObject($"{directory}/{name}")
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
        fileStream.Close();
    }
}