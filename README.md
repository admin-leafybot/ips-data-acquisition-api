# IPS Data Acquisition API

A high-performance backend API for Indoor Positioning System (IPS) data collection from mobile applications. Built with .NET 9, PostgreSQL, and Clean Architecture principles.

## 🚀 Quick Start

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment)

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ips-data-acquisition-api
   ```

2. **Update connection string**
   
   Edit `src/IPSDataAcquisition.Presentation/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Host=localhost;Port=5432;Database=ips_data;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Run database migrations**
   ```bash
   dotnet ef database update --project src/IPSDataAcquisition.Infrastructure --startup-project src/IPSDataAcquisition.Presentation
   ```

4. **Build and run**
   ```bash
   dotnet build IPSDataAcquisition.sln
   dotnet run --project src/IPSDataAcquisition.Presentation
   ```

5. **Access Swagger UI**
   ```
   http://localhost:5000/swagger
   ```

### Using Docker

```bash
# Development
docker-compose up -d

# Production
docker-compose -f docker-compose.prod.yml up -d
```

## 📋 Features

- ✅ **User Authentication** - JWT-based authentication with refresh tokens
- ✅ **Session Management** - Track user delivery sessions
- ✅ **Button Press Tracking** - Record user actions during delivery
- ✅ **IMU Data Collection** - High-volume sensor data ingestion (up to 100Hz)
- ✅ **Bonus Management** - User bonus tracking and retrieval
- ✅ **GZIP Compression** - Automatic decompression for large payloads
- ✅ **Rate Limiting** - Prevent API abuse
- ✅ **Clean Architecture** - Separation of concerns with CQRS pattern
- ✅ **Validation** - FluentValidation for all inputs
- ✅ **Logging** - Comprehensive structured logging

## 🏗️ Project Structure

```
ips-data-acquisition-api/
├── src/
│   ├── IPSDataAcquisition.Domain/          # Entities & Domain Logic
│   ├── IPSDataAcquisition.Application/     # Business Logic & Commands
│   ├── IPSDataAcquisition.Infrastructure/  # Data Access & External Services
│   └── IPSDataAcquisition.Presentation/    # API Controllers & Middleware
├── ARCHITECTURE.md                          # Architecture documentation
├── API_DOCUMENTATION.md                     # API endpoints documentation
├── AWS_DEPLOYMENT.md                        # Production deployment guide
└── README.md                                # This file
```

## 🔑 Key Technologies

- **.NET 9** - High-performance web framework
- **PostgreSQL** - Relational database with excellent JSON support
- **Entity Framework Core 9** - ORM with Code-First migrations
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Input validation
- **ASP.NET Core Identity** - User authentication
- **JWT Bearer** - Token-based authentication
- **Swagger/OpenAPI** - API documentation

## 📦 Database Migrations

### Create a new migration
```bash
dotnet ef migrations add MigrationName --project src/IPSDataAcquisition.Infrastructure --startup-project src/IPSDataAcquisition.Presentation
```

### Apply migrations
```bash
dotnet ef database update --project src/IPSDataAcquisition.Infrastructure --startup-project src/IPSDataAcquisition.Presentation
```

### Remove last migration
```bash
dotnet ef migrations remove --project src/IPSDataAcquisition.Infrastructure --startup-project src/IPSDataAcquisition.Presentation
```

## 🔧 Configuration

### Required Settings

**Development** (`appsettings.json`):
- Database connection string
- JWT secret key (min 32 characters)
- Admin verification key
- Rate limiting rules

**Production** (`appsettings.Production.json`):
- Uses placeholder tokens: `__DB_CONNECTION_STRING__`, `__JWT_SECRET_KEY__`, `__ADMIN_VERIFICATION_KEY__`
- Replaced during CI/CD deployment from GitHub Secrets

### Environment Variables (Docker)

```bash
DATABASE_CONNECTION_STRING="Host=db;Port=5432;Database=ips_data;Username=postgres;Password=yourpassword"
JWT_SECRET_KEY="Your64CharacterSecretKeyHere"
ADMIN_VERIFICATION_KEY="YourAdminSecurityKey"
```

## 🧪 Testing the API

### 1. Sign Up a User
```bash
curl -X POST http://localhost:5000/api/v1/user/signup \
  -H "Content-Type: application/json" \
  -d '{
    "phone": "1234567890",
    "password": "Password123",
    "fullName": "John Doe"
  }'
```

### 2. Activate User Account (Admin only)
```bash
curl -X POST http://localhost:5000/api/v1/user/ChangeVerificationStatus \
  -H "Content-Type: application/json" \
  -d '{
    "phone": "1234567890",
    "status": true,
    "securityKey": "AdminSecureKey123!ChangeThisInProduction"
  }'
```

### 3. Login
```bash
curl -X POST http://localhost:5000/api/v1/user/login \
  -H "Content-Type: application/json" \
  -d '{
    "phone": "1234567890",
    "password": "Password123"
  }'
```

### 4. Create Session (with JWT token)
```bash
curl -X POST http://localhost:5000/api/v1/sessions/create \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "sessionId": "session-123",
    "timestamp": 1697890000000
  }'
```

## 📊 Performance Characteristics

- **IMU Data Upload**: Handles 1000+ data points per request
- **GZIP Compression**: Reduces payload size by ~90%
- **Bulk Insert**: Optimized for high-throughput sensor data
- **Connection Pooling**: Efficient database connection management
- **Rate Limiting**: 600 button presses/min, 1000 IMU uploads/min

## 🔒 Security

- All APIs (except signup/login) require JWT authentication
- Passwords hashed with ASP.NET Core Identity
- Refresh tokens for extended sessions (7 days)
- Access tokens expire after 720 hours
- Admin operations require security key
- HTTPS enforced in production
- SQL injection protection via parameterized queries

## 📝 Logging

Logs include:
- Request/response details
- Database operations
- Authentication attempts
- Errors with full stack traces
- Performance metrics

View logs:
```bash
# Docker
docker logs ips-data-acquisition-api -f

# Local
Check console output
```

## 🐛 Troubleshooting

### Database Connection Issues
- Verify PostgreSQL is running
- Check connection string in appsettings.json
- Ensure database exists

### Migration Errors
- Ensure no pending migrations: `dotnet ef migrations list`
- Reset database (dev only): `dotnet ef database drop`
- Reapply migrations: `dotnet ef database update`

### Authentication Issues
- Verify JWT secret key is at least 32 characters
- Check token expiration settings
- Ensure user account is active (`IsActive = true`)

## 📚 Documentation

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - System architecture and design patterns
- **[API_DOCUMENTATION.md](API_DOCUMENTATION.md)** - Complete API reference
- **[AWS_DEPLOYMENT.md](AWS_DEPLOYMENT.md)** - Production deployment guide

## 🤝 Contributing

1. Follow Clean Architecture principles
2. Use MediatR for all commands/queries
3. Add FluentValidation for all inputs
4. Write structured logs
5. Keep controllers thin (routing only)

## 📄 License

[Your License Here]

## 👥 Support

For issues or questions, please contact the development team.

