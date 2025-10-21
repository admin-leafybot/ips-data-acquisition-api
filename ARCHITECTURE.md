# System Architecture

## Overview

The IPS Data Acquisition API is built using **Clean Architecture** principles with **CQRS pattern** via MediatR. The system is designed for high-performance sensor data collection from mobile devices with enterprise-grade security and scalability.

## Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  (Controllers, Middleware, API Configuration)                │
│  • Thin controllers (routing only)                          │
│  • JWT authentication middleware                            │
│  • GZIP decompression middleware                            │
│  • Global exception handler                                 │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  (Business Logic, Commands, Queries, DTOs)                  │
│  • MediatR Commands & Handlers                              │
│  • FluentValidation Validators                              │
│  • DTOs & Interfaces                                        │
│  • No infrastructure dependencies                           │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  (Data Access, External Services)                           │
│  • EF Core DbContext                                        │
│  • Repository implementations                               │
│  • JWT token generation                                     │
│  • External service integrations                            │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                             │
│  (Entities, Business Rules, Domain Logic)                   │
│  • Pure domain entities                                     │
│  • Business constants                                       │
│  • Domain events (future)                                   │
│  • No external dependencies                                 │
└─────────────────────────────────────────────────────────────┘
```

## Design Patterns

### 1. **Clean Architecture**
- **Dependency Rule**: Dependencies flow inward (Domain ← Application ← Infrastructure ← Presentation)
- **Domain Independence**: Core business logic has no external dependencies
- **Testability**: Each layer can be tested independently

### 2. **CQRS (Command Query Responsibility Segregation)**
- **Commands**: Modify state (Create, Update, Delete)
- **Queries**: Read data without side effects
- Implemented via **MediatR** library

### 3. **Repository Pattern**
- `IApplicationDbContext` interface abstracts data access
- Enables unit testing with mock repositories
- Centralized query logic

### 4. **Dependency Injection**
- All dependencies injected via ASP.NET Core DI
- Scoped lifetime for DbContext and services
- Singleton for configuration and caching

## Data Flow

### Example: Button Press Submission

```
┌──────────┐     ┌────────────┐     ┌─────────────┐     ┌──────────┐     ┌──────────┐
│  Mobile  │────▶│ Controller │────▶│  Validator  │────▶│ Command  │────▶│ Database │
│   App    │     │   (API)    │     │   (Fluent)  │     │ Handler  │     │   (PG)   │
└──────────┘     └────────────┘     └─────────────┘     └──────────┘     └──────────┘
     │                  │                   │                  │
     │                  │                   │                  │
  HTTP POST         Route to            Validate           Execute
  with JWT          MediatR             Input              Business
  token                                                    Logic
                                                           + Save
```

**Steps:**
1. **Controller** receives HTTP request with JWT token
2. **Authentication Middleware** validates JWT and extracts user claims
3. **FluentValidation** validates input DTO
4. **MediatR** dispatches command to appropriate handler
5. **Command Handler** executes business logic
6. **DbContext** persists changes to PostgreSQL
7. **Response** returned to client

## Database Schema

### Core Tables

#### `users` (ASP.NET Identity)
```sql
- id (PK, varchar(450))
- user_name (varchar(256))
- phone_number (varchar(20), unique)
- full_name (varchar(200))
- is_active (boolean, default: false)
- password_hash (text)
- created_at, updated_at (timestamp)
```

#### `sessions`
```sql
- session_id (PK, varchar(36))
- user_id (FK → users.id, nullable)
- start_timestamp, end_timestamp (bigint)
- status (varchar(20): in_progress, completed, approved, rejected)
- payment_status (varchar(20): unpaid, paid)
- bonus_amount (decimal(10,2))
- remarks (text)
- is_synced (boolean)
- created_at, updated_at (timestamp)
```

#### `button_presses`
```sql
- id (PK, bigint, auto-increment)
- session_id (FK → sessions.session_id)
- user_id (FK → users.id, nullable)
- action (varchar(50): ENTERED_RESTAURANT_BUILDING, etc.)
- timestamp (bigint, epoch milliseconds)
- floor_index (integer, nullable)
- is_synced (boolean)
- created_at, updated_at (timestamp)
```

#### `imu_data`
```sql
- id (PK, bigint, auto-increment)
- session_id (FK → sessions.session_id)
- user_id (FK → users.id, nullable)
- timestamp (bigint, epoch milliseconds)
- timestamp_nanos (bigint, nullable)
- accel_x, accel_y, accel_z (float, nullable)
- gyro_x, gyro_y, gyro_z (float, nullable)
- mag_x, mag_y, mag_z (float, nullable)
- [60+ sensor fields - all nullable]
- latitude, longitude, altitude (double, nullable)
- is_synced (boolean)
- created_at, updated_at (timestamp)
```

#### `bonuses`
```sql
- id (PK, bigint, auto-increment)
- user_id (varchar(36))
- date (date)
- amount (decimal(10,2))
- created_at, updated_at (timestamp)
- UNIQUE(user_id, date)
```

#### `refresh_tokens`
```sql
- id (PK, bigint, auto-increment)
- token (varchar(500), unique)
- user_id (FK → users.id)
- expires_at (timestamp)
- is_revoked (boolean)
- revoked_at (timestamp, nullable)
- replaced_by_token (varchar(500), nullable)
- created_at, updated_at (timestamp)
```

### Relationships
- **User → Sessions** (1:N)
- **User → ButtonPresses** (1:N)
- **User → IMUData** (1:N)
- **User → RefreshTokens** (1:N)
- **Session → ButtonPresses** (1:N, cascade delete)
- **Session → IMUData** (1:N, cascade delete)

## Authentication Flow

```
┌─────────────┐
│   Signup    │ → User created with IsActive = false
└──────┬──────┘
       │
       ▼
