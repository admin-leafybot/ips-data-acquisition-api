# Debugging Logs - Enhanced Error Visibility

## ✅ Enhanced Logging Now Enabled

I've added comprehensive logging throughout the application to show detailed errors.

---

## 🔍 View Detailed Logs on Server

### After Redeployment

After you push the updated code and it redeploys, run these commands on your EC2 server:

### 1. Real-Time Logs (Recommended)

```bash
# Watch logs in real-time with detailed output
sudo docker logs -f --tail 100 ips-data-acquisition-api
```

**What you'll now see:**
```
2025-10-17 14:39:00 info: Creating session: 123e4567-e89b-12d3-a456-426614174000 at timestamp 1697587200000
2025-10-17 14:39:00 info: Decompressing GZIP request from /api/v1/imu-data/upload, Content-Length: 8542
2025-10-17 14:39:00 info: GZIP decompressed: 8542 bytes → 32156 bytes
2025-10-17 14:39:00 info: Received IMU data upload: 50 points for session 123e4567-e89b-12d3-a456-426614174000
2025-10-17 14:39:01 info: Successfully processed 50 IMU data points for session 123e4567-e89b-12d3-a456-426614174000
```

**If there's an error, you'll see:**
```
2025-10-17 14:39:00 fail: Unhandled exception occurred. Path: /api/v1/imu-data/upload, Method: POST
System.Exception: Detailed error message here
   at ClassName.MethodName() in /path/to/file.cs:line 123
   Full stack trace...
```

### 2. All Logs (Last 500 Lines)

```bash
sudo docker logs --tail 500 ips-data-acquisition-api
```

### 3. Save Logs to File

```bash
sudo docker logs ips-data-acquisition-api > ~/api-logs.txt 2>&1
cat ~/api-logs.txt
```

### 4. Search for Specific Errors

```bash
# Search for errors
sudo docker logs ips-data-acquisition-api 2>&1 | grep -i "error"

# Search for warnings
sudo docker logs ips-data-acquisition-api 2>&1 | grep -i "warn"

# Search for exceptions
sudo docker logs ips-data-acquisition-api 2>&1 | grep -i "exception"

# Search for failed requests
sudo docker logs ips-data-acquisition-api 2>&1 | grep -i "fail"
```

---

## 📊 What Was Enhanced

### 1. **Global Exception Handler** ✅
Catches ALL unhandled exceptions and logs them with full stack traces

```csharp
app.UseGlobalExceptionHandler();  // Catches everything
```

**Returns to Android app:**
```json
{
  "success": false,
  "message": "An error occurred processing your request.",
  "data": null,
  "error": {
    "type": "NullReferenceException",
    "message": "Object reference not set...",
    "stackTrace": "at ClassName.Method()...",
    "innerException": "Inner error details"
  }
}
```

### 2. **Detailed Logging Levels** ✅

**Updated in appsettings:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",       // ← Was Warning
      "Microsoft.EntityFrameworkCore": "Information", // ← Was Warning
      "IPSDataAcquisition": "Debug",               // ← Shows ALL our code logs
      "System.Net.Http": "Information"
    },
    "Console": {
      "IncludeScopes": true,
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss "
    }
  }
}
```

### 3. **Per-Endpoint Logging** ✅

Every endpoint now logs:
- ✅ Request received
- ✅ Parameters/data received
- ✅ Success/failure status
- ✅ Error details if failed

**Example for IMU upload:**
```csharp
_logger.LogInformation("Received IMU data upload: {PointCount} points for session {SessionId}");
_logger.LogError(ex, "Error uploading IMU data for session {SessionId}", sessionId);
```

### 4. **GZIP Middleware Logging** ✅

Shows compression details:
```
info: Decompressing GZIP request from /api/v1/imu-data/upload, Content-Length: 8542
info: GZIP decompressed: 8542 bytes → 32156 bytes
```

### 5. **Database Query Logging** ✅

Now shows SQL queries:
```
info: Microsoft.EntityFrameworkCore.Database.Command
      Executed DbCommand (45ms) [Parameters=[@p0='123e4567...'], CommandType='Text']
      INSERT INTO sessions (session_id, start_timestamp, ...) VALUES (@p0, @p1, ...)
```

---

## 🧪 Testing the Logging

### Step 1: Redeploy with Enhanced Logging

```bash
# On your local machine:
git add .
git commit -m "Add enhanced logging and global exception handler"
git push origin main
```

Wait for GitHub Actions to complete (build → push → deploy)

### Step 2: Watch Logs During Android App Test

```bash
# SSH into EC2
ssh your-user@your-ec2-ip

# Start watching logs
sudo docker logs -f ips-data-acquisition-api

