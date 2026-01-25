using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using DerotMyBrain.Core.Interfaces.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace DerotMyBrain.Infrastructure.Services;

public class TextExtractor : ITextExtractor
{
    private readonly ILogger<TextExtractor> _logger;

    public TextExtractor(ILogger<TextExtractor> logger)
    {
        _logger = logger;
    }

    public string ExtractText(string filePath, string extension)
    {
        try 
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"File not found: {filePath}");

            return extension.ToLowerInvariant() switch
            {
                ".pdf" => ExtractPdf(filePath),
                ".docx" => ExtractDocx(filePath),
                ".odt" => ExtractOdt(filePath),
                ".txt" => File.ReadAllText(filePath),
                _ => throw new NotSupportedException($"File type {extension} is not supported for text extraction.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from {FilePath}", filePath);
            throw; // Re-throw to allow caller to handle or let it bubble up
        }
    }

    private string ExtractPdf(string filePath)
    {
        using var pdf = PdfDocument.Open(filePath);
        var builder = new StringBuilder();
        foreach (var page in pdf.GetPages())
        {
            builder.AppendLine(page.Text);
        }
        return builder.ToString();
    }

    private string ExtractDocx(string filePath)
    {
        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document.Body;
        return body?.InnerText ?? string.Empty;
    }

    private string ExtractOdt(string filePath)
    {
        // ODT is a ZIP. Content is in content.xml.
        using var archive = ZipFile.OpenRead(filePath);
        var contentEntry = archive.GetEntry("content.xml");
        if (contentEntry == null) return string.Empty;

        using var stream = contentEntry.Open();
        using var reader = new StreamReader(stream);
        var xmlContent = reader.ReadToEnd();

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);

        return xmlDoc.InnerText;
    }
}
