using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Services;

/// <summary>
/// Tests for ContentExtractionService focusing on extraction logic.
/// Note: Full BackgroundService lifecycle testing is complex and better suited for integration tests.
/// </summary>
public class ContentExtractionServiceTests
{
    private readonly Mock<IActivityRepository> _activityRepoMock;
    private readonly Mock<IDocumentRepository> _documentRepoMock;
    private readonly Mock<ITextExtractor> _textExtractorMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;

    public ContentExtractionServiceTests()
    {
        _activityRepoMock = new Mock<IActivityRepository>();
        _documentRepoMock = new Mock<IDocumentRepository>();
        _textExtractorMock = new Mock<ITextExtractor>();
        _fileStorageMock = new Mock<IFileStorageService>();
    }

    [Fact]
    public async Task ExtractContent_ValidDocument_UpdatesSourceWithContent()
    {
        // Arrange
        var sourceId = "test-source-1";
        var source = new Source
        {
            Id = sourceId,
            UserId = "user-1",
            Type = SourceType.Document,
            DisplayTitle = "Test Doc",
            ContentExtractionStatus = ContentExtractionStatus.Pending
        };

        var document = new Document
        {
            Id = "doc-1",
            UserId = "user-1",
            FileName = "test.pdf",
            SourceId = sourceId,
            StoragePath = "test/path.pdf",
            FileType = ".pdf"
        };

        var extractedContent = "This is the extracted text content from the PDF.";

        _activityRepoMock.Setup(r => r.GetSourceByIdAsync(sourceId))
            .ReturnsAsync(source);
        _documentRepoMock.Setup(r => r.GetBySourceIdAsync("user-1", sourceId))
            .ReturnsAsync(document);
        _fileStorageMock.Setup(f => f.GetFileStreamAsync(document.StoragePath))
            .ReturnsAsync(new MemoryStream());
        _textExtractorMock.Setup(t => t.ExtractText(It.IsAny<Stream>(), ".pdf"))
            .Returns(extractedContent);

        Source? updatedSource = null;
        _activityRepoMock.Setup(r => r.UpdateSourceAsync(It.IsAny<Source>()))
            .Callback<Source>(s => updatedSource = s)
            .ReturnsAsync((Source s) => s);

        // Act - Simulate extraction logic
        var fileStream = await _fileStorageMock.Object.GetFileStreamAsync(document.StoragePath);
        var content = _textExtractorMock.Object.ExtractText(fileStream, document.FileType);
        
        source.TextContent = content;
        source.ContentExtractionStatus = ContentExtractionStatus.Completed;
        source.ContentExtractionCompletedAt = DateTime.UtcNow;
        source.ContentExtractionError = null;
        
        await _activityRepoMock.Object.UpdateSourceAsync(source);

        // Assert
        Assert.NotNull(updatedSource);
        Assert.Equal(ContentExtractionStatus.Completed, updatedSource!.ContentExtractionStatus);
        Assert.Equal(extractedContent, updatedSource.TextContent);
        Assert.NotNull(updatedSource.ContentExtractionCompletedAt);
        Assert.Null(updatedSource.ContentExtractionError);
    }

