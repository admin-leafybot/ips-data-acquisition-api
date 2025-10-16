# IMU Data Handling - Technical Notes

## ✅ Key Clarifications

### 1. All Sensor Parameters are Nullable

**ALL 61 sensor parameters** are now properly configured as nullable (`float?`, `double?`, `int?`, `bool?`) because:

- ✅ **Not all devices have all sensors** - Some phones lack magnetometer, barometer, etc.
- ✅ **Sensor availability varies** - Different Android versions and manufacturers
- ✅ **GPS is conditional** - Only populated when user slows down
- ✅ **Environmental sensors are optional** - Humidity, temperature may not be available

### 2. Decimal Precision Support

**3 decimal places are fully supported:**

| Type | C# Type | PostgreSQL Type | Precision | Example |
|------|---------|-----------------|-----------|---------|
| Sensor data | `float` | `REAL` (float4) | ~7 significant digits | 123.456 ✅ |
| GPS coordinates | `double` | `DOUBLE PRECISION` (float8) | ~15-17 significant digits | -122.419400 ✅ |
| Bonus amounts | `decimal(10,2)` | `NUMERIC(10,2)` | Exact to 2 decimals | 10.50 ✅ |

**Examples of supported values:**
```json
{
  "accel_x": 0.123,     // 3 decimals ✅
  "accel_y": 9.789,     // 3 decimals ✅
  "gyro_x": 0.012,      // 3 decimals ✅
  "pressure": 1013.25,  // 2 decimals ✅
  "latitude": 37.774900 // 6 decimals ✅
}
```

### 3. Nullable Fields Behavior

**In the API:**

✅ **Accepts null values:**
```json
{
  "session_id": "uuid-here",
  "data_points": [{
    "timestamp": 1697587210100,
    "accel_x": 0.123,
    "accel_y": 0.456,
    "accel_z": 9.789,
    "gyro_x": null,        // ✅ Allowed
    "gyro_y": null,        // ✅ Allowed
    "gyro_z": null,        // ✅ Allowed
    "mag_x": 23.4,
    "mag_y": null,         // ✅ Allowed
    "mag_z": null          // ✅ Allowed
  }]
}
```

✅ **Stores null in database:**
```sql
-- Example: Device without gyroscope
INSERT INTO imu_data (timestamp, accel_x, gyro_x, mag_x) 
VALUES (1697587210100, 0.123, NULL, 23.4);  -- ✅ NULL allowed
```

### 4. Validation Rules

**Only timestamp is validated:**
```csharp
RuleFor(p => p.Timestamp)
    .GreaterThan(0).WithMessage("timestamp must be a positive number");

// All sensor fields: NO validation - nulls are acceptable ✅
```

**Why no sensor validation?**
- Devices may not have certain sensors
- Null values are valid and expected
- Flexibility for different device capabilities

## 📊 Complete Sensor List (All Nullable)

### Calibrated Motion (15 parameters)
```csharp
public float? AccelX { get; set; }      // ✅ Nullable
public float? AccelY { get; set; }      // ✅ Nullable
public float? AccelZ { get; set; }      // ✅ Nullable
public float? GyroX { get; set; }       // ✅ Nullable
public float? GyroY { get; set; }       // ✅ Nullable
public float? GyroZ { get; set; }       // ✅ Nullable
public float? MagX { get; set; }        // ✅ Nullable
public float? MagY { get; set; }        // ✅ Nullable
public float? MagZ { get; set; }        // ✅ Nullable
public float? GravityX { get; set; }    // ✅ Nullable
public float? GravityY { get; set; }    // ✅ Nullable
public float? GravityZ { get; set; }    // ✅ Nullable
public float? LinearAccelX { get; set; } // ✅ Nullable
public float? LinearAccelY { get; set; } // ✅ Nullable
public float? LinearAccelZ { get; set; } // ✅ Nullable
```

### Uncalibrated Sensors (18 parameters)
All `float?` - Nullable ✅

### Rotation Vectors (12 parameters)
All `float?` - Nullable ✅

### Environmental (5 parameters)
All `float?` - Nullable ✅

### Activity (2 parameters)
- `int? StepCounter` - Nullable ✅
- `bool? StepDetected` - Nullable ✅

### Computed Orientation (4 parameters)
All `float?` - Nullable ✅

### GPS (5 parameters)
All `double?` or `float?` - Nullable ✅

## 🔧 Database Schema

```sql
CREATE TABLE imu_data (
    id BIGSERIAL PRIMARY KEY,
    session_id VARCHAR(36),
    timestamp BIGINT NOT NULL,  -- Only required field
    
    -- All sensor fields are NULL-able
    accel_x REAL,               -- PostgreSQL REAL = 4 bytes, ~7 digits precision ✅
    accel_y REAL,
    accel_z REAL,
    gyro_x REAL,
    gyro_y REAL,
    gyro_z REAL,
    mag_x REAL,
    mag_y REAL,
    mag_z REAL,
    -- ... 52 more nullable fields
    
    latitude DOUBLE PRECISION,  -- PostgreSQL DOUBLE = 8 bytes, ~15 digits precision ✅
    longitude DOUBLE PRECISION,
    -- ...
    
    created_at TIMESTAMP,
    updated_at TIMESTAMP
);
```

## 📝 Example API Requests

