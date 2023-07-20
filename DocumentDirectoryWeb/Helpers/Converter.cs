using DocumentFormat.OpenXml.Packaging;
using HtmlRendererCore.PdfSharp;
using OpenXmlPowerTools;
using PdfSharpCore;

namespace DocumentDirectoryWeb.Helpers;

public static class Converter
{
    private static string DocxToHtml(string docxFilePath)
    {
        var byteArray = File.ReadAllBytes(docxFilePath);

        using var memoryStream = new MemoryStream();
        memoryStream.Write(byteArray, 0, byteArray.Length);

        using var doc = WordprocessingDocument.Open(memoryStream, true);

        var settings = new HtmlConverterSettings
        {
            PageTitle = "My Page Title"
        };

        var html = HtmlConverter.ConvertToHtml(doc, settings);

        return html.ToStringNewLineOnAttributes();
    }

    private static void HtmlToPdf(string html, string pdfFilePath)
    {
        var pdf = PdfGenerator.GeneratePdf(html, PageSize.A4);
        pdf.Save(pdfFilePath);
    }

    public static void DocxToPdf(string docxFilePath, string pdfFilePath)
    {
        var html = DocxToHtml(docxFilePath);
        HtmlToPdf(html, pdfFilePath);
    }
}