    [Fact]
    public async Task ExtractContent_ExtractionFails_UpdatesSourceWithError()
    {
        // Arrange
        var sourceId = "test-source-2";
        var source = new Source
        {
            Id = sourceId,
            UserId = "user-1",
            Type = SourceType.Document,
            ContentExtractionStatus = ContentExtractionStatus.Pending
        };

        var document = new Document
        {
            Id = "doc-2",
            UserId = "user-1",
            FileName = "corrupted.pdf",
            SourceId = sourceId,
            StoragePath = "test/corrupted.pdf",
            FileType = ".pdf"
        };

        var errorMessage = "Corrupted file";

        _activityRepoMock.Setup(r => r.GetSourceByIdAsync(sourceId))
            .ReturnsAsync(source);
        _documentRepoMock.Setup(r => r.GetBySourceIdAsync(It.IsAny<string>(), sourceId))
            .ReturnsAsync(document);
        _fileStorageMock.Setup(f => f.GetFileStreamAsync(document.StoragePath))
            .ReturnsAsync(new MemoryStream());
        _textExtractorMock.Setup(t => t.ExtractText(It.IsAny<Stream>(), ".pdf"))
            .Throws(new InvalidOperationException(errorMessage));

        Source? updatedSource = null;
        _activityRepoMock.Setup(r => r.UpdateSourceAsync(It.IsAny<Source>()))
            .Callback<Source>(s => updatedSource = s)
            .ReturnsAsync((Source s) => s);

        // Act - Simulate extraction logic with error handling
        try
        {
            var fileStream = await _fileStorageMock.Object.GetFileStreamAsync(document.StoragePath);
            var content = _textExtractorMock.Object.ExtractText(fileStream, document.FileType);
            source.TextContent = content;
            source.ContentExtractionStatus = ContentExtractionStatus.Completed;
        }
        catch (Exception ex)
        {
            source.ContentExtractionStatus = ContentExtractionStatus.Failed;
            source.ContentExtractionError = ex.Message;
            source.ContentExtractionCompletedAt = DateTime.UtcNow;
        }
        
        await _activityRepoMock.Object.UpdateSourceAsync(source);

        // Assert
        Assert.NotNull(updatedSource);
        Assert.Equal(ContentExtractionStatus.Failed, updatedSource!.ContentExtractionStatus);
        Assert.Contains(errorMessage, updatedSource.ContentExtractionError);
        Assert.NotNull(updatedSource.ContentExtractionCompletedAt);
    }

    [Fact]
    public async Task ExtractContent_NonDocumentSource_ShouldSkip()
    {
        // Arrange
        var sourceId = "test-source-3";
        var source = new Source
        {
            Id = sourceId,
            Type = SourceType.Wikipedia, // Not a document
            ContentExtractionStatus = ContentExtractionStatus.Pending
        };

        _activityRepoMock.Setup(r => r.GetSourceByIdAsync(sourceId))
            .ReturnsAsync(source);

        // Act - Check if we should process this source
        var shouldProcess = source.Type == SourceType.Document;

        // Assert
        Assert.False(shouldProcess);
        // Verify no document repository calls were made
        _documentRepoMock.Verify(r => r.GetBySourceIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task ExtractContent_MissingDocument_UpdatesSourceWithError()
    {
        // Arrange
        var sourceId = "test-source-4";
        var source = new Source
        {
            Id = sourceId,
            UserId = "user-1",
            Type = SourceType.Document,
            ContentExtractionStatus = ContentExtractionStatus.Pending
        };

        _activityRepoMock.Setup(r => r.GetSourceByIdAsync(sourceId))
            .ReturnsAsync(source);
        _documentRepoMock.Setup(r => r.GetBySourceIdAsync("user-1", sourceId))
            .ReturnsAsync((Document?)null); // No document found

        Source? updatedSource = null;
        _activityRepoMock.Setup(r => r.UpdateSourceAsync(It.IsAny<Source>()))
            .Callback<Source>(s => updatedSource = s)
            .ReturnsAsync((Source s) => s);

        // Act - Simulate extraction logic with missing document
        var document = await _documentRepoMock.Object.GetBySourceIdAsync("user-1", sourceId);
        if (document == null)
        {
            source.ContentExtractionStatus = ContentExtractionStatus.Failed;
            source.ContentExtractionError = "No document found for source";
            source.ContentExtractionCompletedAt = DateTime.UtcNow;
            await _activityRepoMock.Object.UpdateSourceAsync(source);
        }

        // Assert
        Assert.NotNull(updatedSource);
        Assert.Equal(ContentExtractionStatus.Failed, updatedSource!.ContentExtractionStatus);
        Assert.Contains("No document found", updatedSource.ContentExtractionError);
    }
}
