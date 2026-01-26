using System.IO;
using System.Threading.Tasks;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IDocumentService
{
    /// <summary>
    /// Uploads a new document, saves it to storage, and extractions text.
    /// </summary>
    /// <param name="userId">The owner of the document.</param>
    /// <param name="fileName">Original filename.</param>
    /// <param name="fileStream">Stream of the file content.</param>
    /// <param name="contentType">MIME type or file extension.</param>
    /// <returns>The created Document entity.</returns>
    Task<Document> UploadDocumentAsync(string userId, string fileName, Stream fileStream, string contentType);

    /// <summary>
    /// Gets all documents for a user.
    /// </summary>
    Task<IEnumerable<Document>> GetUserDocumentsAsync(string userId);

    /// <summary>
    /// Deletes a document and its file from storage.
    /// </summary>
    Task DeleteDocumentAsync(string userId, string documentId);
    
    /// <summary>
    /// Gets the full text content of a document by its source id.
    /// </summary>
    Task<string> GetDocumentContentAsync(string userId, string sourceId);
}
