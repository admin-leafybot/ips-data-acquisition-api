# IPS Data Acquisition API

Backend API for IPS Indoor Positioning Data Collection Mobile App built with .NET 8 and Clean Architecture.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
IPSDataAcquisition/
├── Domain/              # Enterprise business rules (Entities, Value Objects)
├── Application/         # Application business rules (Features, DTOs, Interfaces)
├── Infrastructure/      # External concerns (Database, Services)
└── Presentation/        # API Controllers, Entry point
```

### Key Technologies

- **.NET 8.0** - Modern C# with minimal APIs
- **Entity Framework Core** - ORM with PostgreSQL
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Request validation
- **Swagger/OpenAPI** - API documentation
- **AspNetCoreRateLimit** - Rate limiting

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- IDE: Visual Studio 2022, VS Code, or Rider

## 🚀 Getting Started

### 1. Clone the Repository

```bash
cd ips-data-acquisition-api
```

### 2. Update Database Connection String

Edit `src/IPSDataAcquisition.Presentation/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=ips_data_acquisition;Username=your_user;Password=your_password"
  }
}
```

### 3. Create Database

```bash
# Create PostgreSQL database
createdb ips_data_acquisition

# Or using psql
psql -U postgres
CREATE DATABASE ips_data_acquisition;
```

### 4. Run Migrations

```bash
cd src/IPSDataAcquisition.Presentation
dotnet ef migrations add InitialCreate --project ../IPSDataAcquisition.Infrastructure
dotnet ef database update --project ../IPSDataAcquisition.Infrastructure
```

### 5. Run the Application

```bash
dotnet run --project src/IPSDataAcquisition.Presentation
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## 📡 API Endpoints

### Base URL
```
https://your-domain.com/api/v1/
```

### Sessions

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/sessions/create` | Create a new session |
| POST | `/sessions/close` | Close an existing session |
| GET | `/sessions?page=1&limit=50` | Get list of sessions |

### Button Presses

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/button-presses` | Submit a button press (waypoint) |

### IMU Data

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/imu-data/upload` | Upload batch of IMU sensor data |

### Bonuses

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/bonuses?start_date=2024-10-01&end_date=2024-10-31` | Get daily bonuses |

## 📝 Example Requests

### Create Session

```bash
curl -X POST https://localhost:5001/api/v1/sessions/create \
  -H "Content-Type: application/json" \
  -d '{
    "session_id": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": 1697587200000
  }'
```

Response:
```json
{
  "success": true,
  "message": "Session created successfully",
  "data": {
    "session_id": "550e8400-e29b-41d4-a716-446655440000",
    "created_at": 1697587200000
  }
}
```

### Submit Button Press

```bash
curl -X POST https://localhost:5001/api/v1/button-presses \
  -H "Content-Type: application/json" \
  -d '{
    "session_id": "550e8400-e29b-41d4-a716-446655440000",
    "action": "ENTERED_RESTAURANT_BUILDING",
    "timestamp": 1697587210000
  }'
```

### Upload IMU Data

```bash
curl -X POST https://localhost:5001/api/v1/imu-data/upload \
  -H "Content-Type: application/json" \
  -d '{
    "session_id": "550e8400-e29b-41d4-a716-446655440000",
    "data_points": [{
      "timestamp": 1697587210100,
      "accel_x": 0.123, "accel_y": 0.456, "accel_z": 9.789,
      "gyro_x": 0.012, "gyro_y": 0.034, "gyro_z": 0.056,
      "mag_x": 23.4, "mag_y": 12.5, "mag_z": 45.6
    }]
  }'
```

## 🗂️ Project Structure

```
src/
├── IPSDataAcquisition.Domain/
│   ├── Common/
│   │   └── BaseEntity.cs
│   └── Entities/
│       ├── Session.cs
│       ├── ButtonPress.cs
│       ├── IMUData.cs (61 sensor parameters!)
│       └── Bonus.cs
│
├── IPSDataAcquisition.Application/
│   ├── Common/
│   │   ├── DTOs/
│   │   └── Interfaces/
│   └── Features/
│       ├── Sessions/
│       │   ├── Commands/ (CreateSession, CloseSession)
│       │   ├── Queries/ (GetSessions)
│       │   └── Validation/
│       ├── ButtonPresses/
│       ├── IMUData/
│       └── Bonuses/
│
├── IPSDataAcquisition.Infrastructure/
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   └── DependencyInjection.cs
│
└── IPSDataAcquisition.Presentation/
    ├── Controllers/
    │   ├── SessionsController.cs
    │   ├── ButtonPressesController.cs
    │   ├── IMUDataController.cs
    │   └── BonusesController.cs
    ├── Program.cs
    └── appsettings.json
```

