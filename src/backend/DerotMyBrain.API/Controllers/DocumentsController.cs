using System.IO;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers;

[Authorize]
[ApiController]
[Route("api/users/{userId}/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IDocumentService documentService,
        IFileStorageService fileStorageService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments(string userId)
    {
        // Simple security check: ensure user accessing matches token
        // (Middleware usually handles this, or consistent policy)
        
        var docs = await _documentService.GetUserDocumentsAsync(userId);
        var dtos = docs.Select(d => new DocumentDto
        {
            Id = d.Id,
            UserId = d.UserId,
            FileName = d.FileName,
            FileType = d.FileType,
            FileSize = d.FileSize,
            UploadDate = d.UploadDate,
            DisplayTitle = d.DisplayTitle,
            SourceId = d.SourceId,
            StoragePath = Path.GetDirectoryName(_fileStorageService.GetAbsolutePath(d.StoragePath)) ?? string.Empty
        });

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<DocumentDto>> UploadDocument(string userId, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        try
        {
            using var stream = file.OpenReadStream();
            var doc = await _documentService.UploadDocumentAsync(userId, file.FileName, stream, file.ContentType);
            
            var dto = new DocumentDto
            {
                Id = doc.Id,
                UserId = doc.UserId,
                FileName = doc.FileName,
                FileType = doc.FileType,
                FileSize = doc.FileSize,
                UploadDate = doc.UploadDate,
                DisplayTitle = doc.DisplayTitle,
                SourceId = doc.SourceId,
                StoragePath = Path.GetDirectoryName(_fileStorageService.GetAbsolutePath(doc.StoragePath)) ?? string.Empty
            };

            return CreatedAtAction(nameof(GetDocuments), new { userId = userId }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for user {UserId}", userId);
            return StatusCode(500, "Internal server error during upload.");
        }
    }

    [HttpDelete("{documentId}")]
    public async Task<IActionResult> DeleteDocument(string userId, string documentId)
    {
        await _documentService.DeleteDocumentAsync(userId, documentId);
        return NoContent();
    }
}
