# Docker Quick Start - Port 90 Production 🐳

## ✅ What Was Set Up

Following the **LeafyBot API** pattern, with **port 90** for production instead of 80.

### Files Created

1. ✅ `Dockerfile` - Multi-stage build (.NET 9.0)
2. ✅ `docker-compose.yml` - Local development (with PostgreSQL)
3. ✅ `docker-compose.prod.yml` - Production deployment (**port 90**)
4. ✅ `.dockerignore` - Exclude unnecessary files
5. ✅ `appsettings.Docker.json` - Docker environment config
6. ✅ `.github/workflows/deploy.yml` - CI/CD to AWS ECR + EC2
7. ✅ `DOCKER_DEPLOYMENT.md` - Complete documentation

---

## 🚀 Quick Start - Local Development

### 1. Run with Docker Compose

```bash
# Start API + PostgreSQL
docker-compose up

# Or in background
docker-compose up -d

# View logs
docker-compose logs -f api
```

**Access:**
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- PostgreSQL: localhost:5432

### 2. Test the API

```bash
curl -X POST http://localhost:5000/api/v1/sessions/create \
  -H "Content-Type: application/json" \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "timestamp": 1697587200000
  }'
```

### 3. Stop

```bash
docker-compose down

# Remove volumes too (clean slate)
docker-compose down -v
```

---

## ☁️ Production Deployment (Port 90)

### GitHub Secrets Required

Configure these in GitHub → Settings → Secrets and variables → Actions:

| Secret | Description | Example |
|--------|-------------|---------|
| `AWS_REGION` | AWS region | `us-east-1` |
| `AWS_ACCOUNT_ID` | AWS account ID | `123456789012` |
| `ECR_REPOSITORY` | ECR repo name | `ips-data-acquisition-api` |
| `AWS_ACCESS_KEY_ID` | IAM access key | (from AWS IAM) |
| `AWS_SECRET_ACCESS_KEY` | IAM secret key | (from AWS IAM) |
| `DB_CONNECTION_STRING` | PostgreSQL connection | `Host=db.xxx;...` |
| `EC2_HOST` | EC2 IP/hostname | `ec2-xx-xx-xx-xx.amazonaws.com` |
| `EC2_USER` | SSH username | `ubuntu` or `ec2-user` |
| `EC2_SSH_KEY` | Private SSH key | (full key content) |

### Deploy

**Push to `main` branch:**
```bash
git push origin main
```

**Or manually trigger:**
- Go to Actions tab
- Select "Build, Push to ECR, Deploy to EC2"
- Click "Run workflow"

**Result:** API live on **port 90** ✅

---

## 🔧 Key Differences from LeafyBot

| Feature | LeafyBot | IPS Data Acquisition |
|---------|----------|---------------------|
| **Production Port** | 80 | **90** ✅ |
| Internal Port | 5252 | 5000 |
| Database | PostgreSQL | PostgreSQL 16 |
| .NET Version | 9.0 | 9.0 |
| Swagger | Disabled in prod | Disabled in prod |

---

## 📊 Port Configuration

### Development
```yaml
# docker-compose.yml
ports:
  - "5000:5000"  # Host:Container
```
Access: http://localhost:5000

### Production
```yaml
# docker-compose.prod.yml
ports:
  - "90:5000"  # Host:Container
```
Access: http://your-server:90

---

## 🛠️ Dockerfile Highlights

**Two-stage build:**

1. **Build Stage** (mcr.microsoft.com/dotnet/sdk:9.0)
   - Restores NuGet packages
   - Compiles application
   - Publishes to /app/publish

2. **Runtime Stage** (mcr.microsoft.com/dotnet/aspnet:9.0)
   - Smaller image (~200MB vs ~700MB)
   - Only runtime dependencies
   - Exposes port 5000

**Benefits:**
- ✅ Smaller final image
- ✅ Faster deployments
- ✅ Better security (no build tools in production)

---

## 🔄 CI/CD Workflow

