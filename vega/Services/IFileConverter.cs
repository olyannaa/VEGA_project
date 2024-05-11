public interface IFileConverter
{
    public FileStream ConvertDocToPdf(string filePath);

    public FileStream ConvertXlsxToPdf(string filePath);

    public bool TryConvertXlsxToJson(string filePath, out string json);
}