### Minimal Request (Only required fields)
```json
{
  "session_id": "uuid-here",
  "data_points": [{
    "timestamp": 1697587210100,
    "accel_x": null,
    "accel_y": null,
    "accel_z": null,
    "gyro_x": null,
    "gyro_y": null,
    "gyro_z": null,
    "mag_x": null,
    "mag_y": null,
    "mag_z": null
  }]
}
```
✅ **This is valid!** All sensor fields can be null.

### Full Precision Example
```json
{
  "session_id": "uuid-here",
  "data_points": [{
    "timestamp": 1697587210100,
    "accel_x": 0.123,      // 3 decimals ✅
    "accel_y": 0.456,      // 3 decimals ✅
    "accel_z": 9.789,      // 3 decimals ✅
    "gyro_x": 0.012,       // 3 decimals ✅
    "gyro_y": 0.034,       // 3 decimals ✅
    "gyro_z": 0.056,       // 3 decimals ✅
    "mag_x": 23.400,       // 3 decimals ✅
    "mag_y": 12.500,       // 3 decimals ✅
    "mag_z": 45.600,       // 3 decimals ✅
    "latitude": 37.774900, // 6 decimals ✅
    "longitude": -122.419400, // 6 decimals ✅
    "pressure": 1013.250   // 3 decimals ✅
  }]
}
```
✅ **All precision preserved!**

### Mixed Nullable Example (Realistic)
```json
{
  "session_id": "uuid-here",
  "data_points": [{
    "timestamp": 1697587210100,
    "accel_x": 0.123,
    "accel_y": 0.456,
    "accel_z": 9.789,
    "gyro_x": 0.012,
    "gyro_y": 0.034,
    "gyro_z": 0.056,
    "mag_x": 23.4,
    "mag_y": 12.5,
    "mag_z": 45.6,
    "gravity_x": null,       // Device doesn't have separate gravity sensor
    "gravity_y": null,
    "gravity_z": null,
    "pressure": null,        // No barometer
    "humidity": null,        // No humidity sensor
    "temperature": null,     // No temperature sensor
    "latitude": null,        // User is moving fast, no GPS
    "longitude": null
  }]
}
```
✅ **Perfect!** This is what most devices will send.

## ✅ Changes Made

1. **Domain Entity** (`IMUData.cs`)
   - Changed `float` → `float?` for all calibrated sensors
   - All 61 sensor fields now nullable

2. **Application DTOs** (`IMUDataDTOs.cs`)
   - Changed `float` → `float?` for all calibrated sensors
   - Consistent with domain model

3. **Database Context** (`ApplicationDbContext.cs`)
   - Added comments clarifying nullable fields
   - Confirmed PostgreSQL REAL supports 3 decimal precision

4. **Command Handler** (`UploadIMUDataCommandHandler.cs`)
   - Updated comments to clarify nullable handling
   - No logic changes needed (already handles nulls)

5. **Validation** (`IMUDataValidators.cs`)
   - Added comments explaining no sensor validation
   - Only timestamp is validated

## 🧪 Testing Null Handling

### Test Case 1: All Nulls (Edge Case)
```bash
curl -X POST https://localhost:5001/api/v1/imu-data/upload \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "data_points": [{
      "timestamp": 1697587210100
    }]
  }'
```
✅ **Should succeed** - All sensor fields default to null

### Test Case 2: Partial Data (Realistic)
```bash
curl -X POST https://localhost:5001/api/v1/imu-data/upload \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "data_points": [{
      "timestamp": 1697587210100,
      "accel_x": 0.123,
      "accel_y": 0.456,
      "accel_z": 9.789
    }]
  }'
```
✅ **Should succeed** - Other fields null

### Test Case 3: Full Precision
```bash
curl -X POST https://localhost:5001/api/v1/imu-data/upload \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "data_points": [{
      "timestamp": 1697587210100,
      "accel_x": 0.123,
      "accel_y": 12.345,
      "accel_z": -9.876,
      "latitude": 37.774900,
      "longitude": -122.419400
    }]
  }'
```
✅ **Should succeed** - All precision maintained

## 📊 Database Query Examples

### Check Null Values
```sql
-- Count how many records have null accel_x
SELECT COUNT(*) 
FROM imu_data 
WHERE accel_x IS NULL;

-- Get non-null sensor coverage
SELECT 
    COUNT(*) as total,
    COUNT(accel_x) as has_accel,
    COUNT(gyro_x) as has_gyro,
    COUNT(mag_x) as has_mag,
    COUNT(pressure) as has_pressure,
    COUNT(latitude) as has_gps
FROM imu_data;
```

### Precision Verification
```sql
-- Check actual precision stored
SELECT 
    accel_x,
    CAST(accel_x AS TEXT) as accel_x_text,
    latitude,
    CAST(latitude AS TEXT) as latitude_text
FROM imu_data
LIMIT 5;
```

## 🎯 Summary

✅ **All 61 sensor parameters are nullable**  
✅ **3 decimal precision fully supported**  
✅ **PostgreSQL REAL/DOUBLE types handle precision**  
✅ **No validation on sensor values (only timestamp)**  
✅ **API accepts and stores null values correctly**  
✅ **Build passes all tests**  

The API is now **fully compatible** with devices that:
- Don't have all sensors
- Have intermittent sensor readings
- Need precise decimal values (up to 3+ decimals)
- Send partial data based on availability

---

**Last Updated:** October 2025  
**Status:** ✅ Production Ready

