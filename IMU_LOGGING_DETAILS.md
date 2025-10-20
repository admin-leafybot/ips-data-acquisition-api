# IMU Data Upload - Detailed Logging

## ✅ Comprehensive Logging Added for IMU Endpoint

Now you'll see **detailed logs at every step** of the IMU data upload process.

---

## 📋 What You'll See in Logs

### Full IMU Upload Flow (Success Case)

```bash
sudo docker logs -f ips-data-acquisition-api
```

**Example output:**

```
2025-10-17 14:39:10 info: Decompressing GZIP request from /api/v1/imu-data/upload, Content-Length: 8542
2025-10-17 14:39:10 info: GZIP decompressed: 8542 bytes → 32156 bytes
2025-10-17 14:39:10 info: IMU Upload Request - Points: 50, Session: abc-123-def, ContentLength: 8542 bytes, Encoding: gzip
2025-10-17 14:39:10 dbug: First IMU point - Timestamp: 1697587210100, AccelX: 0.123, GyroX: 0.012, MagX: 23.4
2025-10-17 14:39:10 info: Processing IMU data upload: 50 data points for session abc-123-def
2025-10-17 14:39:10 info: Prepared 50 IMU data records for bulk insert
2025-10-17 14:39:10 dbug: Calling SaveChangesAsync for 50 IMU records
2025-10-17 14:39:11 info: Executed DbCommand (145ms) INSERT INTO imu_data (session_id, timestamp, accel_x, ...) VALUES (...)
2025-10-17 14:39:11 info: Successfully saved 50 IMU data points to database for session abc-123-def
2025-10-17 14:39:11 info: IMU data upload SUCCESS - 50 points saved for session abc-123-def
```

### IMU Upload Flow (Error Case)

**If there's an error, you'll see:**

```
2025-10-17 14:39:10 info: Decompressing GZIP request from /api/v1/imu-data/upload, Content-Length: 8542
2025-10-17 14:39:10 fail: Failed to decompress GZIP request from /api/v1/imu-data/upload. Error: Invalid data format
System.IO.InvalidDataException: The archive entry was compressed using an unsupported compression method.
   at System.IO.Compression.GZipStream.ReadCore(Span`1 buffer)
   at System.IO.Stream.CopyTo(Stream destination, Int32 bufferSize)
   Full stack trace here...
```

Or:

```
2025-10-17 14:39:10 info: IMU Upload Request - Points: 50, Session: abc-123, ContentLength: 32156 bytes, Encoding: none
2025-10-17 14:39:10 info: Processing IMU data upload: 50 data points for session abc-123
2025-10-17 14:39:10 info: Prepared 50 IMU data records for bulk insert
2025-10-17 14:39:10 dbug: Calling SaveChangesAsync for 50 IMU records
2025-10-17 14:39:11 fail: Database error saving 50 IMU data points for session abc-123. Error: Cannot insert duplicate key
Npgsql.PostgresException (0x80004005): 23505: duplicate key value violates unique constraint "pk_imu_data"
   at Npgsql.Internal.NpgsqlConnector.ReadMessage()
   Full database error stack trace...
2025-10-17 14:39:11 fail: IMU Upload FAILED - Session: abc-123, Points: 50, Error Type: DbUpdateException, Message: ..., StackTrace: ...
```

---

## 🔍 Log Levels Explained

### What Each Level Shows

| Level | What It Logs | When to Use |
|-------|--------------|-------------|
| `dbug` (Debug) | Sample data points, detailed flow | Development, troubleshooting |
| `info` (Information) | Request received, success confirmations | Always on |
| `warn` (Warning) | Validation errors, rejected requests | Always on |
| `fail` (Error) | Exceptions, failures | Always on |

---

## 📊 IMU Upload Logging Points

### 1. **GZIP Middleware** (if compressed)
```
info: Decompressing GZIP request from /api/v1/imu-data/upload, Content-Length: 8542
info: GZIP decompressed: 8542 bytes → 32156 bytes
```

**Shows:**
- ✅ Compressed size
- ✅ Decompressed size
- ✅ Compression ratio

### 2. **Controller Entry**
```
info: IMU Upload Request - Points: 50, Session: abc-123, ContentLength: 8542 bytes, Encoding: gzip
```

**Shows:**
- ✅ Number of data points
- ✅ Session ID
- ✅ Request size
- ✅ Encoding type (gzip or none)

### 3. **First Data Point Sample** (Debug level)
```
dbug: First IMU point - Timestamp: 1697587210100, AccelX: 0.123, GyroX: 0.012, MagX: 23.4
```

**Shows:**
- ✅ Sample sensor values
- ✅ Helps verify data format

### 4. **Handler Processing**
```
info: Processing IMU data upload: 50 data points for session abc-123
info: Prepared 50 IMU data records for bulk insert
```

**Shows:**
- ✅ Handler received data
- ✅ Entity conversion complete

### 5. **Database Operation**
```
dbug: Calling SaveChangesAsync for 50 IMU records
info: Executed DbCommand (145ms) INSERT INTO imu_data ...
info: Successfully saved 50 IMU data points to database for session abc-123
```

**Shows:**
- ✅ Database operation timing
- ✅ SQL queries executed
- ✅ Number of records saved

### 6. **Final Response**
```
info: IMU data upload SUCCESS - 50 points saved for session abc-123
```

**Shows:**
- ✅ Overall success status

---

## 🔧 Error Scenarios & Logs

### Scenario 1: GZIP Decompression Failure

**Log:**
```
fail: Failed to decompress GZIP request from /api/v1/imu-data/upload. Error: Invalid data format
System.IO.InvalidDataException: The archive entry was compressed using an unsupported compression method
```

**Cause:** Corrupted GZIP data from Android app  
**Android will see:** 400 Bad Request with error details in JSON

### Scenario 2: Database Connection Error

**Log:**
```
fail: Database error saving 50 IMU data points. Error: Connection refused
Npgsql.NpgsqlException: Connection refused
   Host: your-db.rds.amazonaws.com:5432
