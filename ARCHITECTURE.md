# Architecture Documentation

## Clean Architecture Overview

This project follows Clean Architecture principles, organizing code into four distinct layers:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  (Controllers, API Endpoints, Program.cs)                   │
│  Dependencies: Application, Infrastructure                   │
└─────────────────────────────────────────────────────────────┘
                         ↓ depends on
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  (Features, Commands, Queries, DTOs, Interfaces)            │
│  Dependencies: Domain only                                   │
└─────────────────────────────────────────────────────────────┘
                         ↓ depends on
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                             │
│  (Entities, Value Objects, Domain Logic)                    │
│  Dependencies: None (Pure C#)                                │
└─────────────────────────────────────────────────────────────┘
                         ↑ implements
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  (DbContext, Repositories, External Services)               │
│  Dependencies: Application, Domain                           │
└─────────────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### 1. Domain Layer (`IPSDataAcquisition.Domain`)

**Purpose**: Contains enterprise business rules and domain entities.

**Dependencies**: None (pure C#)

**Contents**:
- `Entities/` - Domain entities (Session, ButtonPress, IMUData, Bonus)
- `Common/` - Base classes and interfaces

**Example**:
```csharp
public class Session
{
    public string SessionId { get; set; }
    public long StartTimestamp { get; set; }
    public string Status { get; set; }
    // ... domain logic
}
```

### 2. Application Layer (`IPSDataAcquisition.Application`)

**Purpose**: Contains application business rules using CQRS pattern with MediatR.

**Dependencies**: Domain only

**Contents**:
- `Features/` - Organized by feature (Sessions, ButtonPresses, IMUData, Bonuses)
  - `Commands/` - Write operations (CreateSession, SubmitButtonPress, etc.)
  - `Queries/` - Read operations (GetSessions, GetBonuses)
  - `Validation/` - FluentValidation validators
- `Common/`
  - `DTOs/` - Data Transfer Objects
  - `Interfaces/` - Abstractions (IApplicationDbContext)

**Example Command**:
```csharp
public record CreateSessionCommand(string SessionId, long Timestamp) 
    : IRequest<CreateSessionResponseDto>;

public class CreateSessionCommandHandler 
    : IRequestHandler<CreateSessionCommand, CreateSessionResponseDto>
{
    private readonly IApplicationDbContext _context;
    
    public async Task<CreateSessionResponseDto> Handle(
        CreateSessionCommand request, 
        CancellationToken cancellationToken)
    {
        // Business logic here
    }
}
```

### 3. Infrastructure Layer (`IPSDataAcquisition.Infrastructure`)

**Purpose**: Implements external concerns (database, external services).

**Dependencies**: Application, Domain

**Contents**:
- `Data/` - Entity Framework DbContext
- `DependencyInjection.cs` - Service registration

**Example**:
```csharp
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Session> Sessions { get; set; }
    public DbSet<IMUData> IMUData { get; set; }
    // ...
}
```

### 4. Presentation Layer (`IPSDataAcquisition.Presentation`)

**Purpose**: Handles HTTP requests and responses.

**Dependencies**: Application, Infrastructure

**Contents**:
- `Controllers/` - API endpoints
- `Program.cs` - Application startup
- `appsettings.json` - Configuration

**Example**:
```csharp
[ApiController]
[Route("api/v1/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISender _sender; // MediatR
    
    [HttpPost("create")]
    public async Task<ActionResult> CreateSession(
        CreateSessionRequestDto request, 
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateSessionCommand(request.SessionId, request.Timestamp), 
            cancellationToken);
        return Ok(result);
    }
}
```

## Design Patterns

### CQRS (Command Query Responsibility Segregation)

Commands (writes) and Queries (reads) are separated:

- **Commands**: `CreateSessionCommand`, `SubmitButtonPressCommand`
- **Queries**: `GetSessionsQuery`, `GetBonusesQuery`

### Mediator Pattern (MediatR)

Controllers don't directly call business logic. Instead, they send commands/queries through MediatR:

```
Controller → MediatR → CommandHandler → DbContext
```

Benefits:
- Decoupling
- Easy testing
- Single Responsibility Principle

### Repository Pattern

`IApplicationDbContext` interface abstracts database access:

```csharp
public interface IApplicationDbContext
{
    DbSet<Session> Sessions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

### Dependency Injection

All dependencies are registered in `Program.cs` and `DependencyInjection.cs`:

```csharp
// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(AssemblyMarker).Assembly);
```

## Data Flow

### Example: Create Session

1. **HTTP Request** → `SessionsController.CreateSession()`
2. **Controller** → Creates `CreateSessionCommand` and sends via MediatR
3. **MediatR** → Routes to `CreateSessionCommandHandler`
4. **Validator** → `CreateSessionCommandValidator` validates input
5. **Handler** → Business logic, saves to database via `IApplicationDbContext`
6. **Response** → Returns `CreateSessionResponseDto`

```
┌─────────┐   ┌────────────┐   ┌─────────┐   ┌─────────┐   ┌──────────┐
│  HTTP   │──▶│Controller  │──▶│MediatR  │──▶│Handler  │──▶│ Database │
│ Request │   │            │   │         │   │         │   │          │
└─────────┘   └────────────┘   └─────────┘   └─────────┘   └──────────┘
                                     │
                                     ▼
                               ┌──────────┐
                               │Validator │
                               └──────────┘
```

## Database Design

### Entity Relationships

```
Session (1) ──< (Many) ButtonPress
Session (1) ──< (Many) IMUData
```

### Naming Convention

- **C# Entities**: PascalCase (e.g., `SessionId`, `AccelX`)
- **Database Tables**: snake_case (e.g., `session_id`, `accel_x`)

This is handled automatically by `UseSnakeCaseNamingConvention()`.

## Validation Strategy

**FluentValidation** is used for request validation:

```csharp
public class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .Matches(@"^[0-9a-fA-F]{8}-...$")
            .WithMessage("session_id must be a valid UUID");
    }
}
```

Validation happens automatically before command handlers execute.

## Error Handling

Exceptions are handled in controllers:

```csharp
try
{
    var result = await _sender.Send(command);
    return Ok(result);
}
catch (KeyNotFoundException ex)
{
    return NotFound(new ApiResponse { Success = false, Message = ex.Message });
}
catch (InvalidOperationException ex)
{
    return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
}
```

## Testing Strategy

### Unit Tests
- Test command/query handlers independently
- Mock `IApplicationDbContext`

### Integration Tests
- Test entire flow from controller to database
- Use in-memory database or test database

### Example:
```csharp
[Fact]
public async Task CreateSession_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var command = new CreateSessionCommand("uuid", 123456789);
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("uuid", result.SessionId);
}
```

## Performance Optimizations

### 1. Bulk Inserts for IMU Data

```csharp
await _context.IMUData.AddRangeAsync(imuDataList, cancellationToken);
await _context.SaveChangesAsync(cancellationToken);
```

### 2. Async All the Way

All database operations use async/await.

### 3. Database Indexing

Indexes on frequently queried columns:
- `session_id`
- `timestamp`
- `user_id`
- Composite: `(session_id, timestamp)`

### 4. Rate Limiting

Protects API from abuse:
- Button presses: 60/minute
- IMU uploads: 10/minute

## Security Considerations

### Future Enhancements

1. **Authentication**: Add JWT authentication
2. **Authorization**: Ensure users can only access their own data
3. **Input Sanitization**: Already handled by FluentValidation
4. **SQL Injection**: Protected by EF Core parameterized queries
5. **HTTPS**: Enforce in production

## Scalability

### Horizontal Scaling

- Stateless API design allows multiple instances
- Load balancer distributes traffic

### Database Optimization

For high-volume IMU data:
- **Partitioning**: Partition `imu_data` table by timestamp
- **Read Replicas**: Use for GET endpoints
- **Caching**: Add Redis for frequently accessed data

### Async Processing

For IMU uploads, consider:
```
API → Message Queue (RabbitMQ/Redis) → Background Worker → Database
```

This allows API to return immediately while data is processed asynchronously.

## Monitoring & Logging

### Structured Logging

```csharp
_logger.LogInformation(
    "Successfully processed {Count} IMU data points for session {SessionId}",
    count, sessionId);
```

### Metrics to Track

- Request rate per endpoint
- Response times (p50, p95, p99)
- Error rates
- Database query performance

## Deployment

### Environment-Specific Configuration

- `appsettings.json` - Default
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides (not in repo)

### Database Migrations

Apply migrations in deployment pipeline:

```bash
dotnet ef database update --project IPSDataAcquisition.Infrastructure
```

## Best Practices

1. **Keep Domain Layer Pure**: No external dependencies
2. **Use Records for DTOs**: Immutable, concise
3. **Async All the Way**: Never block with `.Result` or `.Wait()`
4. **Validate Early**: Use FluentValidation at application boundary
5. **Dependency Direction**: Always point inward (toward Domain)
6. **Single Responsibility**: One handler per command/query
7. **Use CancellationTokens**: Support request cancellation

## Conclusion

This architecture provides:
- **Maintainability**: Clear separation of concerns
- **Testability**: Easy to unit test and mock
- **Scalability**: Stateless, horizontally scalable
- **Flexibility**: Easy to add new features following patterns