┌─────────────────────┐
│ Admin Activates     │ → ChangeVerificationStatus with security key
│ (IsActive = true)   │
└──────┬──────────────┘
       │
       ▼
┌─────────────┐
│    Login    │ → Returns: Access Token + Refresh Token + Expiry
└──────┬──────┘
       │
       ▼
┌─────────────────────┐
│  Use Protected APIs │ → All requests include: Authorization: Bearer <token>
└──────┬──────────────┘
       │
       ▼ (Token expires)
┌─────────────────────┐
│  Refresh Token      │ → Get new Access Token + New Refresh Token
└─────────────────────┘
```

## Request/Response Pipeline

### Middleware Order (Important!)

```
HTTP Request
    ↓
1. Global Exception Handler
    ↓
2. HTTPS Redirection
    ↓
3. CORS
    ↓
4. GZIP Request Decompression  ← Critical for IMU data
    ↓
5. Rate Limiting
    ↓
6. Authentication  ← JWT validation
    ↓
7. Authorization   ← Permission checks
    ↓
8. Controllers
    ↓
HTTP Response
```

## Performance Optimizations

### 1. **Bulk Insert for IMU Data**
```csharp
// Handles 1000+ sensor readings in single transaction
await _context.IMUData.AddRangeAsync(imuDataList);
await _context.SaveChangesAsync();
```

### 2. **GZIP Compression**
- Middleware automatically decompresses GZIP requests
- Reduces IMU payload size by ~90%
- Example: 2MB → 200KB

### 3. **Connection Pooling**
- EF Core automatically pools database connections
- Minimum pool size: 10
- Maximum pool size: 100

### 4. **Indexed Queries**
- Composite indexes on (session_id, timestamp)
- User lookups indexed by phone_number
- Session queries indexed by status and user_id

### 5. **Request Size Limits**
- Max request body: 10MB
- Timeout settings optimized for mobile networks
- Keep-alive: 120 seconds

## Security Architecture

### 1. **Authentication**
- JWT tokens signed with HS256
- Claims include: UserId, Phone, FullName
- Token rotation on refresh

### 2. **Authorization**
- All data APIs require `[Authorize]` attribute
- User ID automatically extracted from JWT claims
- No manual user ID passing required

### 3. **Data Isolation**
- Users can only access their own data
- UserId automatically set from authenticated context
- Foreign key constraints enforce data integrity

### 4. **Password Security**
- ASP.NET Core Identity with PBKDF2 hashing
- Minimum password length: 6 characters
- Salt automatically applied per user

### 5. **Admin Operations**
- ChangeVerificationStatus requires security key
- Key stored in configuration (not in code)
- Invalid attempts logged for audit

## Scalability Considerations

### Horizontal Scaling
- **Stateless API**: Can run multiple instances behind load balancer
- **Database Connection Pooling**: Efficient connection reuse
- **No In-Memory State**: All state in PostgreSQL

### Vertical Scaling
- **Async/Await**: Non-blocking I/O throughout
- **Bulk Operations**: Batch inserts for IMU data
- **Indexed Queries**: Fast lookups even with millions of records

### Database Scaling
- **Read Replicas**: Queries can use read-only replicas
- **Partitioning**: IMU data can be partitioned by date
- **Archival**: Old sessions can be archived to cold storage

## Monitoring & Observability

### Logging Levels
- **Debug**: Detailed request/response data
- **Information**: Normal operations (login, session create, etc.)
- **Warning**: Validation failures, invalid requests
- **Error**: Exceptions, database errors

### Key Metrics to Monitor
- Request rate per endpoint
- Database query duration
- Failed authentication attempts
- IMU data upload volume
- Error rates

### Health Checks (Future)
```
GET /health
- Database connectivity
- Disk space
- Memory usage
```

## Technology Decisions

### Why .NET 9?
- High performance (outperforms Node.js, Python)
- Native async/await support
- Strong typing prevents runtime errors
- Excellent tooling and IDE support

### Why PostgreSQL?
- ACID compliance for data integrity
- Excellent performance for time-series data
- JSON support for flexible schema
- Mature ecosystem and tooling

### Why Clean Architecture?
- Maintainability: Clear separation of concerns
- Testability: Easy to mock and unit test
- Flexibility: Can swap out infrastructure
- Team Scalability: Multiple developers can work independently

### Why MediatR?
- Decouples controllers from business logic
- Single Responsibility Principle
- Easy to add cross-cutting concerns (logging, validation)
- Pipeline behaviors for AOP

## Future Enhancements

### Planned Features
- [ ] Real-time updates via SignalR
- [ ] Background job processing (Hangfire)
- [ ] Redis caching for frequently accessed data
- [ ] API versioning (v2, v3)
- [ ] GraphQL endpoint for flexible queries
- [ ] Event sourcing for audit trail

### Performance Improvements
- [ ] Response caching for GET endpoints
- [ ] Database query optimization
- [ ] CDN for static content
- [ ] Compression for responses

### DevOps
- [ ] Automated testing in CI/CD
- [ ] Blue-green deployments
- [ ] Canary releases
- [ ] Infrastructure as Code (Terraform)

## Development Guidelines

### Adding a New Feature

1. **Create Entity** (if needed) in `Domain/Entities`
2. **Create DTO** in `Application/Common/DTOs`
3. **Create Command/Query** in `Application/Features/{Feature}/Commands` or `Queries`
4. **Create Handler** that implements `IRequestHandler<TCommand, TResponse>`
5. **Create Validator** using FluentValidation
6. **Update DbContext** (add DbSet, configure entity)
7. **Create Migration**: `dotnet ef migrations add FeatureName`
8. **Create Controller** action that routes to MediatR
9. **Test** via Swagger UI
10. **Document** in API_DOCUMENTATION.md

### Code Standards

- Use **nullable reference types** throughout
- **Async/await** for all I/O operations
- **Structured logging** with semantic properties
- **Record types** for immutable DTOs
- **String interpolation** for log messages
- **Guard clauses** for validation

### Naming Conventions

- **Database**: snake_case (users, button_presses)
- **C# Classes**: PascalCase (ApplicationUser, ButtonPress)
- **C# Properties**: PascalCase (SessionId, FullName)
- **API Routes**: kebab-case (/button-presses, /imu-data)
- **JSON**: camelCase (sessionId, fullName)

## Error Handling Strategy

### Controller Level
```csharp
try 
{
    var result = await _sender.Send(command);
    return result.Success ? Ok(result) : BadRequest(result);
}
catch (Exception ex) 
{
    _logger.LogError(ex, "Error message");
    return StatusCode(500, new ErrorResponse(...));
}
```

### Handler Level
```csharp
// Validation errors
throw new ArgumentException("Invalid action");

