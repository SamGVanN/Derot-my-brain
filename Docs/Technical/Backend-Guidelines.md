# Backend Architecture & Guidelines

This document outlines the backend development guidelines, architectural patterns, and technical standards for the Derot My Brain project.

## Table of Contents

1. [Technology Stack](#technology-stack)
2. [Architecture Patterns](#architecture-patterns)
3. [API Design](#api-design)
4. [Logging Strategy](#logging-strategy)
5. [Error Handling](#error-handling)
6. [Testing](#testing)

---

## Technology Stack

### Core Technologies

- **.NET 9** - Latest LTS version
- **ASP.NET Core Web API** - RESTful API framework
- **C# 13** - Latest language features
- **Serilog** - Structured logging
- **SQLite + Entity Framework Core** - Embedded database for V1 (See [Storage-Policy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Storage-Policy.md))
  - **Decision Date:** 2026-01-20
  - **Rationale:** Dashboard-ready, avoids technical debt, maintains portability

### Testing

- **xUnit** - Unit testing framework
- **Moq** - Mocking framework
- **EF Core InMemory** - For testing database operations
- See [Testing-Strategy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Testing-Strategy.md) for details

---

## Architecture Patterns

The backend follows **SOLID principles** and **Clean Architecture** patterns.

### 1. SOLID Principles

#### Single Responsibility Principle (SRP)
Each class should have one, and only one, reason to change.

```csharp
// ✅ GOOD: Single responsibility
public class UserRepository
{
    public Task<User> GetByIdAsync(string id) { ... }
    public Task SaveAsync(User user) { ... }
}

public class UserService
{
    public Task<User> CreateUserAsync(CreateUserDto dto) { ... }
    public Task UpdatePreferencesAsync(string userId, UserPreferences prefs) { ... }
}
```

#### Open/Closed Principle (OCP)
Software entities should be open for extension, but closed for modification.

```csharp
// ✅ GOOD: Use interfaces for extensibility
public interface IActivityRepository
{
    Task<IEnumerable<UserActivity>> GetAllAsync(string userId);
    Task<UserActivity> CreateAsync(UserActivity activity);
}

public class SqliteActivityRepository : IActivityRepository { ... }
// Future: Can add different implementations without modifying existing code
```

#### Liskov Substitution Principle (LSP)
Derived classes must be substitutable for their base classes.

#### Interface Segregation Principle (ISP)
Clients should not be forced to depend on interfaces they don't use.

```csharp
// ✅ GOOD: Specific interfaces
public interface IUserReader
{
    Task<User> GetByIdAsync(string id);
}

public interface IUserWriter
{
    Task SaveAsync(User user);
}
```

#### Dependency Inversion Principle (DIP)
Depend on abstractions, not concretions.

```csharp
// ✅ GOOD: Depend on interface
public class UserService
{
    private readonly IUserRepository _repository;
    
    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }
}
```

### 2. Repository Pattern

**All data access goes through repositories.**

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<IEnumerable<User>> GetAllAsync();
    Task SaveAsync(User user);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

public class SqliteUserRepository : IUserRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteUserRepository> _logger;
    
    public SqliteUserRepository(DerotDbContext context, ILogger<SqliteUserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<User?> GetByIdAsync(string id)
    {
        try
        {
            return await _context.Users
                .Include(u => u.Preferences)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user {UserId}", id);
            throw;
        }
    }
}
```

**Key Points:**
- Repositories handle data persistence logic
- Use dependency injection for DbContext
- Log all data access operations
- Handle exceptions appropriately
- Use async/await with EF Core methods

### 3. Entity Framework Core (V1)

**SQLite + EF Core is used for V1 data storage.**

#### DbContext Configuration

```csharp
public class DerotDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }
    public DbSet<UserActivity> Activities { get; set; }
    public DbSet<TrackedTopic> TrackedTopics { get; set; }
    
    public DerotDbContext(DbContextOptions<DerotDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure entities
        modelBuilder.Entity<UserActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            
            // Indexes for performance
            entity.HasIndex(e => new { e.UserId, e.LastAttemptDate });
            entity.HasIndex(e => new { e.UserId, e.IsTracked });
            
            // Foreign key
            entity.HasOne(e => e.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

#### Repository Implementation with EF Core

```csharp
public class SqliteActivityRepository : IActivityRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteActivityRepository> _logger;
    
    public SqliteActivityRepository(DerotDbContext context, ILogger<SqliteActivityRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<IEnumerable<UserActivity>> GetAllAsync(string userId)
    {
        return await _context.Activities
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.LastAttemptDate)
            .ToListAsync();
    }
    
    public async Task<UserActivity> CreateAsync(UserActivity activity)
    {
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }
}
```

**EF Core Best Practices:**
- Always use `async/await` with `ToListAsync()`, `FirstOrDefaultAsync()`, etc.
- Use `AsNoTracking()` for read-only queries to improve performance
- Configure indexes in `OnModelCreating` for frequently queried fields
- Use foreign keys and cascade delete for data integrity
- Never use `.Result` or `.Wait()` - always async
- Use `DbContext` scoped lifetime (default in ASP.NET Core)

### 4. Service Layer

**Business logic belongs in services, not controllers.**

```csharp
public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserDto dto);
    Task<User> GetUserByIdAsync(string id);
    Task UpdatePreferencesAsync(string userId, UserPreferences preferences);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    
    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        _logger.LogInformation("Creating new user: {UserName}", dto.Name);
        
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow,
            Preferences = UserPreferences.Default()
        };
        
        await _userRepository.SaveAsync(user);
        
        _logger.LogInformation("User created successfully: {UserId}", user.Id);
        return user;
    }
}
```

**Key Points:**
- Services orchestrate business logic
- Services call repositories for data access
- Services handle validation and business rules
- Services are injected into controllers

### 4. Controller Layer

**Controllers are thin - they delegate to services.**

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var user = await _userService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
}
```

**Key Points:**
- Controllers handle HTTP concerns only
- Delegate business logic to services
- Return appropriate HTTP status codes
- Use DTOs for request/response models
- Validate input using ModelState

---

## API Design

### 1. RESTful Conventions

**Follow REST principles for API design:**

- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

**Nested Resources:**
- `GET /api/users/{id}/history` - Get user's history
- `POST /api/users/{id}/preferences` - Update user preferences

### 2. Response Formats

**Success Response:**
```json
{
  "id": "user-123",
  "name": "John Doe",
  "preferences": {
    "language": "en",
    "theme": "dark"
  }
}
```

**Error Response:**
```json
{
  "message": "User not found",
  "statusCode": 404,
  "timestamp": "2026-01-19T23:30:00Z"
}
```

### 3. Status Codes

- `200 OK` - Successful GET, PUT, PATCH
- `201 Created` - Successful POST
- `204 No Content` - Successful DELETE
- `400 Bad Request` - Invalid input
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

### 4. Validation

**Use Data Annotations for validation:**

```csharp
public class CreateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
}
```

**Validate in controller:**
```csharp
if (!ModelState.IsValid)
    return BadRequest(ModelState);
```

---

## Logging Strategy

### 1. Serilog Configuration

**Configure Serilog in `Program.cs`:**

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/derot-my-brain-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### 2. Log Levels

- **Trace**: Very detailed, typically only enabled in development
- **Debug**: Detailed information for debugging
- **Information**: General informational messages (default)
- **Warning**: Warning messages for unexpected but handled situations
- **Error**: Error messages for failures
- **Fatal**: Critical failures requiring immediate attention

### 3. Structured Logging

**Use structured logging with named properties:**

```csharp
// ✅ GOOD: Structured logging
_logger.LogInformation("User {UserId} logged in at {LoginTime}", userId, DateTime.UtcNow);

// ❌ BAD: String interpolation
_logger.LogInformation($"User {userId} logged in at {DateTime.UtcNow}");
```

### 4. What to Log

**DO Log:**
- Application startup/shutdown
- User actions (login, logout, data changes)
- API requests (endpoint, user, timestamp)
- Errors and exceptions
- Performance metrics (if needed)

**DON'T Log:**
- Sensitive data (passwords, tokens)
- Excessive detail in production
- Personal identifiable information (PII) without consent

---

## Error Handling

### 1. Custom Exceptions

**Create domain-specific exceptions:**

```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
```

### 2. Global Exception Handler

**Use middleware for global exception handling:**

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { message = "Internal server error" });
        }
    }
}
```

### 3. Controller Exception Handling

**Handle exceptions in controllers when appropriate:**

```csharp
try
{
    var user = await _userService.GetUserByIdAsync(id);
    return Ok(user);
}
catch (NotFoundException ex)
{
    return NotFound(new { message = ex.Message });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error getting user");
    return StatusCode(500, new { message = "Internal server error" });
}
```

---

## Testing

See [Testing-Strategy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Testing-Strategy.md) for comprehensive testing guidelines.

**Quick Reference:**

- Use **xUnit** for unit tests
- Use **Moq** for mocking dependencies
- Follow **TDD** (Red-Green-Refactor)
- Aim for **≥80% code coverage**
- Test edge cases and error scenarios

---

## Code Standards

### 1. Naming Conventions

- **Classes**: PascalCase (`UserService`)
- **Methods**: PascalCase (`GetUserById`)
- **Variables**: camelCase (`userId`)
- **Constants**: PascalCase (`MaxRetryCount`)
- **Interfaces**: PascalCase with `I` prefix (`IUserRepository`)

### 2. Async/Await

**Always use async/await, never `.Result` or `.Wait()`:**

```csharp
// ✅ GOOD
public async Task<User> GetUserAsync(string id)
{
    return await _repository.GetByIdAsync(id);
}

// ❌ BAD
public User GetUser(string id)
{
    return _repository.GetByIdAsync(id).Result; // Deadlock risk!
}
```

### 3. XML Documentation

**Document public APIs:**

```csharp
/// <summary>
/// Gets a user by their unique identifier.
/// </summary>
/// <param name="id">The user's unique identifier.</param>
/// <returns>The user if found, null otherwise.</returns>
/// <exception cref="NotFoundException">Thrown when user is not found.</exception>
public async Task<User?> GetUserByIdAsync(string id)
{
    // Implementation
}
```

### 4. Dependency Injection

**Register services in `Program.cs`:**

```csharp
// Register DbContext with SQLite
builder.Services.AddDbContext<DerotDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and services
builder.Services.AddScoped<IUserRepository, SqliteUserRepository>();
builder.Services.AddScoped<IActivityRepository, SqliteActivityRepository>();
builder.Services.AddScoped<IUserService, UserService>();
```

**Lifetimes:**
- **Transient**: Created each time requested
- **Scoped**: Created once per request
- **Singleton**: Created once for application lifetime

---

## Related Documentation

- [Frontend-Architecture.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Frontend-Architecture.md) - Frontend patterns and guidelines
- [Storage-Policy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Storage-Policy.md) - Storage constraints and alternatives
- [Testing-Strategy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Testing-Strategy.md) - Testing approach and standards
- [Architecture.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Architecture.md) - System architecture overview
