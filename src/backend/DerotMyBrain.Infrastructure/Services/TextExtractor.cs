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

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ExtractText(stream, extension);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from file {FilePath}", filePath);
            throw; 
        }
    }

    public string ExtractText(Stream fileStream, string extension)
    {
        try
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => ExtractPdf(fileStream),
                ".docx" => ExtractDocx(fileStream),
                ".odt" => ExtractOdt(fileStream),
                ".txt" => ExtractTxt(fileStream),
                _ => throw new NotSupportedException($"File type {extension} is not supported for text extraction.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from stream with extension {Extension}", extension);
            throw;
        }
    }

    private string ExtractTxt(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var text = reader.ReadToEnd();
        // Reset position if needed, though usually we consume it once.
        return text;
    }

    private string ExtractPdf(Stream stream)
    {
        using var pdf = PdfDocument.Open(stream); // PdfPig supports stream
        var builder = new StringBuilder();
        foreach (var page in pdf.GetPages())
        {
            builder.AppendLine(page.Text);
        }
        return builder.ToString();
    }

    private string ExtractDocx(Stream stream)
    {
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document.Body;
        return body?.InnerText ?? string.Empty;
    }

    private string ExtractOdt(Stream stream)
    {
        // ODT is a ZIP.
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var contentEntry = archive.GetEntry("content.xml");
        if (contentEntry == null) return string.Empty;

        using var entryStream = contentEntry.Open();
        using var reader = new StreamReader(entryStream);
        var xmlContent = reader.ReadToEnd();

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);

        return xmlDoc.InnerText;
    }
}
