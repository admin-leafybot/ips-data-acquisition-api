# Docker Deployment Guide

## 🐳 Docker Setup

This project includes complete Docker containerization and CI/CD deployment similar to LeafyBot API.

### Key Differences from LeafyBot
- **Production Port:** 90 (instead of 80)
- **Container Port:** 5000 (internal)
- **Database:** PostgreSQL 16

---

## 📁 Docker Files

### 1. Dockerfile
Multi-stage build:
- **Build stage:** .NET 9.0 SDK compiles the application
- **Runtime stage:** .NET 9.0 ASP.NET runtime (smaller image)
- **Internal port:** 5000

### 2. docker-compose.yml (Development)
Local development setup with PostgreSQL:
- API on port 5000
- PostgreSQL on port 5432
- Automatic database creation

### 3. docker-compose.prod.yml (Production)
Production deployment:
- Uses ECR image
- Maps to port **90** externally
- Restart policy: `unless-stopped`

### 4. .dockerignore
Excludes unnecessary files from Docker build context

### 5. appsettings.Production.json
Docker-specific configuration with placeholders for secrets

---

## 🚀 Local Development with Docker

### Run with Docker Compose

```bash
# Build and start API + PostgreSQL
docker-compose up --build

# Or in detached mode
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v
```

### Access the Application

- **API:** http://localhost:5000
- **Swagger:** http://localhost:5000/swagger
- **PostgreSQL:** localhost:5432

### Test the API

```bash
# Create session
curl -X POST http://localhost:5000/api/v1/sessions/create \
  -H "Content-Type: application/json" \
  -d '{
    "session_id": "123e4567-e89b-12d3-a456-426614174000",
    "timestamp": 1697587200000
  }'
```

---

## ☁️ Production Deployment (AWS ECR + EC2)

### Prerequisites

1. **AWS Resources:**
   - ECR Repository created
   - EC2 instance with Docker installed
   - IAM credentials with ECR access

2. **GitHub Secrets Required:**

| Secret | Description | Example |
|--------|-------------|---------|
| `AWS_REGION` | AWS region | `us-east-1` |
| `AWS_ACCOUNT_ID` | AWS account ID | `123456789012` |
| `ECR_REPOSITORY` | ECR repository name | `ips-data-acquisition-api` |
| `AWS_ACCESS_KEY_ID` | AWS access key | (from IAM) |
| `AWS_SECRET_ACCESS_KEY` | AWS secret key | (from IAM) |
| `DB_CONNECTION_STRING` | PostgreSQL connection | `Host=db.xxx.rds.amazonaws.com;Port=5432;Database=ips;Username=admin;Password=xxx` |
| `EC2_HOST` | EC2 instance IP/hostname | `ec2-xx-xx-xx-xx.compute.amazonaws.com` |
| `EC2_USER` | SSH username | `ec2-user` or `ubuntu` |
| `EC2_SSH_KEY` | Private SSH key | (full private key content) |

### GitHub Workflow

**Trigger:**
- Push to `main` branch
- Manual trigger via Actions tab

**Process:**
1. **Build & Push:**
   - Replace placeholders in appsettings
   - Build Docker image
   - Push to ECR

2. **Deploy:**
   - Prepare docker-compose.prod.yml (port 90)
   - Upload to EC2
   - Pull image and restart containers

### Manual Deployment

```bash
# 1. Build image
docker build -t ips-data-acquisition-api .

# 2. Tag for ECR
docker tag ips-data-acquisition-api:latest \
  123456789012.dkr.ecr.us-east-1.amazonaws.com/ips-data-acquisition-api:latest

# 3. Login to ECR
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin \
  123456789012.dkr.ecr.us-east-1.amazonaws.com

# 4. Push to ECR
docker push 123456789012.dkr.ecr.us-east-1.amazonaws.com/ips-data-acquisition-api:latest

# 5. On EC2, pull and run
docker pull 123456789012.dkr.ecr.us-east-1.amazonaws.com/ips-data-acquisition-api:latest
docker-compose -f docker-compose.prod.yml up -d
```

---

## 🔧 Configuration