```

**Cause:** Database unreachable  
**Android will see:** 500 Internal Server Error

### Scenario 3: Duplicate Key Error

**Log:**
```
fail: Database error saving 50 IMU data points
Npgsql.PostgresException: 23505: duplicate key value violates unique constraint
```

**Cause:** Trying to insert same record twice  
**Android will see:** 500 Internal Server Error

### Scenario 4: Null Data Points

**Log:**
```
warn: IMU upload rejected - no data points provided
```

**Cause:** Android sent empty array  
**Android will see:** 400 Bad Request

---

## 🧪 How to Debug with These Logs

### When Android App Shows Error

**On EC2, watch logs in real-time:**

```bash
sudo docker logs -f ips-data-acquisition-api 2>&1 | grep -E "(IMU|error|fail|exception)" -i
```

**Then trigger the error from Android app and you'll see:**

1. ✅ Exact request received (size, encoding, data points)
2. ✅ GZIP decompression status
3. ✅ First data point values
4. ✅ Database operation details
5. ✅ Full error stack trace if it fails
6. ✅ Timing information

### Find Specific Session

```bash
# Search for specific session ID in logs
sudo docker logs ips-data-acquisition-api 2>&1 | grep "abc-123-def"
```

### Check Last 10 IMU Uploads

```bash
sudo docker logs --tail 200 ips-data-acquisition-api 2>&1 | grep "IMU Upload"
```

---

## 📊 Example Log Analysis

### Success Pattern
```
info: IMU Upload Request - Points: 50, Session: xxx, ContentLength: 8542 bytes, Encoding: gzip
info: GZIP decompressed: 8542 bytes → 32156 bytes
info: Processing IMU data upload: 50 data points
info: Prepared 50 IMU data records for bulk insert
info: Successfully saved 50 IMU data points to database
info: IMU data upload SUCCESS - 50 points saved
```
✅ **All steps complete** - Working perfectly!

### Failure Pattern
```
info: IMU Upload Request - Points: 50, Session: xxx, ContentLength: 8542 bytes, Encoding: gzip
info: GZIP decompressed: 8542 bytes → 32156 bytes
info: Processing IMU data upload: 50 data points
info: Prepared 50 IMU data records for bulk insert
dbug: Calling SaveChangesAsync for 50 IMU records
fail: Database error saving 50 IMU data points. Error: Connection timeout
fail: IMU Upload FAILED - Session: xxx, Points: 50, Error Type: TimeoutException
```
❌ **Failed at database save** - Database connection issue!

---

## 🎯 Deploy Enhanced Logging

```bash
# Build succeeded ✅
dotnet build --configuration Release

# Push to GitHub
git add .
git commit -m "Add comprehensive IMU logging"
git push origin main
```

**After deployment runs, you'll have full visibility into:**
- ✅ Every IMU upload attempt
- ✅ GZIP compression/decompression
- ✅ Data point samples
- ✅ Database operations
- ✅ Full error details with stack traces
- ✅ Timing information

---

## 📝 Quick Commands Reference

```bash
# Real-time logs (all)
sudo docker logs -f ips-data-acquisition-api

# Real-time logs (errors only)
sudo docker logs -f ips-data-acquisition-api 2>&1 | grep -E "(fail|error)" -i

# Real-time logs (IMU only)
sudo docker logs -f ips-data-acquisition-api 2>&1 | grep "IMU"

# Last 100 lines
sudo docker logs --tail 100 ips-data-acquisition-api

# Save to file
sudo docker logs ips-data-acquisition-api > ~/imu-logs.txt 2>&1
```

---

## ✅ Summary

**Logging enhanced at:**
1. ✅ GZIP Middleware - Decompression details
2. ✅ IMU Controller - Request details, sample data
3. ✅ IMU Handler - Processing steps, database ops
4. ✅ Global Exception Handler - Full error details

**You'll now see:**
- ✅ Exact payload sizes (compressed vs decompressed)
- ✅ Number of data points in each request
- ✅ Sample sensor values
- ✅ Database query execution time
- ✅ Full error stack traces
- ✅ Success/failure status for each upload

**No more silent errors!** Every issue will be visible in the logs! 🎯

