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
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

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
        using var pdf = PdfDocument.Open(stream);
        var builder = new StringBuilder();
        foreach (var page in pdf.GetPages())
        {
            // Use ContentOrderTextExtractor to respect visual reading order and line breaks
            var text = ContentOrderTextExtractor.GetText(page);
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine(text);
                // Extra newline to separate pages
                builder.AppendLine();
            }
        }
        return builder.ToString().TrimEnd();
    }

    private string ExtractDocx(Stream stream)
    {
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document.Body;
        if (body == null) return string.Empty;

        var builder = new StringBuilder();
        // Iterate through all paragraphs
        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            var paragraphBuilder = new StringBuilder();
            // Iterate through runs and their child elements to catch <w:br/> (soft breaks)
            foreach (var run in paragraph.Elements<Run>())
            {
                foreach (var child in run.ChildElements)
                {
                    if (child is Text t)
                    {
                        paragraphBuilder.Append(t.Text);
                    }
                    else if (child is Break)
                    {
                        paragraphBuilder.AppendLine();
                    }
                }
            }
            
            var text = paragraphBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine(text);
            }
            else if (paragraph.Elements<Run>().Any())
            {
                // Ensure empty/whitespace paragraphs that might contain spacing are handled if needed
                // but usually we just want text content with breaks.
                builder.AppendLine();
            }
        }
        return builder.ToString().TrimEnd();
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

        // ODT paragraphs are text:p. We need a NamespaceManager to find them properly or use inner text if we can find a better way.
        // For simplicity and robustness, let's use the local name "p" as a fallback.
        var builder = new StringBuilder();
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
        
        var paragraphs = xmlDoc.SelectNodes("//text:p", nsmgr);
        if (paragraphs != null)
        {
            foreach (XmlNode p in paragraphs)
            {
                if (!string.IsNullOrWhiteSpace(p.InnerText))
                {
                    builder.AppendLine(p.InnerText);
                }
            }
        }
        else 
        {
            // Fallback if namespaces differ
            return xmlDoc.InnerText;
        }

        return builder.ToString().TrimEnd();
    }
}
