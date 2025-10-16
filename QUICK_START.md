# Quick Start Guide

## 🚀 Get Running in 5 Minutes

### 1. Prerequisites Check

```bash
# Check .NET version (need 8.0+)
dotnet --version

# Check PostgreSQL
psql --version
```

### 2. Database Setup

```bash
# Create database
createdb ips_data_acquisition

# Or using psql
psql -U postgres -c "CREATE DATABASE ips_data_acquisition;"
```

### 3. Update Connection String

Edit `src/IPSDataAcquisition.Presentation/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=ips_data_acquisition;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### 4. Run Migrations & Start

```bash
# Navigate to project root
cd /Users/sanjeevkumar/Business/IPS/ips-data-acquisition-api

# Run migrations
cd src/IPSDataAcquisition.Presentation
dotnet ef migrations add InitialCreate --project ../IPSDataAcquisition.Infrastructure
dotnet ef database update --project ../IPSDataAcquisition.Infrastructure

# Run the app
dotnet run
```

### 5. Test the API

Open browser: `https://localhost:5001/swagger`

Or test with curl:

```bash
# Create a session
curl -X POST https://localhost:5001/api/v1/sessions/create \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "timestamp": 1697587200000
  }'

# Submit button press
curl -X POST https://localhost:5001/api/v1/button-presses \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "action": "ENTERED_RESTAURANT_BUILDING",
    "timestamp": 1697587210000
  }'

# Get sessions
curl https://localhost:5001/api/v1/sessions -k
```

## 📡 API Endpoints Summary

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/v1/sessions/create` | POST | Create session |
| `/api/v1/sessions/close` | POST | Close session |
| `/api/v1/sessions` | GET | List sessions |
| `/api/v1/button-presses` | POST | Submit button press |
| `/api/v1/imu-data/upload` | POST | Upload IMU data |
| `/api/v1/bonuses` | GET | Get bonuses |

## 🔧 Troubleshooting

### "Unable to connect to database"

```bash
# Check PostgreSQL is running
pg_isready

# Start PostgreSQL (macOS)
brew services start postgresql

# Start PostgreSQL (Linux)
sudo systemctl start postgresql
```

### "Port already in use"

Change port in `appsettings.json` or `launchSettings.json`:

```json
{
  "applicationUrl": "https://localhost:5002;http://localhost:5003"
}
```

### "Migration already exists"

```bash
# Remove migration
dotnet ef migrations remove --project ../IPSDataAcquisition.Infrastructure

# Re-add
dotnet ef migrations add InitialCreate --project ../IPSDataAcquisition.Infrastructure
```

## 📚 Next Steps

1. Read [README.md](README.md) for full documentation
2. Read [ARCHITECTURE.md](ARCHITECTURE.md) to understand the design
3. Explore Swagger UI at `https://localhost:5001/swagger`
4. Check Android app at `/Users/sanjeevkumar/Business/IPS/ips-data-acquisition-app`

## 🎯 Common Development Tasks

### Add a New Feature

```bash
# 1. Create command in Application/Features/YourFeature/Commands/
# 2. Create handler implementing IRequestHandler
# 3. Add validator using FluentValidation
# 4. Add controller endpoint in Presentation/Controllers/
```

### Add Database Migration

```bash
cd src/IPSDataAcquisition.Presentation
dotnet ef migrations add YourMigrationName --project ../IPSDataAcquisition.Infrastructure
dotnet ef database update --project ../IPSDataAcquisition.Infrastructure
```

### Build for Production

```bash
dotnet publish src/IPSDataAcquisition.Presentation -c Release -o ./publish
```

## ✅ Architecture at a Glance

```
📁 Domain          → Entities (Session, ButtonPress, IMUData, Bonus)
📁 Application     → Features with MediatR (Commands, Queries, Validation)
📁 Infrastructure  → Database (EF Core + PostgreSQL)
📁 Presentation    → API Controllers + Swagger
```

**Pattern**: Clean Architecture + CQRS + MediatR

## 🎉 You're All Set!

Your API is now running at:
- **Swagger UI**: https://localhost:5001/swagger
- **API Base**: https://localhost:5001/api/v1/

Happy coding! 🚀

