# Testing Strategy

This document outlines the testing approach, methodologies, and standards for the Derot My Brain project.

## Table of Contents

1. [Testing Philosophy](#testing-philosophy)
2. [TDD Methodology](#tdd-methodology)
3. [Backend Testing](#backend-testing)
4. [Frontend Testing](#frontend-testing)
5. [Mock Data Strategy](#mock-data-strategy)
6. [Coverage Requirements](#coverage-requirements)

---

## Testing Philosophy

### Core Principles

1. **Test-Driven Development (TDD)** - Write tests before implementation
2. **Comprehensive Coverage** - Aim for ≥80% code coverage
3. **Test Pyramid** - More unit tests, fewer integration tests, minimal E2E tests
4. **Fast Feedback** - Tests should run quickly
5. **Maintainable Tests** - Tests are code too, keep them clean

### Test Pyramid

```
        /\
       /E2E\         <- Few (Browser tests, full workflows)
      /------\
     /  Integ \      <- Some (API tests, component integration)
    /----------\
   / Unit Tests \    <- Many (Functions, hooks, services)
  /--------------\
```

---

## TDD Methodology

Follow the **Red-Green-Refactor** cycle for all new features.

### 1. 🔴 RED PHASE - Write Failing Tests

**Define test cases first:**
- Nominal cases (happy path)
- Edge cases (boundaries, empty data)
- Error cases (invalid input, failures)

**Write tests that fail:**
```csharp
// Backend example
[Fact]
public async Task GetUserById_ShouldReturnUser_WhenUserExists()
{
    // Arrange
    var userId = "test-user-id-001";
    
    // Act
    var result = await _userService.GetUserByIdAsync(userId);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(userId, result.Id);
}
```

```typescript
// Frontend example
it('should login user successfully', async () => {
    const { result } = renderHook(() => useAuth());
    await act(() => result.current.login('test-user-id-001'));
    expect(result.current.isAuthenticated).toBe(true);
});
```

**Verify tests fail** before implementing code.

### 2. 🟢 GREEN PHASE - Implement Minimal Code

**Write the simplest code to make tests pass:**

```csharp
// Backend example
public async Task<User> GetUserByIdAsync(string userId)
{
    return await _repository.GetByIdAsync(userId);
}
```

```typescript
// Frontend example
export const useAuth = () => {
    const login = async (userId: string) => {
        const user = await userApi.getUserById(userId);
        useAuthStore.getState().setUser(user);
    };
    return { login, isAuthenticated: !!useAuthStore.getState().user };
};
```

**Run tests** - they should now pass.

### 3. 🔵 REFACTOR PHASE - Improve Code Quality

**Clean up code while keeping tests green:**

```csharp
// Backend example - Add validation and error handling
public async Task<User> GetUserByIdAsync(string userId)
{
    if (string.IsNullOrEmpty(userId))
        throw new ArgumentException("User ID cannot be null or empty");
    
    var user = await _repository.GetByIdAsync(userId);
    if (user == null)
        throw new NotFoundException($"User with ID {userId} not found");
    
    return user;
}
```

```typescript
// Frontend example - Add error handling
export const useAuth = () => {
    const login = async (userId: string) => {
        try {
            const user = await userApi.getUserById(userId);
            useAuthStore.getState().setUser(user);
            usePreferencesStore.getState().loadPreferences(user.preferences);
        } catch (error) {
            console.error('Login failed:', error);
            throw error;
        }
    };
    return { 
        login, 
        isAuthenticated: !!useAuthStore.getState().user 
    };
};
```

**Run tests again** - ensure they still pass.

### 4. 📊 COVERAGE PHASE - Verify Coverage

**Run coverage tools:**
```bash
# Backend
dotnet test /p:CollectCoverage=true

# Frontend
npm run test:coverage
```

**Verify coverage ≥ 80%** for new code.

---

## Backend Testing

### Technology Stack

- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library (optional)

### 1. Unit Tests

**Test individual methods in isolation:**

```csharp
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _sut; // System Under Test
    
    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _sut = new UserService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateUser_ShouldReturnUser_WhenValidDto()
    {
        // Arrange
        var dto = new CreateUserDto { Name = "John Doe" };
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _sut.CreateUserAsync(dto);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<User>()), Times.Once);
    }
    
    [Fact]
    public async Task GetUserById_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync("invalid-id"))
            .ReturnsAsync((User?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.GetUserByIdAsync("invalid-id")
        );
    }
}
```

### 2. Integration Tests

**Test API endpoints with real dependencies:**

```csharp
public class UsersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public UsersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetUser_ShouldReturn200_WhenUserExists()
    {
        // Arrange
        var userId = "test-user-id-001";
        
        // Act
        var response = await _client.GetAsync($"/api/users/{userId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
    }
}
```

### 3. Repository Tests

**Test data access with JSON files:**

```csharp
public class JsonUserRepositoryTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly JsonUserRepository _repository;
    
    public JsonUserRepositoryTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        
        var storage = new JsonFileStorageProvider(_testDataPath);
        var logger = new Mock<ILogger<JsonUserRepository>>().Object;
        _repository = new JsonUserRepository(storage, logger);
    }
    
    [Fact]
    public async Task SaveAsync_ShouldPersistUser_ToJsonFile()
    {
        // Arrange
        var user = new User { Id = "test-001", Name = "Test User" };
        
        // Act
        await _repository.SaveAsync(user);
        
        // Assert
        var retrieved = await _repository.GetByIdAsync("test-001");
        Assert.NotNull(retrieved);
        Assert.Equal("Test User", retrieved.Name);
    }
    
    public void Dispose()
    {
        Directory.Delete(_testDataPath, true);
    }
}
```

### 4. Test Naming Convention

**Use descriptive test names:**

```
MethodName_Should[ExpectedBehavior]_When[Condition]
```

Examples:
- `CreateUser_ShouldReturnUser_WhenValidDto`
- `GetUserById_ShouldThrowNotFoundException_WhenUserDoesNotExist`
- `SaveAsync_ShouldPersistUser_ToJsonFile`

---

## Frontend Testing

### Technology Stack

- **Vitest** - Testing framework (Vite-native)
- **React Testing Library** - Component testing
- **@testing-library/react-hooks** - Hook testing
- **MSW (Mock Service Worker)** - API mocking (optional)

### 1. Custom Hook Tests

**Test hooks in isolation:**

```typescript
import { renderHook, act } from '@testing-library/react';
import { useAuth } from '@/hooks/useAuth';

describe('useAuth', () => {
    it('should login user successfully', async () => {
        const { result } = renderHook(() => useAuth());
        
        await act(async () => {
            await result.current.login('test-user-id-001');
        });
        
        expect(result.current.isAuthenticated).toBe(true);
        expect(result.current.user).not.toBeNull();
    });
    
    it('should logout user successfully', async () => {
        const { result } = renderHook(() => useAuth());
        
        await act(async () => {
            await result.current.login('test-user-id-001');
            result.current.logout();
        });
        
        expect(result.current.isAuthenticated).toBe(false);
        expect(result.current.user).toBeNull();
    });
});
```

### 2. Component Tests

**Test component rendering and interaction:**

```typescript
import { render, screen, fireEvent } from '@testing-library/react';
import { LoginForm } from '@/components/LoginForm';

describe('LoginForm', () => {
    it('should render login form', () => {
        render(<LoginForm onSubmit={vi.fn()} />);
        
        expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument();
    });
    
    it('should call onSubmit when form is submitted', async () => {
        const mockSubmit = vi.fn();
        render(<LoginForm onSubmit={mockSubmit} />);
        
        const input = screen.getByLabelText(/username/i);
        const button = screen.getByRole('button', { name: /login/i });
        
        fireEvent.change(input, { target: { value: 'test-user' } });
        fireEvent.click(button);
        
        expect(mockSubmit).toHaveBeenCalledWith('test-user');
    });
});
```

### 3. Zustand Store Tests

**Test store logic:**

```typescript
import { useAuthStore } from '@/stores/authStore';

describe('authStore', () => {
    beforeEach(() => {
        // Reset store before each test
        useAuthStore.getState().actions.reset();
    });
    
    it('should set user on login', () => {
        const user = { id: 'test-001', name: 'Test User' };
        
        useAuthStore.getState().actions.onLoginSuccess(user);
        
        expect(useAuthStore.getState().user).toEqual(user);
        expect(useAuthStore.getState().isAuthenticated).toBe(true);
    });
    
    it('should clear user on logout', () => {
        const user = { id: 'test-001', name: 'Test User' };
        useAuthStore.getState().actions.onLoginSuccess(user);
        
        useAuthStore.getState().actions.onLogout();
        
        expect(useAuthStore.getState().user).toBeNull();
        expect(useAuthStore.getState().isAuthenticated).toBe(false);
    });
});
```

### 4. API Service Tests

**Test API calls with mocks:**

```typescript
import { userApi } from '@/api/userApi';
import { vi } from 'vitest';

// Mock axios
vi.mock('@/api/client', () => ({
    apiClient: {
        get: vi.fn(),
        post: vi.fn(),
    }
}));

describe('userApi', () => {
    it('should fetch user by id', async () => {
        const mockUser = { id: 'test-001', name: 'Test User' };
        apiClient.get.mockResolvedValue({ data: mockUser });
        
        const result = await userApi.getUserById('test-001');
        
        expect(result).toEqual(mockUser);
        expect(apiClient.get).toHaveBeenCalledWith('/users/test-001');
    });
});
```

### 5. Test Naming Convention

**Use descriptive test names:**

```
should [ExpectedBehavior] when [Condition]
```

Examples:
- `should login user successfully`
- `should render login form`
- `should call onSubmit when form is submitted`

---

## Mock Data Strategy

### TestUser Profile

**All tests should use the designated test account:**

- **Name**: `TestUser`
- **ID**: `test-user-id-001`

### Mock Data Structure

```
/data/users/
  ├── users.json                          # Contains TestUser profile
  ├── user-test-user-id-001-history.json  # TestUser history
  └── user-test-user-id-001-backlog.json  # TestUser backlog
```

### Mock Data Quality Criteria

- [ ] **Realistic**: Data should be plausible and consistent
- [ ] **Comprehensive**: Cover nominal cases AND edge cases
- [ ] **Valid**: Timestamps, IDs, and references must be valid
- [ ] **Consistent**: Data should be logically coherent

### Edge Cases to Cover

**Empty Data:**
```json
{
  "userId": "test-user-id-001",
  "activities": []
}
```

**Maximum Data:**
```json
{
  "userId": "test-user-id-001",
  "activities": [
    // 100+ activities with various scores
  ]
}
```

**Boundary Values:**
```json
{
  "score": 0,
  "score": 100,
  "questionCount": 1,
  "questionCount": 20
}
```

---

## Coverage Requirements

### Minimum Coverage Targets

- **Overall**: ≥80%
- **Critical Paths**: 100% (authentication, data persistence)
- **Business Logic**: ≥90% (services, hooks)
- **UI Components**: ≥70% (presentational components)

### Running Coverage Reports

**Backend:**
```bash
cd src/backend
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**Frontend:**
```bash
cd src/frontend
npm run test:coverage
```

### Coverage Tools

- **Backend**: Coverlet (integrated with dotnet test)
- **Frontend**: Vitest coverage (c8 or istanbul)

### What to Exclude from Coverage

- Auto-generated code
- Third-party libraries
- Configuration files
- Simple DTOs/models without logic

---

## Best Practices

### 1. Test Independence

**Tests should not depend on each other:**

```typescript
// ✅ GOOD: Each test is independent
describe('UserService', () => {
    beforeEach(() => {
        // Reset state before each test
        useAuthStore.getState().actions.reset();
    });
    
    it('test 1', () => { /* ... */ });
    it('test 2', () => { /* ... */ });
});
```

### 2. Arrange-Act-Assert (AAA)

**Structure tests clearly:**

```csharp
[Fact]
public async Task Example()
{
    // Arrange - Set up test data and mocks
    var user = new User { Id = "test-001" };
    
    // Act - Execute the method under test
    var result = await _service.CreateUserAsync(user);
    
    // Assert - Verify the outcome
    Assert.NotNull(result);
}
```

### 3. Test One Thing

**Each test should verify one behavior:**

```typescript
// ✅ GOOD: Tests one specific behavior
it('should display error message when login fails', () => { /* ... */ });

// ❌ BAD: Tests multiple behaviors
it('should login and redirect and update preferences', () => { /* ... */ });
```

### 4. Avoid Test Logic

**Tests should be simple and straightforward:**

```typescript
// ✅ GOOD: Simple assertion
expect(result.length).toBe(3);

// ❌ BAD: Logic in test
if (result.length > 0) {
    expect(result[0].name).toBe('Test');
}
```

### 5. Use Descriptive Assertions

**Make test failures clear:**

```csharp
// ✅ GOOD: Clear assertion message
Assert.Equal(expected, actual, "User name should match the input");

// ❌ BAD: No context
Assert.Equal(expected, actual);
```

---

## Running Tests

### Backend

```bash
# Run all tests
dotnet test

# Run specific test file
dotnet test --filter "FullyQualifiedName~UserServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Frontend

```bash
# Run all tests
npm run test

# Run in watch mode
npm run test:watch

# Run with coverage
npm run test:coverage

# Run with UI
npm run test:ui
```

---

## Related Documentation

- [Backend-Guidelines.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Backend-Guidelines.md) - Backend architecture and patterns
- [Frontend-Architecture.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Frontend-Architecture.md) - Frontend architecture and patterns
- [Getting-Started.md](file:///d:/Repos/Derot-my-brain/Docs/Developer/Getting-Started.md) - How to run tests locally