# Keep this terminal open
```

### Step 3: Use Android App

While logs are displaying, use the Android app to:
1. Create a session
2. Press buttons
3. Upload IMU data

You'll see EVERY request in real-time with detailed information!

---

## 🔍 What to Look For

### Success Logs (Good)

```
2025-10-17 14:39:00 info: Creating session: abc123...
2025-10-17 14:39:00 info: Session created successfully: abc123...
2025-10-17 14:39:05 info: Button press: ENTERED_RESTAURANT_BUILDING for session abc123...
2025-10-17 14:39:10 info: Decompressing GZIP request, Content-Length: 8542
2025-10-17 14:39:10 info: GZIP decompressed: 8542 bytes → 32156 bytes
2025-10-17 14:39:10 info: Received IMU data upload: 50 points for session abc123...
2025-10-17 14:39:11 info: Successfully processed 50 IMU data points
```

### Error Logs (Need Attention)

```
2025-10-17 14:39:00 fail: Unhandled exception occurred
System.InvalidOperationException: Session with ID abc123 already exists
   at CreateSessionCommandHandler.Handle() in /src/.../CreateSessionCommandHandler.cs:line 24
```

```
2025-10-17 14:39:10 fail: Failed to decompress GZIP request. Error: Invalid data
System.IO.InvalidDataException: The archive entry was compressed using an unsupported compression method
```

```
2025-10-17 14:39:15 fail: Error uploading IMU data for session abc123
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes
   Inner exception: Npgsql.PostgresException: 23505: duplicate key value violates unique constraint
```

---

## 🛠️ Common Errors & Solutions

### Error 1: GZIP Decompression Failed

**Log shows:**
```
fail: Failed to decompress GZIP request. Error: Invalid data
```

**Cause:** Android app sent corrupted GZIP data  
**Solution:** 
- Check Android app GZIP implementation
- Verify `Content-Encoding: gzip` header is set
- Test with non-compressed request first

### Error 2: Database Connection Failed

**Log shows:**
```
fail: An error occurred using the connection to database 'ips_data_acquisition'
Npgsql.NpgsqlException: Connection refused
```

**Cause:** Can't reach database  
**Solution:**
```bash
# Verify database connection string
sudo docker exec ips-data-acquisition-api printenv | grep DB_CONNECTION_STRING

# Test database from container
sudo docker exec ips-data-acquisition-api \
  sh -c 'apt-get update && apt-get install -y postgresql-client && \
  psql "$DB_CONNECTION_STRING" -c "SELECT 1"'
```

### Error 3: Timeout / Request Too Large

**Log shows:**
```
fail: Request body too large
Microsoft.AspNetCore.Server.Kestrel.Core.BadHttpRequestException
```

**Cause:** Payload exceeds limits  
**Solution:** Already fixed with 10MB limit in latest code

---

## 📋 Logging Checklist

After redeployment, verify these are visible in logs:

- [ ] Timestamps on every log line
- [ ] Request paths (e.g., "/api/v1/sessions/create")
- [ ] HTTP methods (POST, GET)
- [ ] Session IDs and request data
- [ ] GZIP decompression messages
- [ ] Database queries (SQL statements)
- [ ] Success confirmations
- [ ] Full exception stack traces
- [ ] Inner exception details

---

## 🚀 Deploy Enhanced Logging

```bash
# Build succeeded ✅
dotnet build --configuration Release

# Push to trigger auto-deploy
git add .
git commit -m "Add enhanced logging and global exception handler"
git push origin main
```

**After deployment, run:**
```bash
# SSH into EC2
ssh your-user@your-ec2-ip

# Watch logs in real-time
sudo docker logs -f ips-data-acquisition-api
```

**Then use the Android app and you'll see EVERYTHING!** 🎯

---

## 💡 Pro Tips

### Tail with Grep for Specific Info

```bash
# Only show errors
sudo docker logs -f ips-data-acquisition-api 2>&1 | grep -E "(fail|error|exception)" -i

# Only show IMU uploads
sudo docker logs -f ips-data-acquisition-api 2>&1 | grep "IMU"

# Only show GZIP operations
sudo docker logs -f ips-data-acquisition-api 2>&1 | grep "GZIP"
```

### Save Logs with Timestamps

```bash
sudo docker logs ips-data-acquisition-api 2>&1 | tee ~/api-logs-$(date +%Y%m%d-%H%M%S).txt
```

### Check Logs from Multiple Containers

```bash
# If you have multiple instances
sudo docker ps --format "{{.Names}}" | grep ips | xargs -I {} sudo docker logs --tail 50 {}
```

---

## ✅ Summary of Logging Enhancements

| Enhancement | Status |
|-------------|--------|
| Global exception handler | ✅ Added |
| Detailed log levels | ✅ Enabled |
| Per-endpoint logging | ✅ Added |
| GZIP operation logging | ✅ Enhanced |
| Database query logging | ✅ Enabled |
| Timestamps on logs | ✅ Configured |
| Stack traces in errors | ✅ Included |
| Console + Debug output | ✅ Both enabled |

**After redeployment, you'll see DETAILED error information in `docker logs`!** 🎉

---

**Current Status:** Build succeeded ✅  
**Next Step:** Push to GitHub and redeploy  
**Result:** Detailed logs will show exactly what's failing! 🔍

