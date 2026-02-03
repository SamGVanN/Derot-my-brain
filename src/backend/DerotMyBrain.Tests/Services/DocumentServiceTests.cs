using System;
using System.IO;
using System.Threading.Tasks;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Services;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepoMock;
    private readonly Mock<ITextExtractor> _textExtractorMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<ISourceService> _sourceServiceMock;
    private readonly Mock<IContentExtractionQueue> _extractionQueueMock;
    private readonly Mock<ILogger<DocumentService>> _loggerMock;
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        _documentRepoMock = new Mock<IDocumentRepository>();
        _textExtractorMock = new Mock<ITextExtractor>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _sourceServiceMock = new Mock<ISourceService>();
        _extractionQueueMock = new Mock<IContentExtractionQueue>();
        _loggerMock = new Mock<ILogger<DocumentService>>();

        _service = new DocumentService(
            _documentRepoMock.Object,
            _textExtractorMock.Object,
            _fileStorageMock.Object,
            _sourceServiceMock.Object,
            _extractionQueueMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task UploadDocumentAsync_ValidFile_SetsStatusToPending()
    {
        // Arrange
        var userId = "user-1";
        var fileName = "test.pdf";
        var fileStream = new MemoryStream();
        var contentType = "application/pdf";
        var storagePath = "uploads/user-1/test.pdf";

        var source = new Source
        {
            Id = "source-1",
            UserId = userId,
            Type = SourceType.Document,
            DisplayTitle = "test",
            ContentExtractionStatus = ContentExtractionStatus.Completed // Initial state
        };

        _fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<Stream>(), fileName, userId))
            .ReturnsAsync(storagePath);
        _sourceServiceMock.Setup(s => s.GetOrCreateSourceAsync(userId, "test", It.IsAny<string>(), SourceType.Document))
            .ReturnsAsync(source);
        _sourceServiceMock.Setup(s => s.UpdateSourceAsync(It.IsAny<Source>()))
            .Returns(Task.CompletedTask);
        _documentRepoMock.Setup(r => r.CreateAsync(It.IsAny<Document>()))
            .ReturnsAsync((Document d) => d);

        // Act
        var result = await _service.UploadDocumentAsync(userId, fileName, fileStream, contentType);

        // Assert
        _sourceServiceMock.Verify(s => s.UpdateSourceAsync(It.Is<Source>(src =>
            src.ContentExtractionStatus == ContentExtractionStatus.Pending &&
            src.ContentExtractionError == null &&
            src.ContentExtractionCompletedAt == null
        )), Times.Once());
    }

    [Fact]
    public async Task UploadDocumentAsync_ValidFile_QueuesExtraction()
    {
        // Arrange
        var userId = "user-1";
        var fileName = "test.pdf";
        var fileStream = new MemoryStream();
        var contentType = "application/pdf";
        var storagePath = "uploads/user-1/test.pdf";

        var source = new Source
        {
            Id = "source-1",
            UserId = userId,
            Type = SourceType.Document,
            DisplayTitle = "test"
        };

        _fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<Stream>(), fileName, userId))
            .ReturnsAsync(storagePath);
        _sourceServiceMock.Setup(s => s.GetOrCreateSourceAsync(userId, "test", It.IsAny<string>(), SourceType.Document))
            .ReturnsAsync(source);
        _sourceServiceMock.Setup(s => s.UpdateSourceAsync(It.IsAny<Source>()))
            .Returns(Task.CompletedTask);
        _documentRepoMock.Setup(r => r.CreateAsync(It.IsAny<Document>()))
            .ReturnsAsync((Document d) => d);

        // Act
        var result = await _service.UploadDocumentAsync(userId, fileName, fileStream, contentType);

        // Assert
        _extractionQueueMock.Verify(q => q.QueueExtraction(source.Id), Times.Once());
    }

    [Fact]
    public async Task UploadDocumentAsync_ValidFile_ReturnsImmediately()
    {
        // Arrange
        var userId = "user-1";
        var fileName = "test.pdf";
        var fileStream = new MemoryStream();
        var contentType = "application/pdf";
        var storagePath = "uploads/user-1/test.pdf";

        var source = new Source
        {
            Id = "source-1",
            UserId = userId,
            Type = SourceType.Document,
            DisplayTitle = "test"
        };

        _fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<Stream>(), fileName, userId))
            .ReturnsAsync(storagePath);
        _sourceServiceMock.Setup(s => s.GetOrCreateSourceAsync(userId, "test", It.IsAny<string>(), SourceType.Document))
            .ReturnsAsync(source);
        _sourceServiceMock.Setup(s => s.UpdateSourceAsync(It.IsAny<Source>()))
            .Returns(Task.CompletedTask);
        _documentRepoMock.Setup(r => r.CreateAsync(It.IsAny<Document>()))
            .ReturnsAsync((Document d) => d);

        // Act
        var startTime = DateTime.UtcNow;
        var result = await _service.UploadDocumentAsync(userId, fileName, fileStream, contentType);
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert - Should return quickly (< 1 second) without waiting for extraction
        Assert.True(duration.TotalSeconds < 1, "Upload should return immediately without waiting for extraction");
        Assert.NotNull(result);
        Assert.Equal(fileName, result.FileName);
        
        // Verify text extractor was NOT called during upload
        _textExtractorMock.Verify(t => t.ExtractText(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task UploadDocumentAsync_ClearsExistingError_OnReupload()
    {
        // Arrange
        var userId = "user-1";
        var fileName = "test.pdf";
        var fileStream = new MemoryStream();
        var contentType = "application/pdf";
        var storagePath = "uploads/user-1/test.pdf";

        var source = new Source
        {
            Id = "source-1",
            UserId = userId,
            Type = SourceType.Document,
            DisplayTitle = "test",
            ContentExtractionStatus = ContentExtractionStatus.Failed,
            ContentExtractionError = "Previous extraction failed",
            ContentExtractionCompletedAt = DateTime.UtcNow.AddHours(-1)
        };

        _fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<Stream>(), fileName, userId))
            .ReturnsAsync(storagePath);
        _sourceServiceMock.Setup(s => s.GetOrCreateSourceAsync(userId, "test", It.IsAny<string>(), SourceType.Document))
            .ReturnsAsync(source);
        _sourceServiceMock.Setup(s => s.UpdateSourceAsync(It.IsAny<Source>()))
            .Returns(Task.CompletedTask);
        _documentRepoMock.Setup(r => r.CreateAsync(It.IsAny<Document>()))
            .ReturnsAsync((Document d) => d);

        // Act
        var result = await _service.UploadDocumentAsync(userId, fileName, fileStream, contentType);

        // Assert - Should clear previous error and reset status
        _sourceServiceMock.Verify(s => s.UpdateSourceAsync(It.Is<Source>(src =>
            src.ContentExtractionStatus == ContentExtractionStatus.Pending &&
            src.ContentExtractionError == null &&
            src.ContentExtractionCompletedAt == null
        )), Times.Once());
    }
}
