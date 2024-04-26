
using System.Drawing.Imaging;
using System.Xml.Linq;
using DinkToPdf;
using DinkToPdf.Contracts;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;

public class FileConverter : IFileConverter
{
    IConverter _pdfConverter;

    public FileConverter(IConverter pdfConverter)
    {
        _pdfConverter = pdfConverter;
    }

    public FileStream ConvertDocToPdf(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var fullFilePath = fileInfo.FullName;
        var htmlText = string.Empty;
        try
        {
            htmlText = ParseDOCX(fileInfo);
        }
        catch (OpenXmlPackageException e)
        {
            if (e.ToString().Contains("Invalid Hyperlink"))
            {
                using (FileStream fs = new FileStream(fullFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    UriFixer.FixInvalidUri(fs, brokenUri => FixUri(brokenUri));
                }
                htmlText = ParseDOCX(fileInfo);
            }
        }

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
            ColorMode = DinkToPdf.ColorMode.Color,
            Orientation = Orientation.Landscape,
            PaperSize = PaperKind.A4,
            },
            Objects = {
                new ObjectSettings() {
                PagesCount = true,
                HtmlContent = htmlText,
                WebSettings = { DefaultEncoding = "utf-8" },
                HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                FooterSettings = { FontSize = 9, Right = "Page [page] of [toPage]" }
                }
            }
        };

        var pdf = _pdfConverter.Convert(doc);
        var tempPath = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(fileInfo.Name, ".pdf"));
        File.WriteAllBytes(tempPath, pdf);
        return new FileStream(tempPath, FileMode.Open);
    }

    public string ConvertXlsxToJson(string filePath)
    {
        //Todo
        return String.Empty;
    }

    public FileStream ConvertXlsxToPdf(string filePath)
    {
        //Todo
        return null;
    }

    private static Uri FixUri(string brokenUri)
    {
        var newURI = string.Empty;
        if (brokenUri.Contains("mailto:"))
        {
            var mailToCount = "mailto:".Length;
            brokenUri = brokenUri.Remove(0, mailToCount);
            newURI = brokenUri;
        }
        else
        {
            newURI = " ";
        }
        return new Uri(newURI);
    }

    private static string ParseDOCX(FileInfo fileInfo)
    {
        try
        {
            byte[] byteArray = File.ReadAllBytes(fileInfo.FullName);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(byteArray, 0, byteArray.Length);
                using (WordprocessingDocument wDoc =
                                            WordprocessingDocument.Open(memoryStream, true))
                {
                    int imageCounter = 0;
                    var pageTitle = fileInfo.FullName;
                    var part = wDoc.CoreFilePropertiesPart;
                    if (part != null)
                        pageTitle = (string?)part.GetXDocument()
                                                .Descendants(DC.title)
                                                .FirstOrDefault() ?? fileInfo.FullName;

                    WmlToHtmlConverterSettings settings = new WmlToHtmlConverterSettings()
                    {
                        AdditionalCss = "body { margin: 1cm auto; max-width: 20cm; padding: 0; }",
                        PageTitle = pageTitle,
                        FabricateCssClasses = true,
                        CssClassPrefix = "pt-",
                        RestrictToSupportedLanguages = false,
                        RestrictToSupportedNumberingFormats = false,
                        ImageHandler = imageInfo =>
                        {
                            ++imageCounter;
                            var extension = imageInfo.ContentType.Split('/')[1].ToLower();
                            ImageFormat? imageFormat = null;
                            if (extension == "png") imageFormat = ImageFormat.Png;
                            else if (extension == "gif") imageFormat = ImageFormat.Gif;
                            else if (extension == "bmp") imageFormat = ImageFormat.Bmp;
                            else if (extension == "jpeg") imageFormat = ImageFormat.Jpeg;
                            else if (extension == "tiff")
                            {
                                extension = "gif";
                                imageFormat = ImageFormat.Gif;
                            }
                            else if (extension == "x-wmf")
                            {
                                extension = "wmf";
                                imageFormat = ImageFormat.Wmf;
                            }

                            if (imageFormat == null) return null;

                            string? base64 = null;
                            try
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    imageInfo.Bitmap.Save(ms, imageFormat);
                                    var ba = ms.ToArray();
                                    base64 = System.Convert.ToBase64String(ba);
                                }
                            }
                            catch (System.Runtime.InteropServices.ExternalException)
                            { return null; }

                            var format = imageInfo.Bitmap.RawFormat;
                            var codec = ImageCodecInfo.GetImageDecoders()
                                                        .First(c => c.FormatID == format.Guid);
                            var mimeType = codec.MimeType;

                            var imageSource =
                                    string.Format("data:{0};base64,{1}", mimeType, base64);

                            var img = new XElement(Xhtml.img,
                                    new XAttribute(NoNamespace.src, imageSource),
                                    imageInfo.ImgStyleAttribute,
                                    imageInfo.AltText != null ?
                                        new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                            return img;
                        }
                    };

                    var htmlElement = WmlToHtmlConverter.ConvertToHtml(wDoc, settings);
                    var html = new XDocument(new XDocumentType("html", null, null, null),
                                                                                htmlElement);
                    var htmlString = html.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
                    return htmlString;
                }
            }
        }
        catch
        {
            return "The file is either open, please close it or contains corrupt data";
        }
    }
}