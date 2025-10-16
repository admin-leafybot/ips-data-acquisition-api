# Deployment Checklist

## ✅ Pre-Deployment Verification

### Build & Compile
- [x] Debug build successful
- [x] Release build successful
- [x] All 4 projects compile without errors
- [x] No linter warnings

### Code Quality
- [x] Clean Architecture implemented
- [x] MediatR commands and queries created
- [x] FluentValidation on all commands
- [x] Proper error handling in controllers
- [x] Async/await throughout
- [x] Dependency injection configured

### Database
- [ ] PostgreSQL installed and running
- [ ] Database created (`ips_data_acquisition`)
- [ ] Connection string updated
- [ ] Migrations created
- [ ] Migrations applied successfully
- [ ] Test data inserted (optional)

### Configuration
- [ ] Production connection string set
- [ ] Secrets removed from `appsettings.json`
- [ ] Environment variables configured
- [ ] CORS origins updated (if needed)
- [ ] Rate limiting reviewed
- [ ] Swagger disabled for production (or secured)

---

## 🚀 Deployment Steps

### 1. Database Setup

```bash
# Create PostgreSQL database
createdb ips_data_acquisition

# Or using psql
psql -U postgres
CREATE DATABASE ips_data_acquisition;
\q
```

### 2. Update Configuration

Create `appsettings.Production.json` (DO NOT commit to git):

```json
{
  "ConnectionStrings": {
    "Default": "Host=your-db-host;Port=5432;Database=ips_data_acquisition;Username=your_user;Password=YOUR_SECURE_PASSWORD"
  },
  "Swagger": {
    "Enabled": false
  }
}
```

### 3. Run Migrations

```bash
cd src/IPSDataAcquisition.Presentation

# Create migration (if not already created)
dotnet ef migrations add InitialCreate --project ../IPSDataAcquisition.Infrastructure

# Apply migration
dotnet ef database update --project ../IPSDataAcquisition.Infrastructure
```

### 4. Build for Production

```bash
# Navigate to project root
cd /Users/sanjeevkumar/Business/IPS/ips-data-acquisition-api

# Build in Release mode
dotnet build --configuration Release

# Publish
dotnet publish src/IPSDataAcquisition.Presentation -c Release -o ./publish
```

### 5. Run Locally (Test)

```bash
cd src/IPSDataAcquisition.Presentation
dotnet run --configuration Release
```

Visit: https://localhost:5001/swagger

### 6. Deploy to Server

**Option A: Docker**
```bash
# Build Docker image
docker build -t ips-data-acquisition-api .

# Run container
docker run -d -p 5000:80 \
  -e ConnectionStrings__Default="Host=db;Database=ips_data_acquisition;..." \
  ips-data-acquisition-api
```

**Option B: Direct Deployment**
```bash
# Copy publish folder to server
scp -r ./publish user@server:/var/www/ips-api

# On server, run with systemd or supervisor
dotnet /var/www/ips-api/IPSDataAcquisition.Presentation.dll
```

**Option C: Cloud Platform**
- Azure App Service
- AWS Elastic Beanstalk
- Google Cloud Run
- Heroku

---

## 🧪 Testing Checklist

### Local Testing

```bash
# 1. Create Session
curl -X POST https://localhost:5001/api/v1/sessions/create \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "timestamp": 1697587200000
  }'

# Expected: {"success": true, "message": "Session created successfully", ...}

# 2. Submit Button Press
curl -X POST https://localhost:5001/api/v1/button-presses \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "action": "ENTERED_RESTAURANT_BUILDING",
    "timestamp": 1697587210000
  }'

# Expected: {"success": true, "message": "Button press recorded", ...}

# 3. Upload IMU Data (minimal)
curl -X POST https://localhost:5001/api/v1/imu-data/upload \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "data_points": [{
      "timestamp": 1697587210100,
      "accel_x": 0.123, "accel_y": 0.456, "accel_z": 9.789,
      "gyro_x": 0.012, "gyro_y": 0.034, "gyro_z": 0.056,
      "mag_x": 23.4, "mag_y": 12.5, "mag_z": 45.6
    }]
  }'

# Expected: {"success": true, "message": "IMU data uploaded successfully", ...}

# 4. Get Sessions
curl https://localhost:5001/api/v1/sessions -k

# Expected: {"success": true, "data": [...]}

# 5. Close Session
curl -X POST https://localhost:5001/api/v1/sessions/close \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "end_timestamp": 1697587800000
  }'

# Expected: {"success": true, "message": "Session closed successfully", ...}
```

### Database Verification

