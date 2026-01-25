using System.Threading.Tasks;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface ITextExtractor
{
    /// <summary>
    /// Extracts text content from a file.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <param name="fileType">File extension (e.g. .pdf, .docx).</param>
    /// <returns>Extracted text.</returns>
    string ExtractText(string filePath, string fileType);
}
