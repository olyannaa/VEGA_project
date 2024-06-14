
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

    public void CreateOrder(IFormFileCollection files, string kks, string bucketName, string? description, string? step)
    { 
        if (kks == null)
        {
            throw new NullReferenceException();
        }
        foreach (IFormFile file in files)
        {
            var fileStream = file.OpenReadStream();
            UploadFileAsync(fileStream, kks, bucketName, file.ContentType, file.FileName, step);
        }
    }

    public async void DeleteFilesAsync(List<string> fileNames, string bucketName)
    {
        if (!fileNames.Any())
        {
            return;
        }
        var removeArgs = new RemoveObjectsArgs()
            .WithBucket(bucketName)
            .WithObjects(fileNames);
        await _minioClient.RemoveObjectsAsync(removeArgs).ConfigureAwait(false);
    }

    public async void UploadFileAsync(Stream fileStream, string directory, string bucketName, string contentType, string name, string? step = null)
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

    public async Task<(FileStream stream, string contentType)> GetFile(string filePath, string bucketName)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), filePath.Split("/").Last());
        using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
        {
            try
            {
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                                                .WithBucket(bucketName)
                                                .WithObject(filePath);
                await _minioClient.StatObjectAsync(statObjectArgs);

                GetObjectArgs getObjectArgs = new GetObjectArgs()
                                              .WithBucket(bucketName)
                                              .WithObject(filePath)
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
        if (!provider.TryGetContentType(filePath, out contentType))
        {
            contentType = "application/octet-stream";
        }
        var stream = new FileStream(tempFilePath, FileMode.Open);
        return (stream, contentType);
    }

    public async Task<string> SaveTempFileAsync(Stream fileStream, string fileName)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);
        var buffer = new byte[fileStream.Length];
        fileStream.Read(buffer);
        fileStream.Close();
        await File.WriteAllBytesAsync(tempPath, buffer);
        return tempPath;
    }

    public bool TryDeleteTempFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        return false;
    }
}