// Not found errors
throw new KeyNotFoundException("Session not found");

// Business rule violations
throw new InvalidOperationException("Session already exists");
```

### Global Exception Handler
- Catches all unhandled exceptions
- Returns consistent error responses
- Logs full stack traces
- Hides sensitive details in production

## Data Consistency

### Transaction Management
- EF Core uses automatic transactions for SaveChangesAsync
- Bulk inserts are atomic (all or nothing)
- No distributed transactions (single database)

### Concurrency Control
- Optimistic concurrency via UpdatedAt timestamp
- Foreign key constraints prevent orphaned records
- Unique constraints on critical fields (phone, session_id)

## API Versioning Strategy

### Current: v1
All routes prefixed with `/api/v1/`

### Future Versions
- v2 endpoints can coexist with v1
- Deprecation notices for old versions
- Sunset policy: 6 months notice

## Deployment Architecture

### Development
```
Developer Machine
    ├── .NET SDK
    ├── PostgreSQL (local)
    └── Swagger UI
```

### Production (AWS)
```
Internet
    ↓
Application Load Balancer (ALB)
    ↓
┌─────────────────────────────────────┐
│   ECS/EC2 Instances (Auto-Scaling)  │
│   ├── Container 1: API              │
│   ├── Container 2: API              │
│   └── Container N: API              │
└──────────────┬──────────────────────┘
               ↓
        Amazon RDS PostgreSQL
        (Multi-AZ, Read Replicas)
               ↓
        Automated Backups
