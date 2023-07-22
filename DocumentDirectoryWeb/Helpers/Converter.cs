using DocumentFormat.OpenXml.Packaging;
using HtmlAgilityPack;
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
            PageTitle = "Document"
        };

        var html = HtmlConverter.ConvertToHtml(doc, settings);

        return html.ToStringNewLineOnAttributes();
    }

    private static string RemoveLinksFromHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Находим все элементы "a" (ссылки) и удаляем их из документа
        var linkElements = doc.DocumentNode.SelectNodes("//a");
        if (linkElements != null)
            foreach (var linkElement in linkElements)
            {
                // Получаем текст из тега "a"
                var linkText = linkElement.InnerText;

                // Заменяем тег "a" его текстом
                linkElement.ParentNode.ReplaceChild(doc.CreateTextNode(linkText), linkElement);
            }

        // Возвращаем очищенный HTML без ссылок
        return doc.DocumentNode.OuterHtml;
    }

    private static void HtmlToPdf(string html, string pdfFilePath)
    {
        var pdf = PdfGenerator.GeneratePdf(html, PageSize.A4);
        pdf.Save(pdfFilePath);
    }

    public static void DocxToPdf(string docxFilePath, string pdfFilePath)
    {
        var html = DocxToHtml(docxFilePath);
        html = RemoveLinksFromHtml(html);
        HtmlToPdf(html, pdfFilePath);
    }
}