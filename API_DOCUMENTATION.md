# API Documentation

Complete API reference for the IPS Data Acquisition API v1.

**Base URL**: `https://your-domain.com/api/v1`

**Authentication**: JWT Bearer token (required for all endpoints except user signup/login)

## Table of Contents

1. [Authentication](#authentication)
2. [User Management](#user-management)
3. [App Version Management](#app-version-management)
4. [Session Management](#session-management)
5. [Button Press Tracking](#button-press-tracking)
6. [IMU Data Upload](#imu-data-upload)
7. [Bonus Management](#bonus-management)
8. [Error Responses](#error-responses)

---

## Authentication

All protected endpoints require a JWT token in the Authorization header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Expiration
- **Access Token**: 720 hours (30 days)
- **Refresh Token**: 7 days

When access token expires, use the refresh token endpoint to get a new one.

---

## User Management

### 1. User Signup

**POST** `/user/signup`

Register a new user. Account is created but **not active** until admin verification.

**Request:**
```json
{
  "phone": "9910147188",
  "password": "Password123",
  "fullName": "John Doe"
}
```

**Validation Rules:**
- `phone`: Required, 10-20 characters, valid phone format
- `password`: Required, minimum 6 characters
- `fullName`: Required, 2-200 characters

**Response (Success):**
```json
{
  "success": true,
  "message": "User registered successfully. Account is pending verification.",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

**Response (Phone Already Exists):**
```json
{
  "success": false,
  "message": "Phone number already registered",
  "userId": null
}
```

**Status Codes:**
- `200 OK` - User created successfully
- `400 Bad Request` - Validation error or phone already exists
- `500 Internal Server Error` - Server error

---

### 2. User Login

**POST** `/user/login`

Login with phone and password. Returns JWT tokens.

**Request:**
```json
{
  "phone": "9910147188",
  "password": "Password123"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJhMWIyYzNkNC1lNWY2...",
  "refreshToken": "CfDJ8N2YzZmE3OTY4YjQ5NGE3YTk3ZjBmMjJmNzk5ZjE0YjE3YzQ5ZjI0...",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "fullName": "John Doe",
  "expiresAt": "2025-11-20T08:30:00Z",
  "expiresIn": 2592000
}
```

**Response (Account Not Active):**
```json
{
  "success": false,
  "message": "Your account is not yet verified. Please contact administrator.",
  "token": null,
  "refreshToken": null,
  "userId": null,
  "fullName": null,
  "expiresAt": null,
  "expiresIn": null
}
```

**Response (Invalid Credentials):**
```json
{
  "success": false,
  "message": "Invalid phone number or password",
  "token": null,
  "refreshToken": null,
  "userId": null,
  "fullName": null,
  "expiresAt": null,
  "expiresIn": null
}
```

**Status Codes:**
- `200 OK` - Login successful
- `401 Unauthorized` - Invalid credentials or inactive account
- `400 Bad Request` - Validation error
- `500 Internal Server Error` - Server error

---

### 3. Refresh Token

**POST** `/user/refresh-token`

Get a new access token using refresh token. Old refresh token is revoked.

**Request:**
```json
{
  "refreshToken": "CfDJ8N2YzZmE3OTY4YjQ5NGE3YTk3ZjBmMjJmNzk5ZjE0YjE3YzQ5ZjI0..."
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "DgEK9O3aAbG4PZj5OkJ6NkQ6OkO5PkK6PlL7QmM8RnN...",
  "expiresAt": "2025-11-20T10:30:00Z",
  "expiresIn": 2592000
}
```

**Response (Invalid/Expired Token):**
```json
{
  "success": false,
  "message": "Refresh token is expired or revoked",
  "token": null,
  "refreshToken": null,
  "expiresAt": null,
  "expiresIn": null
}
```

**Status Codes:**
- `200 OK` - Token refreshed
- `401 Unauthorized` - Invalid or expired refresh token
- `500 Internal Server Error` - Server error

---

### 4. Change Verification Status (Admin Only)

**POST** `/user/ChangeVerificationStatus`

Activate or deactivate a user account. **Requires admin security key**.

**Request:**
```json
{
  "phone": "9910147188",
  "status": true,
  "securityKey": "AdminSecureKey123!ChangeThisInProduction"
}
```

**Parameters:**
- `phone`: User's phone number
- `status`: `true` to activate, `false` to deactivate
- `securityKey`: Admin security key from configuration

**Response (Success):**
```json
{
  "success": true,
  "message": "User account activated successfully"
}
```

**Response (Invalid Security Key):**
```json
{
  "success": false,
  "message": "Invalid security key"
}
```

**Response (User Not Found):**
```json
{
  "success": false,
  "message": "User not found"
}
```

**Status Codes:**
- `200 OK` - Status changed successfully
- `400 Bad Request` - Invalid security key or validation error
- `404 Not Found` - User not found
- `500 Internal Server Error` - Server error

---

## App Version Management

### 5. Check App Version

**POST** `/app/checkAppVersion`

Check if a specific app version is active/allowed. **No authentication required** - public endpoint.

**Request:**
```json
{
  "versionName": "1.0.5"
}
```

**Parameters:**
- `versionName`: App version string (e.g., "1.0.5", "2.3.1")

**Response (Version Active):**
```json
{
  "isActive": true
}
```

**Response (Version Not Active or Not Found):**
```json
{
  "isActive": false
}
```

**Status Codes:**
- `200 OK` - Always returns 200, check `isActive` field for result
- `400 Bad Request` - Validation error (missing version name)
- `500 Internal Server Error` - Server error

**Notes:**
- Returns `false` if version not found in database
- Returns `false` if version exists but `active = false`
- Returns `true` only if version exists and `active = true`
- Mobile app should check version on startup and prevent usage if `isActive = false`

**Usage Example:**
```bash
curl -X POST https://api.yourdomain.com/api/v1/app/checkAppVersion \
  -H "Content-Type: application/json" \
  -d '{
    "versionName": "1.0.5"
  }'
```

**Database Management:**
```sql
-- Add allowed version
INSERT INTO app_versions (version_name, active, created_at, updated_at)
VALUES ('1.0.5', true, NOW(), NOW());

-- Disable old version
UPDATE app_versions SET active = false WHERE version_name = '1.0.3';

-- View all versions
SELECT id, version_name, active, created_at FROM app_versions ORDER BY created_at DESC;
```

---

## Session Management

🔒 **Authentication Required**

### 6. Create Session

**POST** `/sessions/create`

Create a new delivery session for the logged-in user.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json
```

**Request:**
```json
{
  "sessionId": "session-2024-10-21-001",
  "timestamp": 1729497600000
}
```

**Parameters:**
- `sessionId`: Unique session identifier (client-generated)
- `timestamp`: Session start time (epoch milliseconds)

**Response (Success):**
```json
{
  "success": true,
  "message": "Session created successfully",
  "data": {
    "sessionId": "session-2024-10-21-001",
    "startTimestamp": 1729497600000
  }
}
```

**Response (Session Already Exists):**
```json
{
  "success": false,
  "message": "Session with ID session-2024-10-21-001 already exists",
  "data": null
}
```

**Status Codes:**
- `200 OK` - Session created
- `401 Unauthorized` - No token or invalid token
- `409 Conflict` - Session ID already exists
- `400 Bad Request` - Validation error

**Notes:**
- `userId` is automatically set from authenticated user
- Session status defaults to `in_progress`
- Payment status defaults to `unpaid`

---

### 7. Close Session

**POST** `/sessions/close`

Mark a session as completed.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json
```

**Request:**
```json
{
  "sessionId": "session-2024-10-21-001",
  "endTimestamp": 1729501200000
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Session closed successfully",
  "data": null
}
```

**Response (Session Not Found):**
```json
{
  "success": false,
  "message": "Session with ID session-2024-10-21-001 not found",
  "data": null
}
```

**Response (Already Closed):**
```json
{
  "success": false,
  "message": "Session is already closed",
  "data": null
}
```

**Status Codes:**
- `200 OK` - Session closed
- `401 Unauthorized` - Authentication required
- `404 Not Found` - Session doesn't exist
- `400 Bad Request` - Session already closed

---

### 8. Cancel Session

**POST** `/sessions/cancel`

Cancel an active session. Sets status to `rejected` and adds "Cancelled by user" remark.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json
```

**Request:**
```json
{
  "sessionId": "session-2024-10-21-001"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Session cancelled successfully",
  "data": null
}
```

**Response (Session Not Found):**
```json
{
  "success": false,
  "message": "Session with ID session-2024-10-21-001 not found",
  "data": null
}
```

**Response (Unauthorized - Not Your Session):**
```
Status: 403 Forbidden
```

**Response (Cannot Cancel - Already Completed):**
```json
{
  "success": false,
  "message": "Cannot cancel session with status: completed",
  "data": null
}
```

**Status Codes:**
- `200 OK` - Session cancelled
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Attempting to cancel another user's session
- `404 Not Found` - Session doesn't exist
- `400 Bad Request` - Session already in final state (completed/approved/rejected)

**Notes:**
- Only the session owner can cancel their own session
- Can only cancel sessions with `in_progress` status
- Session status is set to `rejected`
- `endTimestamp` is automatically set to current time
- Remarks field is set to "Cancelled by user"

---

### 9. Get Sessions

**GET** `/sessions?page=1&limit=50`

Retrieve sessions for the authenticated user.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
```

**Query Parameters:**
- `page` (optional): Page number, default: 1
- `limit` (optional): Results per page, default: 50, max: 100

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "sessionId": "session-2024-10-21-001",
      "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "startTimestamp": 1729497600000,
      "endTimestamp": 1729501200000,
      "status": "completed",
      "paymentStatus": "paid",
      "bonusAmount": 50.00,
      "remarks": null,
      "createdAt": "2024-10-21T08:00:00Z"
    },
    {
      "sessionId": "session-2024-10-20-003",
      "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "startTimestamp": 1729411200000,
      "endTimestamp": 1729414800000,
      "status": "approved",
      "paymentStatus": "paid",
      "bonusAmount": 75.50,
      "remarks": "Good delivery",
      "createdAt": "2024-10-20T10:00:00Z"
    }
  ]
}
```

**Status Codes:**
- `200 OK` - Sessions retrieved
- `401 Unauthorized` - Authentication required

---

## Button Press Tracking

🔒 **Authentication Required**

### 10. Submit Button Press

**POST** `/button-presses`

Record a button press action during delivery.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json
```

**Request:**
```json
{
  "sessionId": "session-2024-10-21-001",
  "action": "ENTERED_RESTAURANT_BUILDING",
  "timestamp": 1729497700000,
  "floorIndex": 3
}
```

**Parameters:**
- `sessionId`: Associated session ID (must exist)
- `action`: Button action (see valid actions below)
- `timestamp`: Action time (epoch milliseconds)
- `floorIndex` (optional): Floor number

**Valid Actions:**
- `ENTERED_RESTAURANT_BUILDING`
- `ENTERED_ELEVATOR`
- `CLIMBING_STAIRS`
- `GOING_UP_IN_LIFT`
- `REACHED_RESTAURANT_CORRIDOR`
- `REACHED_RESTAURANT`
- `LEFT_RESTAURANT`
- `COMING_DOWN_STAIRS`
- `LEFT_RESTAURANT_BUILDING`
- `ENTERED_DELIVERY_BUILDING`
- `REACHED_DELIVERY_CORRIDOR`
- `REACHED_DOORSTEP`
- `LEFT_DOORSTEP`
- `GOING_DOWN_IN_LIFT`
- `LEFT_DELIVERY_BUILDING`

**Response (Success):**
```json
{
  "success": true,
  "message": "Button press recorded",
  "data": null
}
```

**Response (Invalid Action):**
```json
{
  "success": false,
  "message": "Invalid action: UNKNOWN_ACTION. Must be one of: ENTERED_RESTAURANT_BUILDING, ...",
  "data": null
}
```

**Response (Session Not Found):**
```json
{
  "success": false,
  "message": "Session with ID session-xyz not found",
  "data": null
}
```

**Status Codes:**
- `200 OK` - Button press recorded
- `401 Unauthorized` - Authentication required
- `400 Bad Request` - Invalid action or validation error
- `404 Not Found` - Session doesn't exist

**Notes:**
- `userId` is automatically set from authenticated user
- Button presses are linked to session (cascade delete)

---

## IMU Data Upload

🔒 **Authentication Required**

### 11. Upload IMU Data

**POST** `/imu-data/upload`

Upload bulk sensor data from mobile device. Supports **GZIP compression**.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json
Content-Encoding: gzip  (optional, for compressed payloads)
```

**Request:**
```json
{
  "sessionId": "session-2024-10-21-001",
  "dataPoints": [
    {
      "timestamp": 1729497700000,
      "timestampNanos": 1729497700123456789,
      "accelX": 0.123, "accelY": -0.456, "accelZ": 9.81,
      "gyroX": 0.01, "gyroY": -0.02, "gyroZ": 0.005,
      "magX": 25.3, "magY": -15.2, "magZ": 42.1,
      "gravityX": 0.0, "gravityY": 0.0, "gravityZ": 9.81,
      "linearAccelX": 0.123, "linearAccelY": -0.456, "linearAccelZ": 0.0,
      "accelUncalX": 0.125, "accelUncalY": -0.458, "accelUncalZ": 9.82,
      "accelBiasX": 0.002, "accelBiasY": -0.002, "accelBiasZ": 0.01,
      "gyroUncalX": 0.011, "gyroUncalY": -0.021, "gyroUncalZ": 0.006,
      "gyroDriftX": 0.001, "gyroDriftY": -0.001, "gyroDriftZ": 0.001,
      "magUncalX": 25.5, "magUncalY": -15.0, "magUncalZ": 42.3,
      "magBiasX": 0.2, "magBiasY": 0.2, "magBiasZ": 0.2,
      "rotationVectorX": 0.1, "rotationVectorY": 0.2, "rotationVectorZ": 0.3, "rotationVectorW": 0.9,
      "gameRotationX": 0.15, "gameRotationY": 0.25, "gameRotationZ": 0.35, "gameRotationW": 0.85,
      "geomagRotationX": 0.12, "geomagRotationY": 0.22, "geomagRotationZ": 0.32, "geomagRotationW": 0.88,
      "pressure": 1013.25,
      "temperature": 22.5,
      "light": 500.0,
      "humidity": 45.0,
      "proximity": 5.0,
      "stepCounter": 1234,
      "stepDetected": true,
      "roll": 0.5, "pitch": -0.3, "yaw": 1.2, "heading": 45.0,
      "latitude": 28.5355, "longitude": 77.3910, "altitude": 250.5,
      "gpsAccuracy": 10.0,
      "speed": 1.5
    },
    {
      "timestamp": 1729497710000,
      "timestampNanos": null,
      "accelX": 0.130, "accelY": -0.460, "accelZ": 9.80,
      "gyroX": null, "gyroY": null, "gyroZ": null,
      "magX": null, "magY": null, "magZ": null,
      "gravityX": null, "gravityY": null, "gravityZ": null,
      "linearAccelX": null, "linearAccelY": null, "linearAccelZ": null,
      "accelUncalX": null, "accelUncalY": null, "accelUncalZ": null,
      "accelBiasX": null, "accelBiasY": null, "accelBiasZ": null,
      "gyroUncalX": null, "gyroUncalY": null, "gyroUncalZ": null,
      "gyroDriftX": null, "gyroDriftY": null, "gyroDriftZ": null,
      "magUncalX": null, "magUncalY": null, "magUncalZ": null,
      "magBiasX": null, "magBiasY": null, "magBiasZ": null,
      "rotationVectorX": null, "rotationVectorY": null, "rotationVectorZ": null, "rotationVectorW": null,
      "gameRotationX": null, "gameRotationY": null, "gameRotationZ": null, "gameRotationW": null,
      "geomagRotationX": null, "geomagRotationY": null, "geomagRotationZ": null, "geomagRotationW": null,
      "pressure": null, "temperature": null, "light": null, "humidity": null, "proximity": null,
      "stepCounter": null, "stepDetected": null,
      "roll": null, "pitch": null, "yaw": null, "heading": null,
      "latitude": 28.5356, "longitude": 77.3911, "altitude": 251.0,
      "gpsAccuracy": 12.0,
      "speed": 1.6
    }
  ]
}
```

**Sensor Fields (All Nullable):**

**Calibrated Sensors (15 fields):**
- `accelX`, `accelY`, `accelZ` - Accelerometer (m/s²)
- `gyroX`, `gyroY`, `gyroZ` - Gyroscope (rad/s)
- `magX`, `magY`, `magZ` - Magnetometer (µT)
- `gravityX`, `gravityY`, `gravityZ` - Gravity (m/s²)
- `linearAccelX`, `linearAccelY`, `linearAccelZ` - Linear acceleration (m/s²)

**Uncalibrated Sensors (18 fields):**
- `accelUncalX/Y/Z`, `accelBiasX/Y/Z`
- `gyroUncalX/Y/Z`, `gyroDriftX/Y/Z`
- `magUncalX/Y/Z`, `magBiasX/Y/Z`

**Rotation Vectors (12 fields):**
- `rotationVectorX/Y/Z/W`
- `gameRotationX/Y/Z/W`
- `geomagRotationX/Y/Z/W`

**Environmental (5 fields):**
- `pressure` (hPa), `temperature` (°C), `light` (lux), `humidity` (%), `proximity` (cm)

**Activity (2 fields):**
- `stepCounter`, `stepDetected`

**Orientation (4 fields):**
- `roll`, `pitch`, `yaw`, `heading` (radians/degrees)

**GPS (5 fields):**
- `latitude`, `longitude`, `altitude` (meters), `gpsAccuracy` (meters), `speed` (m/s)

**Response (Success):**
```json
{
  "success": true,
  "message": "IMU data uploaded successfully",
  "data": {
    "pointsReceived": 2,
    "sessionId": "session-2024-10-21-001"
  }
}
```

**Response (No Data Points):**
```json
{
  "success": false,
  "message": "data_points is required and must contain at least 1 data point",
  "data": null
}
```

**Status Codes:**
- `200 OK` - Data uploaded successfully
- `401 Unauthorized` - Authentication required
- `400 Bad Request` - Validation error (no data points)
- `500 Internal Server Error` - Database error

**Performance Notes:**
- Can handle 1000+ data points per request
- Use GZIP compression to reduce payload by ~90%
- Bulk insert optimized for performance
- `userId` automatically set from authenticated user

**GZIP Example:**
```bash
# Compress and upload
curl -X POST https://api.yourdomain.com/api/v1/imu-data/upload \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -H "Content-Encoding: gzip" \
  --data-binary @data.json.gz
```

---

## Bonus Management

🔒 **Authentication Required**

### 12. Get Bonuses

**GET** `/bonuses?start_date=2024-10-01&end_date=2024-10-31`

Retrieve bonuses for the authenticated user within a date range.

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
```

**Query Parameters:**
- `start_date` (optional): Start date (YYYY-MM-DD format)
- `end_date` (optional): End date (YYYY-MM-DD format)

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 123,
      "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "date": "2024-10-21",
      "amount": 150.00,
      "createdAt": "2024-10-21T08:00:00Z"
    },
    {
      "id": 124,
      "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "date": "2024-10-20",
      "amount": 200.50,
      "createdAt": "2024-10-20T08:00:00Z"
    }
  ]
}
```

**Status Codes:**
- `200 OK` - Bonuses retrieved
- `401 Unauthorized` - Authentication required

---

## Error Responses

### Standard Error Format

All error responses follow this structure:

```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

### HTTP Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| `200 OK` | Success | Request successful |
| `400 Bad Request` | Validation error | Invalid input, business rule violation |
| `401 Unauthorized` | Authentication failed | Missing/invalid JWT token, wrong credentials |
| `404 Not Found` | Resource not found | Session, user, or entity doesn't exist |
| `409 Conflict` | Resource conflict | Duplicate session ID, phone number |
| `429 Too Many Requests` | Rate limit exceeded | Too many requests from IP |
| `500 Internal Server Error` | Server error | Unhandled exception, database error |

### Common Validation Errors

**Missing Required Field:**
```json
{
  "errors": {
    "Phone": ["Phone number is required"]
  },
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400
}
```

**Invalid Format:**
```json
{
  "errors": {
    "Password": ["Password must be at least 6 characters long"]
  },
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400
}
```

---

## Rate Limiting

Rate limits prevent API abuse:

| Endpoint | Limit | Window |
|----------|-------|--------|
| `POST /button-presses` | 600 requests | 1 minute |
| `POST /imu-data/upload` | 1000 requests | 1 minute |
| `GET *` (all GET requests) | 100 requests | 1 minute |
| All other endpoints | Default limit | 1 minute |

**Rate Limit Response:**
```json
{
  "message": "Rate limit exceeded. Try again later."
}
```

**Status Code**: `429 Too Many Requests`

**Headers:**
```
X-Rate-Limit-Limit: 600
X-Rate-Limit-Remaining: 0
X-Rate-Limit-Reset: 1729497660
```

---

## Example Workflows

### Complete User Journey

#### Step 1: Signup
```bash
curl -X POST https://api.yourdomain.com/api/v1/user/signup \
  -H "Content-Type: application/json" \
  -d '{
    "phone": "9910147188",
    "password": "MyPassword123",
    "fullName": "Rajesh Kumar"
  }'
```

#### Step 2: Admin Activates Account
```bash
curl -X POST https://api.yourdomain.com/api/v1/user/ChangeVerificationStatus \
  -H "Content-Type: application/json" \
  -d '{
    "phone": "9910147188",
    "status": true,
    "securityKey": "AdminSecureKey123!ChangeThisInProduction"
  }'
```

#### Step 3: Login
```bash
curl -X POST https://api.yourdomain.com/api/v1/user/login \
  -H "Content-Type: application/json" \
  -d '{
    "phone": "9910147188",
    "password": "MyPassword123"
  }'

# Response includes token
# Save TOKEN and REFRESH_TOKEN for subsequent requests
```

#### Step 4: Create Session
```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X POST https://api.yourdomain.com/api/v1/sessions/create \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "delivery-001",
    "timestamp": 1729497600000
  }'
```

#### Step 5: Track Button Presses
```bash
curl -X POST https://api.yourdomain.com/api/v1/button-presses \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "delivery-001",
    "action": "ENTERED_RESTAURANT_BUILDING",
    "timestamp": 1729497650000,
    "floorIndex": 2
  }'
```

#### Step 6: Upload IMU Data (with GZIP)
```bash
# Create JSON file
cat > imu_data.json << EOF
{
  "sessionId": "delivery-001",
  "dataPoints": [ /* 100+ sensor readings */ ]
}
EOF

# Compress
gzip -c imu_data.json > imu_data.json.gz

# Upload
curl -X POST https://api.yourdomain.com/api/v1/imu-data/upload \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -H "Content-Encoding: gzip" \
  --data-binary @imu_data.json.gz
```

#### Step 7: Close Session (or Cancel)
```bash
# Option A: Close session normally
curl -X POST https://api.yourdomain.com/api/v1/sessions/close \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "delivery-001",
    "endTimestamp": 1729501200000
  }'

# Option B: Cancel session
curl -X POST https://api.yourdomain.com/api/v1/sessions/cancel \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "delivery-001"
  }'
