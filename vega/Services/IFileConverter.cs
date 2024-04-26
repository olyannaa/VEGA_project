public interface IFileConverter
{
    public FileStream ConvertDocToPdf(string filePath);

    public FileStream ConvertXlsxToPdf(string filePath);

    public string ConvertXlsxToJson(string filePath);
}