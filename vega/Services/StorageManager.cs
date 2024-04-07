using System.IO.Pipelines;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Win32.SafeHandles;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
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

    public async Task CreateOrderAsync(IFormFileCollection files, string kks, string bucketName, string? description, string? role)
    { 
        if (kks == null)
        {
            throw new NullReferenceException();
        }
        foreach (IFormFile file in files)
        {
            var fileStream = file.OpenReadStream();
            await UploadFileAsync(fileStream, kks, bucketName, file.ContentType, file.FileName, role);
        }

        await InitMetaAsync(description, kks, bucketName, role);
    }

    public async Task DeleteOrderAsync(string? orderKKS, List<string> fileNames, string bucketName)
    {
        if (orderKKS == null)
        {
            throw new NullReferenceException();
        }
        if (!fileNames.Any())
        {
            return;
        }
        var removeArgs = new RemoveObjectsArgs()
            .WithBucket(bucketName)
            .WithObjects(fileNames);
        await _minioClient.RemoveObjectsAsync(removeArgs).ConfigureAwait(false);
    }

    private async Task InitMetaAsync(string? description, string orderKKS, string bucketName, string? role, string meta = "meta.txt")
    {
        var filePath = Path.Combine(Path.GetTempPath(), meta);

            using (StreamWriter sw = File.CreateText(filePath))
            {           
                sw.WriteLine(description);
                sw.Close();
            }
            var fileStream = new FileStream(filePath, FileMode.Open);
            await UploadFileAsync(fileStream, orderKKS, bucketName, "text/txt", meta, role);
            fileStream.Close();
            File.Delete(filePath);
    }

    public async Task UploadFileAsync(Stream fileStream, string directory, string bucketName, string contentType, string name, string? role = null)
    {
        var updRole = role != null ? role + '/' : null;
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject($"{directory}/{updRole}{name}")
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
        fileStream.Close();
    }

    public async Task<(FileStream stream, string contentType)> GetFile(string fileName, string bucketName)
    {
        var filePath = Path.Combine(Path.GetTempPath(), fileName.Split("/").Last());
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            try
            {
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                                                .WithBucket(bucketName)
                                                .WithObject(fileName);
                await _minioClient.StatObjectAsync(statObjectArgs);

                GetObjectArgs getObjectArgs = new GetObjectArgs()
                                              .WithBucket(bucketName)
                                              .WithObject(fileName)
                                              .WithCallbackStream((stream) =>
                                                   {
                                                       stream.CopyTo(fileStream);
                                                   });
                await _minioClient.GetObjectAsync(getObjectArgs);
            }
            catch (MinioException e)
            {
                Console.WriteLine("Error occurred: " + e);
            }

            fileStream.Close();
        };

        var provider = new FileExtensionContentTypeProvider();
        string? contentType;
        if (!provider.TryGetContentType(fileName, out contentType))
        {
            contentType = "application/octet-stream";
        }
        var stream = new FileStream(filePath, FileMode.Open);
        return (stream, contentType);
    }
}