```
┌──────────────┐
│ git push main│
└──────┬───────┘
       │
       ▼
┌──────────────────────┐
│ GitHub Actions       │
│ • Build .NET app     │
│ • Create Docker image│
│ • Push to ECR        │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│ Deploy to EC2        │
│ • Pull from ECR      │
│ • docker-compose up  │
│ • Port 90 → 5000     │
└──────┬───────────────┘
       │
       ▼
  🎉 Live on port 90!
```

---

## ✅ Pre-Deployment Checklist

### AWS Setup
- [ ] ECR repository created
- [ ] EC2 instance running with Docker
- [ ] RDS PostgreSQL database created
- [ ] Security group allows port 90 inbound
- [ ] Security group allows SSH (port 22)
- [ ] IAM user with ECR permissions

### GitHub Setup
- [ ] All 9 secrets configured
- [ ] Workflow file in `.github/workflows/deploy.yml`
- [ ] Repository pushed to GitHub

### EC2 Setup
- [ ] Docker installed
- [ ] Docker Compose installed
- [ ] AWS CLI installed
- [ ] ECR login works
- [ ] Directory `~/ips-data-acquisition-api` exists

### Testing
- [ ] Local Docker build succeeds
- [ ] Local docker-compose runs
- [ ] Can create session locally
- [ ] Database migrations work

---

## 🧪 Testing Docker Locally

### Test Docker Build

```bash
# Build image
docker build -t ips-data-acquisition-api .

# Run container
docker run -d -p 5000:5000 \
  -e ConnectionStrings__Default="Host=host.docker.internal;Port=5432;Database=ips_data_acquisition;Username=postgres;Password=postgres" \
  ips-data-acquisition-api

# Check logs
docker logs <container-id>

# Test API
curl http://localhost:5000/api/v1/sessions
```

### Test Docker Compose

```bash
# Start everything
docker-compose up -d

# Check status
docker-compose ps

# Should see:
# ips-data-acquisition-api   Up   0.0.0.0:5000->5000/tcp
# ips-postgres              Up   0.0.0.0:5432->5432/tcp

# Test API
curl http://localhost:5000/api/v1/sessions

# Stop
docker-compose down
```

---

## 🔍 Troubleshooting

### Build Fails

```bash
# Check Docker is running
docker info

# Clear cache and rebuild
docker builder prune
docker build --no-cache -t ips-data-acquisition-api .
```

### Container Won't Start

```bash
# Check logs
docker logs ips-data-acquisition-api

# Common issues:
# - Database connection string wrong
# - Port already in use
# - Migrations failed
```

### Port 90 Not Accessible in Production

```bash
# On EC2, check security group
# AWS Console → EC2 → Security Groups
# Add inbound rule: TCP port 90 from 0.0.0.0/0

# Check if container is running
docker ps

# Check port mapping
docker port ips-data-acquisition-api
# Should show: 5000/tcp -> 0.0.0.0:90
```

### Database Connection Issues

```bash
# Local: Check PostgreSQL is running
docker-compose ps

# Production: Verify RDS connection string
# Test from EC2:
psql -h your-rds-host -U admin -d ips_data_acquisition
```

---

## 📝 Quick Commands

```bash
# Development
docker-compose up                    # Start
docker-compose down                  # Stop
docker-compose logs -f api           # View logs
docker-compose restart api           # Restart API

# Build
docker build -t ips-api .            # Build image
docker images                        # List images

# Production (on EC2)
docker ps                            # Running containers
docker logs ips-data-acquisition-api # View logs
docker restart ips-data-acquisition-api # Restart
docker-compose -f docker-compose.prod.yml up -d # Start
```

---

## 🎯 Summary

✅ **Docker setup complete** - Just like LeafyBot API  
✅ **Production port 90** - Configured and ready  
✅ **Auto-deployment** - Push to main → Deploy  
✅ **Local dev ready** - `docker-compose up`  
✅ **CI/CD configured** - GitHub Actions → ECR → EC2  

---

**Next Steps:**
1. Configure GitHub secrets
2. Set up AWS resources (ECR, EC2, RDS)
3. Push to `main` branch
4. Access API on port 90! 🚀

**Documentation:**
- Full guide: `DOCKER_DEPLOYMENT.md`
- First run: `FIRST_RUN.md`
- API docs: `README.md`