## 🗄️ Database Schema

### Sessions Table
```sql
CREATE TABLE sessions (
    session_id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36),
    start_timestamp BIGINT NOT NULL,
    end_timestamp BIGINT,
    is_synced BOOLEAN DEFAULT TRUE,
    status VARCHAR(20) DEFAULT 'in_progress',
    payment_status VARCHAR(20) DEFAULT 'unpaid',
    remarks TEXT,
    bonus_amount DECIMAL(10,2),
    created_at TIMESTAMP,
    updated_at TIMESTAMP
);
```

### Button Presses Table
```sql
CREATE TABLE button_presses (
    id BIGSERIAL PRIMARY KEY,
    session_id VARCHAR(36) REFERENCES sessions(session_id) ON DELETE CASCADE,
    action VARCHAR(50) NOT NULL,
    timestamp BIGINT NOT NULL,
    is_synced BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP,
    updated_at TIMESTAMP
);
```

### IMU Data Table
```sql
CREATE TABLE imu_data (
    id BIGSERIAL PRIMARY KEY,
    session_id VARCHAR(36) REFERENCES sessions(session_id) ON DELETE CASCADE,
    timestamp BIGINT NOT NULL,
    -- 61 sensor parameters (accel_x, accel_y, gyro_x, mag_x, etc.)
    ...
    created_at TIMESTAMP,
    updated_at TIMESTAMP
);
```

## ⚙️ Configuration

### Rate Limiting

Configured in `appsettings.json`:

```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "POST:/api/v1/button-presses",
        "Period": "1m",
        "Limit": 60
      },
      {
        "Endpoint": "POST:/api/v1/imu-data/upload",
        "Period": "1m",
        "Limit": 10
      }
    ]
  }
}
```

### Swagger

Enable/disable Swagger in `appsettings.json`:

```json
{
  "Swagger": {
    "Enabled": true
  }
}
```

## 🧪 Testing

### Using Swagger UI

1. Navigate to `https://localhost:5001/swagger`
2. Expand an endpoint
3. Click "Try it out"
4. Fill in the request body
5. Click "Execute"

### Using cURL

See example requests above.

## 📦 Building for Production

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Publish
dotnet publish src/IPSDataAcquisition.Presentation -c Release -o ./publish
```

## 🐳 Docker Deployment (Optional)

Create `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IPSDataAcquisition.Presentation.dll"]
```

## 🔧 Development

### Adding a New Feature

1. **Create Command/Query** in `Application/Features/YourFeature/Commands` or `Queries`
2. **Create Handler** implementing `IRequestHandler<TRequest, TResponse>`
3. **Add Validation** using FluentValidation
4. **Add Controller** endpoint in `Presentation/Controllers`
5. **Run migrations** if database changes are needed

### Adding a Migration

```bash
cd src/IPSDataAcquisition.Presentation
dotnet ef migrations add YourMigrationName --project ../IPSDataAcquisition.Infrastructure
dotnet ef database update --project ../IPSDataAcquisition.Infrastructure
```

## 📊 Performance Considerations

- **IMU Data Endpoint**: High volume (~250KB payloads every 10 seconds)
  - Uses bulk insert (`AddRangeAsync`)
  - Consider async queue processing for scaling
  - Enable gzip compression
  
- **Database**: 
  - Indexes on `session_id`, `timestamp`, `user_id`
  - Consider partitioning `imu_data` table by timestamp for large datasets

## 📖 API Documentation

Full API documentation is available:
- **Swagger UI**: `https://localhost:5001/swagger`
- **OpenAPI JSON**: `https://localhost:5001/swagger/v1/swagger.json`

## 🤝 Contributing

1. Create a feature branch
2. Follow Clean Architecture principles
3. Add FluentValidation for new commands
4. Update documentation

## 📄 License

MIT

## 🔗 Related Projects

- **Mobile App**: `/Users/sanjeevkumar/Business/IPS/ips-data-acquisition-app` (Android)
- **API Documentation**: See `API_DOCUMENTATION.md` in mobile app folder

## 📞 Support

For issues or questions, please open an issue in the repository.

