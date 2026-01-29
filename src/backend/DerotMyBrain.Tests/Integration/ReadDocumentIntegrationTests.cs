using System.Text;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Interfaces.Utils;
using DerotMyBrain.Core.Services;
using DerotMyBrain.Infrastructure.Data;
using DerotMyBrain.Infrastructure.Repositories;
using DerotMyBrain.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Integration;

public class ReadDocumentIntegrationTests
{
    private readonly DerotDbContext _context;
    private readonly IActivityService _activityService;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly string _userId = "test-user-id";

    public ReadDocumentIntegrationTests()
    {
        // 1. Setup In-Memory DB
        var options = new DbContextOptionsBuilder<DerotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new DerotDbContext(options);

        // 2. Setup Repositories
        var activityRepository = new SqliteActivityRepository(_context, new NullLogger<SqliteActivityRepository>());
        var documentRepository = new SqliteDocumentRepository(_context, new NullLogger<SqliteDocumentRepository>());
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new User { Id = _userId, Name = "Test User" });

        // 3. Setup File Content Source Dependencies
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        var textExtractor = new TextExtractor(new NullLogger<TextExtractor>());
        
        var fileContentSource = new FileContentSource(
            new NullLogger<FileContentSource>(),
            textExtractor,
            _fileStorageServiceMock.Object,
            documentRepository
        );

        var contentSources = new List<IContentSource> { fileContentSource };

        // 4. Setup Activity Service
        _activityService = new ActivityService(
            activityRepository,
            userRepository.Object,
            contentSources,
            new Mock<IWikipediaService>().Object,
            new Mock<ILlmService>().Object,
            new Mock<IJsonSerializer>().Object,
            new NullLogger<ActivityService>()
        );
    }

    [Fact]
    public async Task ReadAsync_Should_ExtractContent_And_SaveToActivity()
    {
        // Arrange: Create Source and Document in DB
        var sourceId = "test-source-id";
        var storagePath = "test/path/doc.txt";
        var expectedContent = "This is the content of the document.";

        var source = new Source 
        { 
            Id = sourceId, 
            UserId = _userId, 
            Type = SourceType.Document, 
            ExternalId = "file://" + storagePath,
            DisplayTitle = "Test Doc"
        };
        _context.Sources.Add(source);

        var document = new Document
        {
            Id = Guid.NewGuid().ToString(),
            UserId = _userId,
            SourceId = sourceId,
            FileName = "doc.txt",
            FileType = ".txt",
            StoragePath = storagePath,
            UploadDate = DateTime.UtcNow
        };
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        // Arrange: Mock File Storage to return stream
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));
        _fileStorageServiceMock.Setup(x => x.GetFileStreamAsync(storagePath))
            .ReturnsAsync(stream);

        // Act
        var result = await _activityService.ReadAsync(
            _userId,
            "Test Read",
            "en",
            sourceId,
            SourceType.Document
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedContent, result.ArticleContent);
        Assert.True(result.ArticleContent.Length > 0);

        // Verify DB Persistence
        var activityInDb = await _context.Activities
            .Include(a => a.Source)
            .FirstOrDefaultAsync(a => a.Id == result.Id);

        Assert.NotNull(activityInDb);
        Assert.Equal(expectedContent, activityInDb.ArticleContent);
        // Assert correct Source association
        Assert.Equal(SourceType.Document, activityInDb.Source.Type);
        Assert.Equal(sourceId, activityInDb.SourceId);
    }
}
