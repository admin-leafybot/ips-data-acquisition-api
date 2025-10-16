# First Run Setup - Auto Migration Guide

## ✅ Current Setup

Your API is configured to **automatically apply migrations** when you run it!

```csharp
// This runs automatically on startup:
db.Database.Migrate();  // ✅ Creates tables if they don't exist
```

## 🚀 Quick Start (First Time)

### Step 1: Create PostgreSQL Database

**The database itself must exist** (PostgreSQL doesn't auto-create databases):

```bash
# Quick one-liner:
createdb ips_data_acquisition
```

Or using psql:
```bash
psql -U postgres
CREATE DATABASE ips_data_acquisition;
\q
```

### Step 2: Create Initial Migration

**First time only** - Create the migration files:

```bash
cd src/IPSDataAcquisition.Presentation
dotnet ef migrations add InitialCreate --project ../IPSDataAcquisition.Infrastructure
```

This creates files in `src/IPSDataAcquisition.Infrastructure/Migrations/`

### Step 3: Run the Application

```bash
# From project root
dotnet run --project src/IPSDataAcquisition.Presentation
```

**What happens automatically:**
1. ✅ Application starts
2. ✅ `db.Database.Migrate()` executes
3. ✅ All tables are created (sessions, button_presses, imu_data, bonuses)
4. ✅ API is ready at https://localhost:5001

### Step 4: Verify

Visit: https://localhost:5001/swagger

Test creating a session:
```bash
curl -X POST https://localhost:5001/api/v1/sessions/create \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "timestamp": 1697587200000
  }'
```

## 🔄 Every Subsequent Run

**After the first setup, you just need:**

```bash
dotnet run --project src/IPSDataAcquisition.Presentation
```

That's it! Migrations apply automatically. ✅

## 🛠️ Migration Commands Reference

### Check Migration Status
```bash
cd src/IPSDataAcquisition.Presentation
dotnet ef migrations list --project ../IPSDataAcquisition.Infrastructure
```

### Add New Migration (after model changes)
```bash
dotnet ef migrations add YourMigrationName --project ../IPSDataAcquisition.Infrastructure
```

### Remove Last Migration
```bash
dotnet ef migrations remove --project ../IPSDataAcquisition.Infrastructure
```

### Update Database Manually (if needed)
```bash
dotnet ef database update --project ../IPSDataAcquisition.Infrastructure
```

### Reset Database (CAUTION: Deletes all data!)
```bash
dotnet ef database drop --project ../IPSDataAcquisition.Infrastructure --force
dotnet ef database update --project ../IPSDataAcquisition.Infrastructure
```

## 🔧 Two Migration Approaches

### Approach 1: Migrations (Recommended for Production)

✅ **Current setup** - Uses migration files  
✅ Tracks schema changes  
✅ Can roll back  
✅ Safe for production  

```csharp
db.Database.Migrate();  // ← Currently active
```

### Approach 2: EnsureCreated (Quick Testing Only)

⚠️ Alternative for quick testing  
⚠️ No migration history  
⚠️ Can't track changes  
⚠️ Not for production  

```csharp
// Uncomment this and comment out Migrate() for quick testing:
// db.Database.EnsureCreated();
```

## 📊 Verify Database Creation

### Check Tables Were Created

```bash
psql -U postgres -d ips_data_acquisition
```

In psql:
```sql
-- List all tables
\dt

-- Expected output:
-- sessions
-- button_presses  
-- imu_data
-- bonuses

-- Check table structure
\d sessions
\d imu_data

-- Exit psql
\q
```

### Check from Application

```bash
# Check if sessions endpoint works
curl https://localhost:5001/api/v1/sessions -k

# Should return: {"success":true,"data":[]}
```

## ⚠️ Troubleshooting

### "Database does not exist"

**Solution:** Create the database manually
```bash
createdb ips_data_acquisition
```

### "No migrations found"

**Solution:** Create initial migration
```bash
cd src/IPSDataAcquisition.Presentation
dotnet ef migrations add InitialCreate --project ../IPSDataAcquisition.Infrastructure
```

### "Connection refused"

**Solution:** Make sure PostgreSQL is running
```bash
# Check if PostgreSQL is running
pg_isready

# Start PostgreSQL (macOS)
brew services start postgresql

# Start PostgreSQL (Linux)
sudo systemctl start postgresql
```

### "Password authentication failed"

**Solution:** Update connection string in `appsettings.json`
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=ips_data_acquisition;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

## 📝 Complete First Run Checklist

- [ ] PostgreSQL installed and running
- [ ] Database created (`createdb ips_data_acquisition`)
- [ ] Connection string updated in `appsettings.json`
- [ ] Initial migration created (`dotnet ef migrations add InitialCreate`)
- [ ] Application runs (`dotnet run`)
- [ ] Swagger UI accessible (https://localhost:5001/swagger)
- [ ] Tables created (verify with psql `\dt`)
- [ ] Can create a session via API

## 🎯 Summary

**YES!** ✅ Auto-migrations are set up.

**You only need to:**
1. Create the database once: `createdb ips_data_acquisition`
2. Create initial migration once: `dotnet ef migrations add InitialCreate`
3. Run the app: `dotnet run`

**From then on, just run the app!** Migrations apply automatically on every start.

---

**Status:** ✅ Auto-migration enabled  
**Location:** `Program.cs` lines 56-69  
**Command:** `db.Database.Migrate()`