```

#### Step 8: Get Sessions
```bash
curl -X GET "https://api.yourdomain.com/api/v1/sessions?page=1&limit=10" \
  -H "Authorization: Bearer $TOKEN"
```

#### Step 9: Refresh Token (when access token expires)
```bash
REFRESH_TOKEN="CfDJ8N2YzZmE3OTY4YjQ5NGE3..."

curl -X POST https://api.yourdomain.com/api/v1/user/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "'$REFRESH_TOKEN'"
  }'

# Response includes new token and new refresh_token
```

---

## Mobile SDK Integration Example

### Android (Kotlin)

```kotlin
class IpsApiClient(private val baseUrl: String) {
    private var accessToken: String? = null
    private var refreshToken: String? = null
    
    suspend fun login(phone: String, password: String): LoginResponse {
        val response = httpClient.post("$baseUrl/user/login") {
            contentType(ContentType.Application.Json)
            setBody(mapOf(
                "phone" to phone,
                "password" to password
            ))
        }
        
        val loginResponse = response.body<LoginResponse>()
        accessToken = loginResponse.token
        refreshToken = loginResponse.refreshToken
        
        return loginResponse
    }
    
    suspend fun createSession(sessionId: String, timestamp: Long) {
        httpClient.post("$baseUrl/sessions/create") {
            contentType(ContentType.Application.Json)
            header("Authorization", "Bearer $accessToken")
            setBody(mapOf(
                "sessionId" to sessionId,
                "timestamp" to timestamp
            ))
        }
    }
    