```

See [AWS_DEPLOYMENT.md](AWS_DEPLOYMENT.md) for detailed production setup.

## Configuration Management

### Configuration Sources (Priority Order)
1. Command-line arguments
2. Environment variables
3. appsettings.{Environment}.json
4. appsettings.json

### Sensitive Data
- **Never commit** secrets to git
- Use **GitHub Secrets** for CI/CD
- Use **AWS Secrets Manager** for production (future)
- Placeholders (`__SECRET__`) replaced during deployment

## Testing Strategy (Future)

### Unit Tests
- Test handlers independently
- Mock IApplicationDbContext
- Verify business logic

### Integration Tests
- Test full request pipeline
- Use in-memory database
- Verify database operations

### Performance Tests
- Load testing with 1000+ concurrent users
- Stress testing for IMU bulk uploads
- Latency measurements

## Monitoring & Alerts (Production)

### CloudWatch Metrics
- HTTP 4xx/5xx error rates
- Request latency (p50, p95, p99)
- Database connection pool usage
- CPU and memory utilization

### Alerts
- Error rate > 1%
- Average latency > 500ms
- Database connection failures
- Disk space < 20%

## Dependencies

### NuGet Packages

**Presentation:**
- Microsoft.AspNetCore.Authentication.JwtBearer (9.0.10)
- AspNetCoreRateLimit (5.0.0)
- Swashbuckle.AspNetCore (6.x)

**Application:**
- MediatR (12.2.0)
- FluentValidation (11.9.0)
- Microsoft.EntityFrameworkCore (9.0.10)

**Infrastructure:**
- Npgsql.EntityFrameworkCore.PostgreSQL (9.0.1)
- EFCore.NamingConventions (9.0.0)
- Microsoft.AspNetCore.Identity.EntityFrameworkCore (9.0.10)

**Domain:**
- Microsoft.Extensions.Identity.Stores (9.0.10)

## Performance Benchmarks

### Typical Response Times (Production)
- User Login: 50-100ms
- Create Session: 20-50ms
- Submit Button Press: 15-30ms
- Upload IMU Data (100 points): 100-200ms
- Upload IMU Data (1000 points): 500-800ms

### Throughput
- Concurrent Users: 1000+
- Requests/second: 500+
- IMU Points/second: 10,000+

## Design Principles Applied

1. **SOLID**
   - Single Responsibility: Each handler does one thing
   - Open/Closed: Extend via new handlers, not modifications
   - Liskov Substitution: Interface-based design
   - Interface Segregation: Small, focused interfaces
   - Dependency Inversion: Depend on abstractions

2. **DRY (Don't Repeat Yourself)**
   - Base entities for common properties
   - Shared DTOs and response types
   - Reusable middleware components

3. **KISS (Keep It Simple, Stupid)**
   - Thin controllers
   - Clear command names
   - Straightforward data flow

4. **YAGNI (You Aren't Gonna Need It)**
   - No premature optimization
   - Features added as needed
   - Simple solutions first

## Maintenance

### Database Backups
- Automated daily backups (AWS RDS)
- Point-in-time recovery available
- Retention: 7 days

### Updates
- Regular security patches
- .NET version updates
- NuGet package updates
- Database migrations

### Rollback Strategy
- Docker images tagged by git commit SHA
- Previous versions available in ECR
- Database migrations reversible via Down() methods
- Zero-downtime deployments

---

For API usage details, see [API_DOCUMENTATION.md](API_DOCUMENTATION.md)

For deployment instructions, see [AWS_DEPLOYMENT.md](AWS_DEPLOYMENT.md)