```sql
-- Check sessions created
SELECT * FROM sessions;

-- Check button presses
SELECT * FROM button_presses;

-- Check IMU data
SELECT COUNT(*) FROM imu_data;

-- Check bonuses
SELECT * FROM bonuses;
```

### Android App Integration

- [ ] Update `RetrofitClient.kt` base URL
- [ ] Test session creation from app
- [ ] Test button press submission
- [ ] Test IMU data upload
- [ ] Verify data appears in database
- [ ] Test offline queue sync

---

## 📊 Monitoring Setup

### Application Logs

Check logs for errors:
```bash
# In production
tail -f /var/log/ips-api/app.log

# Or use dotnet logging
dotnet run | grep -i error
```

### Database Monitoring

```sql
-- Check table sizes
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- Check recent activity
SELECT * FROM sessions ORDER BY created_at DESC LIMIT 10;
SELECT * FROM button_presses ORDER BY created_at DESC LIMIT 10;
```

### Performance Monitoring

- [ ] Response times < 2 seconds
- [ ] IMU upload handles 250KB payloads
- [ ] Database queries use indexes
- [ ] No N+1 query issues

---

## 🔒 Security Checklist

### Pre-Production
- [ ] Remove default passwords
- [ ] Update connection strings with strong passwords
- [ ] Disable Swagger in production (or add authentication)
- [ ] Review CORS origins (don't use `*` in production)
- [ ] Enable HTTPS only
- [ ] Review rate limiting rules
- [ ] Add authentication (JWT) if needed
- [ ] Add authorization if multi-tenant

### Production
- [ ] Use environment variables for secrets
- [ ] Enable firewall rules
- [ ] Use SSL certificates
- [ ] Regular security updates
- [ ] Monitor for suspicious activity
- [ ] Regular database backups

---

## 🔄 Post-Deployment

### Smoke Tests
- [ ] API is accessible
- [ ] Swagger UI loads (if enabled)
- [ ] Can create session
- [ ] Can submit button press
- [ ] Can upload IMU data
- [ ] Can retrieve sessions
- [ ] Database is receiving data

### Monitoring
- [ ] Set up application monitoring
- [ ] Configure error alerting
- [ ] Set up database monitoring
- [ ] Configure backup schedule

### Documentation
- [ ] Update team with API URL
- [ ] Share Swagger/API documentation
- [ ] Update Android app configuration
- [ ] Document any environment-specific notes

---

## 🆘 Troubleshooting

### API Won't Start

```bash
# Check port availability
lsof -i :5000
lsof -i :5001

# Check database connection
psql -h localhost -U postgres -d ips_data_acquisition
```

### Database Connection Errors

```bash
# Verify PostgreSQL is running
pg_isready

# Check connection string format
# Format: Host=host;Port=5432;Database=dbname;Username=user;Password=pass

# Test connection manually
psql -h your-host -p 5432 -U your-user -d ips_data_acquisition
```

### Migration Errors

```bash
# Reset database (CAUTION: deletes all data!)
dotnet ef database drop --project ../IPSDataAcquisition.Infrastructure --force
dotnet ef database update --project ../IPSDataAcquisition.Infrastructure

# Or manually drop and recreate
dropdb ips_data_acquisition
createdb ips_data_acquisition
dotnet ef database update --project ../IPSDataAcquisition.Infrastructure
```

### Rate Limiting Issues

Edit `appsettings.json` and adjust limits:
```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "POST:/api/v1/imu-data/upload",
        "Period": "1m",
        "Limit": 20  // Increase if needed
      }
    ]
  }
}
```

---

## 📞 Support

### Resources
- README.md - Complete documentation
- ARCHITECTURE.md - Architecture details
- QUICK_START.md - Quick setup guide
- PROJECT_SUMMARY.md - Project overview

### Common Commands

```bash
# Build
dotnet build

# Run
dotnet run --project src/IPSDataAcquisition.Presentation

# Add migration
dotnet ef migrations add MigrationName --project src/IPSDataAcquisition.Infrastructure

# Update database
dotnet ef database update --project src/IPSDataAcquisition.Infrastructure

# Publish
dotnet publish -c Release -o ./publish
```

---

## ✅ Final Checklist

Before going live:

- [ ] All tests pass
- [ ] Database migrations applied
- [ ] Production config set
- [ ] Secrets secured
- [ ] Swagger disabled (or secured)
- [ ] HTTPS enabled
- [ ] Monitoring configured
- [ ] Backups scheduled
- [ ] Android app integrated
- [ ] Team notified

---

**Good luck with your deployment! 🚀**

