
using Microsoft.AspNetCore.StaticFiles;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
public class StorageManager : IStorageManager
{
    private readonly IMinioClient _minioClient;

    public StorageManager(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task CreateOrderAsync(IFormFileCollection files, string kks, string bucketName, string? description, string? step)
    { 
        if (kks == null)
        {
            throw new NullReferenceException();
        }
        foreach (IFormFile file in files)
        {
            var fileStream = file.OpenReadStream();
            await UploadFileAsync(fileStream, kks, bucketName, file.ContentType, file.FileName, step);
        }
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

    public async Task UploadFileAsync(Stream fileStream, string directory, string bucketName, string contentType, string name, string? step = null)
    {
        var updStep = step != null ? step + '/' : null;
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject($"{directory}/{updStep}{name}")
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