### Environment Variables

Set in docker-compose or via environment:

```bash
# Development
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__Default=Host=db;Port=5432;Database=ips_data_acquisition;Username=postgres;Password=postgres

# Production (handled by appsettings.Production.json)
ASPNETCORE_ENVIRONMENT=Production
```

### Port Mapping

| Environment | External Port | Internal Port | Access |
|-------------|---------------|---------------|--------|
| Development | 5000 | 5000 | http://localhost:5000 |
| **Production** | **90** | 5000 | http://your-server:90 |

### Database Connection

**Development (docker-compose):**
```
Host=db;Port=5432;Database=ips_data_acquisition;Username=postgres;Password=postgres
```

**Production:**
```
Host=your-rds.amazonaws.com;Port=5432;Database=ips;Username=admin;Password=SecurePassword
```

---

## 📊 Monitoring

### View Logs

```bash
# Docker Compose
docker-compose logs -f api

# Production (on EC2)
docker logs -f ips-data-acquisition-api

# Last 100 lines
docker logs --tail 100 ips-data-acquisition-api
```

### Health Check

```bash
# Development
curl http://localhost:5000/api/v1/sessions

# Production
curl http://your-server:90/api/v1/sessions
```

### Container Stats

```bash
docker stats ips-data-acquisition-api
```

---

## 🛠️ Troubleshooting

### Build Fails

```bash
# Clear Docker cache
docker builder prune

# Build with no cache
docker build --no-cache -t ips-data-acquisition-api .
```

### Database Connection Issues

```bash
# Check if PostgreSQL is running
docker-compose ps

# Check logs
docker-compose logs db

# Verify connection from API container
docker exec ips-data-acquisition-api \
  psql -h db -U postgres -d ips_data_acquisition -c "\dt"
```

### Container Won't Start

```bash
# Check logs
docker logs ips-data-acquisition-api

# Inspect container
docker inspect ips-data-acquisition-api

# Remove and recreate
docker rm -f ips-data-acquisition-api
docker-compose up -d
```

### Production Port 90 Not Accessible

```bash
# On EC2, check if container is running
docker ps

# Check port mapping
docker port ips-data-acquisition-api

# Check EC2 security group
# Ensure inbound rule allows TCP port 90 from your IP/0.0.0.0/0
```

---

## 🔐 Security Best Practices

### 1. Secrets Management
- ✅ Never commit secrets to Git
- ✅ Use GitHub Secrets for CI/CD
- ✅ Use AWS Secrets Manager for production

### 2. Network Security
- ✅ Limit port exposure
- ✅ Use VPC for database
- ✅ Enable HTTPS with reverse proxy (nginx)

### 3. Container Security
- ✅ Run as non-root user (optional enhancement)
- ✅ Scan images for vulnerabilities
- ✅ Keep base images updated

---

## 📦 Production Checklist

- [ ] ECR repository created
- [ ] EC2 instance provisioned with Docker
- [ ] RDS PostgreSQL database created
- [ ] Security groups configured (port 90, SSH)
- [ ] GitHub secrets configured
- [ ] SSH key added to GitHub secrets
- [ ] Database migrations run
- [ ] Test deployment with workflow_dispatch
- [ ] Verify API accessible on port 90
- [ ] Set up monitoring/alerts

---

## 🔄 Continuous Deployment Flow

```
┌─────────────┐
│ Push to main│
└──────┬──────┘
       │
       ▼
┌─────────────────────┐
│ GitHub Actions      │
│ 1. Build Docker     │
│ 2. Push to ECR      │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│ Deploy to EC2       │
│ 1. Pull from ECR    │
│ 2. Restart (port 90)│
└─────────────────────┘
       │
       ▼
   🎉 Live on port 90!
```

---

## 📖 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [AWS ECR Guide](https://docs.aws.amazon.com/ecr/)
- [GitHub Actions](https://docs.github.com/en/actions)

---

**Port Configuration:** 90 (Production) ✅  
**Database:** PostgreSQL 16 ✅  
**CI/CD:** GitHub Actions ✅  
**Deployment:** AWS ECR + EC2 ✅