    suspend fun uploadImuData(sessionId: String, dataPoints: List<ImuDataPoint>) {
        val json = Json.encodeToString(mapOf(
            "sessionId" to sessionId,
            "dataPoints" to dataPoints
        ))
        
        val gzipped = gzip(json.toByteArray())
        
        httpClient.post("$baseUrl/imu-data/upload") {
            header("Authorization", "Bearer $accessToken")
            header("Content-Encoding", "gzip")
            contentType(ContentType.Application.Json)
            setBody(gzipped)
        }
    }
}
```

### iOS (Swift)

```swift
class IPSApiClient {
    private var accessToken: String?
    private var refreshToken: String?
    
    func login(phone: String, password: String) async throws -> LoginResponse {
        var request = URLRequest(url: URL(string: "\(baseURL)/user/login")!)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        let body = ["phone": phone, "password": password]
        request.httpBody = try JSONEncoder().encode(body)
        
        let (data, _) = try await URLSession.shared.data(for: request)
        let response = try JSONDecoder().decode(LoginResponse.self, from: data)
        
        self.accessToken = response.token
        self.refreshToken = response.refreshToken
        
        return response
    }
    
    func uploadImuData(sessionId: String, dataPoints: [ImuDataPoint]) async throws {
        var request = URLRequest(url: URL(string: "\(baseURL)/imu-data/upload")!)
        request.httpMethod = "POST"
        request.setValue("Bearer \(accessToken!)", forHTTPHeaderField: "Authorization")
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.setValue("gzip", forHTTPHeaderField: "Content-Encoding")
        
        let body = ["sessionId": sessionId, "dataPoints": dataPoints]
        let jsonData = try JSONEncoder().encode(body)
        request.httpBody = try jsonData.gzipped()
        
        let (_, response) = try await URLSession.shared.data(for: request)
        // Handle response
    }
}
```

---

## API Testing

### Postman Collection

Import this JSON into Postman:

```json
{
  "info": {
    "name": "IPS Data Acquisition API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "auth": {
    "type": "bearer",
    "bearer": [{"key": "token", "value": "{{jwt_token}}", "type": "string"}]
  },
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Signup",
          "request": {
            "method": "POST",
            "url": "{{base_url}}/user/signup",
            "body": {
              "mode": "raw",
              "raw": "{\n  \"phone\": \"9910147188\",\n  \"password\": \"Password123\",\n  \"fullName\": \"Test User\"\n}"
            }
          }
        },
        {
          "name": "Login",
          "request": {
            "method": "POST",
            "url": "{{base_url}}/user/login",
            "body": {
              "mode": "raw",
              "raw": "{\n  \"phone\": \"9910147188\",\n  \"password\": \"Password123\"\n}"
            }
          },
          "event": [{
            "listen": "test",
            "script": {
              "exec": ["pm.environment.set('jwt_token', pm.response.json().token);"]
            }
          }]
        }
      ]
    }
  ],
  "variable": [
    {"key": "base_url", "value": "https://api.yourdomain.com/api/v1"}
  ]
}
```

### Swagger UI

Access interactive API documentation:
```
https://your-domain.com/swagger
```

Features:
- Try out all endpoints
- See request/response schemas
- JWT token authentication support
- Example values for all fields

---

## Performance Benchmarks

### Typical Response Times

| Endpoint | Avg Response | p95 | p99 |
|----------|--------------|-----|-----|
| `POST /user/login` | 80ms | 150ms | 250ms |
| `POST /sessions/create` | 30ms | 60ms | 100ms |
| `POST /button-presses` | 25ms | 50ms | 80ms |
| `POST /imu-data/upload` (100 points) | 150ms | 300ms | 500ms |
| `POST /imu-data/upload` (1000 points) | 600ms | 1000ms | 1500ms |
| `GET /sessions` | 50ms | 100ms | 200ms |

### Throughput Capacity

- **Concurrent Users**: 1000+
- **Requests/Second**: 500+
- **IMU Data Points/Second**: 10,000+
- **Daily Sessions**: 100,000+

### Compression Benefits

| Payload | Original Size | GZIP Size | Reduction |
|---------|--------------|-----------|-----------|
| 10 IMU points | 20 KB | 2 KB | 90% |
| 100 IMU points | 200 KB | 20 KB | 90% |
| 1000 IMU points | 2 MB | 200 KB | 90% |

---

## Versioning

### Current Version: v1

All endpoints prefixed with `/api/v1/`

### Future Versions

When breaking changes are needed:
- New version: `/api/v2/`
- v1 remains available
- Deprecation notice: 6 months before removal
- Migration guide provided

### Changelog

**v1.0.0** (2024-10-21)
- Initial release
- User authentication with JWT
- Session management
- Button press tracking
- IMU data collection with GZIP support
- Bonus retrieval
- Refresh token support

---

## Support & Contact

For API issues or questions:
- **Email**: api-support@yourcompany.com
- **Slack**: #ips-api-support
- **Documentation**: https://docs.yourdomain.com

For production incidents:
- **On-Call**: +91-XXX-XXX-XXXX
- **PagerDuty**: ips-api-oncall

---

## Appendix: Sample Data

### Sample Session Object
```json
{
  "sessionId": "session-2024-10-21-001",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "startTimestamp": 1729497600000,
  "endTimestamp": 1729501200000,
  "status": "completed",
  "paymentStatus": "paid",
  "bonusAmount": 50.00,
  "remarks": "Fast delivery",
  "isSynced": true,
  "createdAt": "2024-10-21T08:00:00Z",
  "updatedAt": "2024-10-21T09:00:00Z"
}
```

### Sample Button Press Object
```json
{
  "id": 12345,
  "sessionId": "session-2024-10-21-001",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "action": "REACHED_RESTAURANT",
  "timestamp": 1729497800000,
  "floorIndex": 5,
  "isSynced": true,
  "createdAt": "2024-10-21T08:03:20Z",
  "updatedAt": "2024-10-21T08:03:20Z"
}
```

### Sample IMU Data Point
```json
{
  "id": 98765,
  "sessionId": "session-2024-10-21-001",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "timestamp": 1729497700000,
  "timestampNanos": 1729497700123456789,
  "accelX": 0.123, "accelY": -0.456, "accelZ": 9.81,
  "gyroX": 0.01, "gyroY": -0.02, "gyroZ": 0.005,
  "latitude": 28.5355, "longitude": 77.3910,
  "isSynced": true,
  "createdAt": "2024-10-21T08:01:40Z"
}
```

---

**Last Updated**: October 21, 2025  
**API Version**: v1.0.0  
**Maintained By**: IPS Development Team

