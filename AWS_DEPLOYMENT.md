# AWS High Availability Deployment Guide

This guide covers deploying the IPS Data Acquisition API to AWS with high availability, auto-scaling, and disaster recovery.

## Architecture Overview

### Production Architecture (High Availability)

```
                                    ┌─────────────────┐
                                    │   Route 53      │
                                    │   (DNS)         │
                                    └────────┬────────┘
                                             │
                                             ▼
                              ┌──────────────────────────┐
                              │  Application Load        │
                              │  Balancer (ALB)          │
                              │  Multi-AZ                │
                              └───────┬──────────────────┘
                                      │
                 ┌────────────────────┼────────────────────┐
                 │                    │                    │
                 ▼                    ▼                    ▼
        ┌────────────────┐   ┌────────────────┐   ┌────────────────┐
        │  ECS Service    │   │  ECS Service    │   │  ECS Service    │
        │  Container 1    │   │  Container 2    │   │  Container 3    │
        │  (AZ-1a)        │   │  (AZ-1b)        │   │  (AZ-1c)        │
        └────────────────┘   └────────────────┘   └────────────────┘
                 │                    │                    │
                 └────────────────────┼────────────────────┘
                                      │
                                      ▼
                          ┌────────────────────────┐
                          │   Amazon RDS           │
                          │   PostgreSQL           │
                          │   ├── Primary (AZ-1a)  │
                          │   └── Standby (AZ-1b)  │
                          │   Multi-AZ Deployment  │
                          └────────────────────────┘
                                      │
                                      ▼
                          ┌────────────────────────┐
                          │  Automated Backups     │
                          │  S3 Bucket             │
                          │  Cross-Region Replica  │
                          └────────────────────────┘
```

## AWS Services Used

| Service | Purpose | Configuration |
|---------|---------|---------------|
| **ECS (Fargate)** | Container orchestration | Auto-scaling, multi-AZ |
| **ALB** | Load balancing | Health checks, SSL termination |
| **RDS PostgreSQL** | Database | Multi-AZ, automated backups |
| **ECR** | Docker registry | Private registry for images |
| **Route 53** | DNS | Health checks, failover routing |
| **CloudWatch** | Monitoring & Logs | Metrics, alarms, log aggregation |
| **Secrets Manager** | Secrets storage | JWT keys, DB passwords |
| **S3** | Backup storage | Cross-region replication |
| **VPC** | Networking | Private subnets, security groups |

## Prerequisites

### 1. AWS Account Setup
- AWS account with appropriate permissions
- AWS CLI installed and configured
- IAM user with deployment permissions

### 2. GitHub Secrets Configuration

Navigate to **Repository → Settings → Secrets and variables → Actions** and add:

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `AWS_REGION` | AWS region | `ap-south-1` |
| `AWS_ACCOUNT_ID` | AWS account ID | `123456789012` |
| `ECR_REPOSITORY` | ECR repository name | `ips-data-acquisition-api` |
| `DB_CONNECTION_STRING` | Production database connection | `Host=db.xxx.rds.amazonaws.com;Port=5432;...` |
| `JWT_SECRET_KEY` | JWT signing key (64 chars) | `YGvxiN18CaPGW...` |
| `ADMIN_VERIFICATION_KEY` | Admin security key | `AdminKey_2024_...` |
| `AWS_ACCESS_KEY_ID` | AWS access key | From IAM user |
| `AWS_SECRET_ACCESS_KEY` | AWS secret key | From IAM user |
| `EC2_HOST` | EC2 instance IP | `54.123.45.67` |
| `EC2_USER` | SSH username | `ubuntu` |
| `EC2_SSH_KEY` | SSH private key | `-----BEGIN RSA...` |

## Step-by-Step Deployment

### Phase 1: Network Setup (VPC)

#### 1.1 Create VPC
```bash
aws ec2 create-vpc \
  --cidr-block 10.0.0.0/16 \
  --tag-specifications 'ResourceType=vpc,Tags=[{Key=Name,Value=ips-vpc}]'
```

#### 1.2 Create Subnets (Multi-AZ)
```bash
# Public Subnet AZ-1a
aws ec2 create-subnet \
  --vpc-id vpc-xxxxx \
  --cidr-block 10.0.1.0/24 \
  --availability-zone ap-south-1a

# Public Subnet AZ-1b
aws ec2 create-subnet \
  --vpc-id vpc-xxxxx \
  --cidr-block 10.0.2.0/24 \
  --availability-zone ap-south-1b

# Private Subnet AZ-1a (for RDS)
aws ec2 create-subnet \
  --vpc-id vpc-xxxxx \
  --cidr-block 10.0.10.0/24 \
  --availability-zone ap-south-1a

# Private Subnet AZ-1b (for RDS)
aws ec2 create-subnet \
  --vpc-id vpc-xxxxx \
  --cidr-block 10.0.11.0/24 \
  --availability-zone ap-south-1b
```

#### 1.3 Security Groups
```bash
# Application security group
aws ec2 create-security-group \
  --group-name ips-api-sg \
  --description "IPS API security group" \
  --vpc-id vpc-xxxxx

# Allow HTTP/HTTPS from anywhere
aws ec2 authorize-security-group-ingress \
  --group-id sg-xxxxx \
  --protocol tcp --port 80 --cidr 0.0.0.0/0

aws ec2 authorize-security-group-ingress \
  --group-id sg-xxxxx \
  --protocol tcp --port 443 --cidr 0.0.0.0/0

# Database security group
aws ec2 create-security-group \
  --group-name ips-db-sg \
  --description "IPS Database security group" \
  --vpc-id vpc-xxxxx

# Allow PostgreSQL from API security group only
aws ec2 authorize-security-group-ingress \
  --group-id sg-db-xxxxx \
  --protocol tcp --port 5432 \
  --source-group sg-api-xxxxx
```

### Phase 2: Database Setup (RDS Multi-AZ)

#### 2.1 Create DB Subnet Group
```bash
aws rds create-db-subnet-group \
  --db-subnet-group-name ips-db-subnet-group \
  --db-subnet-group-description "IPS Database subnet group" \
  --subnet-ids subnet-private-1a subnet-private-1b
```

#### 2.2 Create RDS Instance (Multi-AZ)
```bash
aws rds create-db-instance \
  --db-instance-identifier ips-database \
  --db-instance-class db.t3.medium \
  --engine postgres \
  --engine-version 15.4 \
  --master-username postgres \
  --master-user-password YourStrongPassword \
  --allocated-storage 100 \
  --storage-type gp3 \
  --storage-encrypted \
  --multi-az \
  --db-subnet-group-name ips-db-subnet-group \
  --vpc-security-group-ids sg-db-xxxxx \
  --backup-retention-period 7 \
  --preferred-backup-window "03:00-04:00" \
  --preferred-maintenance-window "Mon:04:00-Mon:05:00" \
  --enable-performance-insights \
  --publicly-accessible false
```

**Key Settings:**
- **Multi-AZ**: Automatic failover to standby in different AZ
- **Storage**: 100GB SSD with auto-scaling enabled
- **Backups**: 7-day retention, daily automated backups
- **Encryption**: At-rest encryption enabled
- **Performance Insights**: Query performance monitoring

#### 2.3 Create Read Replica (Optional - for scaling)
```bash
aws rds create-db-instance-read-replica \
  --db-instance-identifier ips-database-read-replica \
  --source-db-instance-identifier ips-database \
  --db-instance-class db.t3.medium \
  --publicly-accessible false
```

### Phase 3: Container Registry (ECR)

#### 3.1 Create ECR Repository
```bash
aws ecr create-repository \
  --repository-name ips-data-acquisition-api \
  --image-scanning-configuration scanOnPush=true \
  --encryption-configuration encryptionType=AES256
```

#### 3.2 Set Lifecycle Policy (Cleanup old images)
```bash
aws ecr put-lifecycle-policy \
  --repository-name ips-data-acquisition-api \
  --lifecycle-policy-text '{
    "rules": [{
      "rulePriority": 1,
      "description": "Keep last 10 images",
      "selection": {
        "tagStatus": "any",
        "countType": "imageCountMoreThan",
        "countNumber": 10
      },
      "action": { "type": "expire" }
    }]
  }'
```

### Phase 4: ECS Cluster Setup

#### 4.1 Create ECS Cluster
```bash
aws ecs create-cluster \
  --cluster-name ips-api-cluster \
  --capacity-providers FARGATE FARGATE_SPOT \
  --default-capacity-provider-strategy \
    capacityProvider=FARGATE,weight=1 \
    capacityProvider=FARGATE_SPOT,weight=4
```

#### 4.2 Create Task Definition
```json
{
  "family": "ips-api-task",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "containerDefinitions": [{
    "name": "ips-api",
    "image": "123456789012.dkr.ecr.ap-south-1.amazonaws.com/ips-data-acquisition-api:latest",
    "portMappings": [{
      "containerPort": 5000,
      "protocol": "tcp"
    }],
    "environment": [
      {"name": "ASPNETCORE_ENVIRONMENT", "value": "Production"}
    ],
    "secrets": [
      {
        "name": "ConnectionStrings__Default",
        "valueFrom": "arn:aws:secretsmanager:region:account-id:secret:db-connection"
      },
      {
        "name": "JwtSettings__SecretKey",
        "valueFrom": "arn:aws:secretsmanager:region:account-id:secret:jwt-secret"
      }
    ],
    "logConfiguration": {
      "logDriver": "awslogs",
      "options": {
        "awslogs-group": "/ecs/ips-api",
        "awslogs-region": "ap-south-1",
        "awslogs-stream-prefix": "api"
      }
    },
    "healthCheck": {
      "command": ["CMD-SHELL", "curl -f http://localhost:5000/health || exit 1"],
      "interval": 30,
      "timeout": 5,
      "retries": 3
    }
  }],
  "executionRoleArn": "arn:aws:iam::account-id:role/ecsTaskExecutionRole",
  "taskRoleArn": "arn:aws:iam::account-id:role/ecsTaskRole"
}
```

#### 4.3 Create Application Load Balancer
```bash
aws elbv2 create-load-balancer \
  --name ips-api-alb \
  --subnets subnet-public-1a subnet-public-1b \
  --security-groups sg-alb-xxxxx \
  --scheme internet-facing \
  --type application
```

#### 4.4 Create Target Group
```bash
aws elbv2 create-target-group \
  --name ips-api-targets \
  --protocol HTTP \
  --port 5000 \
  --vpc-id vpc-xxxxx \
  --target-type ip \
  --health-check-enabled \
  --health-check-path /health \
  --health-check-interval-seconds 30 \
  --healthy-threshold-count 2 \
  --unhealthy-threshold-count 3
```

#### 4.5 Create ECS Service with Auto-Scaling
```bash
aws ecs create-service \
  --cluster ips-api-cluster \
  --service-name ips-api-service \
  --task-definition ips-api-task:1 \
  --desired-count 3 \
  --launch-type FARGATE \
  --platform-version LATEST \
  --network-configuration "awsvpcConfiguration={
    subnets=[subnet-public-1a,subnet-public-1b,subnet-public-1c],
    securityGroups=[sg-api-xxxxx],
    assignPublicIp=ENABLED
  }" \
  --load-balancers "targetGroupArn=arn:aws:elasticloadbalancing:...,containerName=ips-api,containerPort=5000" \
  --health-check-grace-period-seconds 60 \
  --deployment-configuration "maximumPercent=200,minimumHealthyPercent=100"
```

#### 4.6 Configure Auto-Scaling
```bash
# Register scalable target
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --resource-id service/ips-api-cluster/ips-api-service \
  --scalable-dimension ecs:service:DesiredCount \
  --min-capacity 2 \
  --max-capacity 10

# CPU-based scaling policy
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/ips-api-cluster/ips-api-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name cpu-scale-up \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration '{
    "TargetValue": 70.0,
    "PredefinedMetricSpecification": {
      "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
    },
    "ScaleOutCooldown": 60,
    "ScaleInCooldown": 300
  }'

# Memory-based scaling policy
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/ips-api-cluster/ips-api-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name memory-scale-up \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration '{
    "TargetValue": 80.0,
    "PredefinedMetricSpecification": {
      "PredefinedMetricType": "ECSServiceAverageMemoryUtilization"
    },
    "ScaleOutCooldown": 60,
    "ScaleInCooldown": 300
  }'
```

## CI/CD Pipeline (GitHub Actions)

### Current Setup (EC2 Simple Deployment)

The existing `.github/workflows/deploy.yml` deploys to a single EC2 instance. This works for development but **not for high availability**.

### Workflow Flow

1. **Build Phase**
   - Checkout code
   - Replace config placeholders with GitHub Secrets
   - Build Docker image
   - Push to ECR

2. **Deploy Phase**
   - Prepare docker-compose file
   - SCP files to EC2
   - SSH to EC2 and pull latest image
   - Restart containers

### Migration to ECS (High Availability)

To migrate to ECS, update the deploy job:

```yaml
deploy:
  needs: build-and-push
  runs-on: ubuntu-latest
  environment:
    name: Production
  
  steps:
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@v4
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-region: ${{ env.AWS_REGION }}
    
    - name: Deploy to ECS
      run: |
        aws ecs update-service \
          --cluster ips-api-cluster \
          --service ips-api-service \
          --force-new-deployment \
          --task-definition ips-api-task:latest
        
        echo "Waiting for service to stabilize..."
        aws ecs wait services-stable \
          --cluster ips-api-cluster \
          --services ips-api-service
        
        echo "Deployment complete!"
```

## High Availability Features

### 1. **Multi-AZ Deployment**
- **API Containers**: Deployed across 3 availability zones
- **Database**: Primary + Standby in different AZs
- **Load Balancer**: Multi-AZ by default

**Benefits:**
- Survives entire availability zone failure
- Zero downtime during AZ outages
- Automatic failover (RDS: <2 minutes)

### 2. **Auto-Scaling**

**Scaling Triggers:**
- CPU utilization > 70% → Scale out
- Memory utilization > 80% → Scale out
- Request count > 1000/min → Scale out
- CPU/Memory normalized → Scale in

**Configuration:**
- Min instances: 2 (always running)
- Max instances: 10 (during peak load)
- Scale-out cooldown: 60 seconds
- Scale-in cooldown: 300 seconds (prevent flapping)

### 3. **Load Balancing**

**Health Checks:**
- Endpoint: `GET /health` (implement this endpoint)
- Interval: 30 seconds
- Timeout: 5 seconds
- Healthy threshold: 2 consecutive successes
- Unhealthy threshold: 3 consecutive failures

**Traffic Distribution:**
- Round-robin across healthy targets
- Sticky sessions: Disabled (stateless API)
- Connection draining: 60 seconds

### 4. **Database Resilience**

**Multi-AZ Configuration:**
- Synchronous replication to standby
- Automatic failover on primary failure
- Failover time: ~60-120 seconds
- DNS automatically updated to standby

**Backup Strategy:**
- Automated daily backups (3 AM UTC)
- Retention: 7 days
- Point-in-time recovery (PITR): Last 7 days
- Manual snapshots before major changes

**Read Replicas** (for scaling reads):
- Asynchronous replication
- Can have up to 5 read replicas
- Use for reporting queries
- Promote to primary if needed

### 5. **Disaster Recovery**

**RTO (Recovery Time Objective): 15 minutes**
**RPO (Recovery Point Objective): 5 minutes**

**DR Procedures:**

1. **Database Failure**
   - Multi-AZ: Automatic failover (2 min)
   - Complete failure: Restore from backup (10 min)

2. **Region Failure** (future enhancement)
   - Cross-region RDS replica
   - Update Route 53 to failover region
   - Estimated RTO: 30 minutes

3. **Complete Outage**
   - Restore from S3 backup
   - Redeploy infrastructure via Terraform
   - Estimated RTO: 2 hours

## Monitoring & Alerts

### CloudWatch Dashboards

Create dashboard monitoring:
```json
{
  "widgets": [
    {
      "type": "metric",
      "properties": {
        "metrics": [
          ["AWS/ECS", "CPUUtilization", {"stat": "Average"}],
          [".", "MemoryUtilization", {"stat": "Average"}]
        ],
        "period": 300,
        "stat": "Average",
        "region": "ap-south-1",
        "title": "ECS Service Metrics"
      }
    },
    {
      "type": "metric",
      "properties": {
        "metrics": [
          ["AWS/RDS", "DatabaseConnections"],
          [".", "CPUUtilization"],
          [".", "FreeStorageSpace"]
        ],
        "period": 300,
        "stat": "Average",
        "title": "RDS Metrics"
      }
    },
    {
      "type": "metric",
      "properties": {
        "metrics": [
          ["AWS/ApplicationELB", "TargetResponseTime"],
          [".", "RequestCount"],
          [".", "HTTPCode_Target_4XX_Count"],
          [".", "HTTPCode_Target_5XX_Count"]
        ],
        "period": 60,
        "stat": "Sum",
        "title": "ALB Metrics"
      }
    }
  ]
}
```

### CloudWatch Alarms

```bash
# High error rate alarm
aws cloudwatch put-metric-alarm \
  --alarm-name ips-api-high-error-rate \
  --alarm-description "Alert when error rate exceeds 5%" \
  --metric-name HTTPCode_Target_5XX_Count \
  --namespace AWS/ApplicationELB \
  --statistic Sum \
  --period 300 \
  --threshold 50 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2 \
  --alarm-actions arn:aws:sns:region:account:ips-alerts

# High latency alarm
aws cloudwatch put-metric-alarm \
  --alarm-name ips-api-high-latency \
  --metric-name TargetResponseTime \
  --namespace AWS/ApplicationELB \
  --statistic Average \
  --period 300 \
  --threshold 1.0 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2

# Database CPU alarm
aws cloudwatch put-metric-alarm \
  --alarm-name ips-db-high-cpu \
  --metric-name CPUUtilization \
  --namespace AWS/RDS \
  --statistic Average \
  --period 300 \
  --threshold 80 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2
```

### SNS Topic for Alerts
```bash
aws sns create-topic --name ips-api-alerts
aws sns subscribe \
  --topic-arn arn:aws:sns:region:account:ips-api-alerts \
  --protocol email \
  --notification-endpoint admin@yourcompany.com
```

## Cost Optimization

### Current EC2 Setup (Monthly Estimate)
- **EC2 t3.medium**: $30/month
- **RDS db.t3.micro**: $15/month
- **Storage**: $10/month
- **Data transfer**: $5-20/month
- **Total**: ~$60-75/month

### High Availability ECS Setup (Monthly Estimate)
- **ECS Fargate** (3 tasks, 0.5 vCPU, 1GB): $35/month
- **ALB**: $20/month
- **RDS db.t3.medium Multi-AZ**: $130/month
- **RDS storage** (100GB GP3): $12/month
- **ECR storage**: $5/month
- **Data transfer**: $10-30/month
- **CloudWatch logs**: $10/month
- **Total**: ~$220-250/month

### Cost Optimization Strategies

1. **Use Spot Instances**
   - Fargate Spot: 70% cheaper than regular Fargate
   - Configure capacity provider: 80% Spot, 20% On-Demand

2. **Auto-Scaling**
   - Scale down during low traffic (nights, weekends)
   - Min instances: 1-2 (save 50%)

3. **Reserved Instances**
   - RDS Reserved Instance: 40% discount (1-year commitment)
   - For stable, predictable workloads

4. **S3 Intelligent-Tiering**
   - Archive old backups to Glacier
   - Reduce storage costs by 90%

## Security Hardening

### 1. **Network Security**
- API in public subnets (behind ALB)
- Database in private subnets (no internet access)
- Security groups with minimal permissions
- NACLs for additional layer

### 2. **Secrets Management**

**Migrate from appsettings to AWS Secrets Manager:**

```bash
# Store JWT secret
aws secretsmanager create-secret \
  --name ips/jwt-secret \
  --secret-string "YourJWTSecretKey64CharactersLong"

# Store DB connection
aws secretsmanager create-secret \
  --name ips/db-connection \
  --secret-string "Host=db.xxx.rds.amazonaws.com;Port=5432;..."

# Store admin key
aws secretsmanager create-secret \
  --name ips/admin-verification-key \
  --secret-string "YourAdminSecurityKey"
```

**Update Task Definition:**
```json
"secrets": [
  {
    "name": "ConnectionStrings__Default",
    "valueFrom": "arn:aws:secretsmanager:region:account:secret:ips/db-connection"
  },
  {
    "name": "JwtSettings__SecretKey",
    "valueFrom": "arn:aws:secretsmanager:region:account:secret:ips/jwt-secret"
  }
]
```

### 3. **SSL/TLS**

**Option A: AWS Certificate Manager (Free)**
```bash
# Request certificate
aws acm request-certificate \
  --domain-name api.yourdomain.com \
  --validation-method DNS

# Add to ALB listener
aws elbv2 create-listener \
  --load-balancer-arn arn:aws:elasticloadbalancing:... \
  --protocol HTTPS \
  --port 443 \
  --certificates CertificateArn=arn:aws:acm:... \
  --default-actions Type=forward,TargetGroupArn=arn:...
```

**Option B: Let's Encrypt**
- Use Certbot in container
- Auto-renewal via cron job

### 4. **WAF (Web Application Firewall)**

```bash
# Create WAF WebACL
aws wafv2 create-web-acl \
  --name ips-api-waf \
  --scope REGIONAL \
  --default-action Allow={} \
  --rules '[{
    "Name": "RateLimitRule",
    "Priority": 1,
    "Statement": {
      "RateBasedStatement": {
        "Limit": 2000,
        "AggregateKeyType": "IP"
      }
    },
    "Action": {"Block": {}},
    "VisibilityConfig": {
      "SampledRequestsEnabled": true,
      "CloudWatchMetricsEnabled": true,
      "MetricName": "RateLimitRule"
    }
  }]'

# Associate with ALB
aws wafv2 associate-web-acl \
  --web-acl-arn arn:aws:wafv2:... \
  --resource-arn arn:aws:elasticloadbalancing:...
```

## Database Optimization

### 1. **Indexing Strategy**

Already implemented in code:
- `sessions`: (user_id), (status), (start_timestamp)
- `button_presses`: (session_id), (user_id), (timestamp)
- `imu_data`: (session_id, timestamp), (user_id)
- `users`: (phone_number - unique)
- `refresh_tokens`: (token - unique), (user_id), (expires_at)

### 2. **Connection Pooling**

Add to connection string:
```
Host=...;Minimum Pool Size=10;Maximum Pool Size=100;
```

### 3. **Query Optimization**

Monitor slow queries:
```sql
-- Enable pg_stat_statements
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

-- Find slow queries
SELECT query, mean_exec_time, calls 
FROM pg_stat_statements 
ORDER BY mean_exec_time DESC 
LIMIT 10;
```

### 4. **Partitioning** (for large datasets)

Partition `imu_data` by month:
```sql
CREATE TABLE imu_data_2024_10 PARTITION OF imu_data
FOR VALUES FROM ('2024-10-01') TO ('2024-11-01');
```

## Backup & Recovery

### Automated Backups

**RDS Automated Backups:**
- Daily snapshots at 3 AM UTC
- 7-day retention
- Encrypted at rest
- Cross-region copy enabled

### Manual Backup
```bash
aws rds create-db-snapshot \
  --db-instance-identifier ips-database \
  --db-snapshot-identifier ips-db-manual-$(date +%Y%m%d)
```

### Restore Procedure

**From Automated Backup:**
```bash
aws rds restore-db-instance-to-point-in-time \
  --source-db-instance-identifier ips-database \
  --target-db-instance-identifier ips-database-restored \
  --restore-time 2024-10-20T10:00:00Z
```

**From Snapshot:**
```bash
aws rds restore-db-instance-from-db-snapshot \
  --db-instance-identifier ips-database-restored \
  --db-snapshot-identifier ips-db-manual-20241020
```

## Zero-Downtime Deployment

### Blue-Green Deployment Strategy

1. **Green Environment**: Deploy new version to new ECS task
2. **Health Check**: Wait for green tasks to be healthy
3. **Traffic Shift**: ALB gradually shifts traffic (10% → 50% → 100%)
4. **Monitor**: Watch error rates and latency
5. **Complete**: Decommission blue tasks
6. **Rollback**: If issues, shift traffic back to blue

### Rolling Update Strategy (Current)

```yaml
deploymentConfiguration:
  maximumPercent: 200      # Can temporarily have 2x capacity
  minimumHealthyPercent: 100  # Always maintain full capacity
```

**Process:**
1. Start new task with updated image
2. Wait for health check to pass
3. Register with load balancer
4. Start receiving traffic
5. Drain connections from old task
6. Terminate old task
7. Repeat for remaining tasks

## Performance Tuning

### Application Settings

**Program.cs Kestrel Configuration:**
```csharp
options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(60);
options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
```

### Database Connection Pool
```
Minimum Pool Size=10;Maximum Pool Size=100;Connection Lifetime=300;
```

### ECS Task Resources

**Small Load** (< 100 users):
- CPU: 0.5 vCPU
- Memory: 1 GB
- Tasks: 2-3

**Medium Load** (100-500 users):
- CPU: 1 vCPU
- Memory: 2 GB
- Tasks: 3-5

**High Load** (500+ users):
- CPU: 2 vCPU
- Memory: 4 GB
- Tasks: 5-10

## Disaster Recovery Plan

### Scenario 1: Single Container Failure
- **Detection**: Health check fails
- **Action**: ECS automatically replaces task
- **Impact**: None (other containers handle traffic)
- **RTO**: 2 minutes

### Scenario 2: AZ Failure
- **Detection**: All tasks in AZ unreachable
- **Action**: ALB routes to other AZs, ECS starts new tasks
- **Impact**: Reduced capacity temporarily
- **RTO**: 5 minutes

### Scenario 3: Database Primary Failure
- **Detection**: Connection errors
- **Action**: RDS automatic failover to standby
- **Impact**: 60-120 seconds downtime
- **RTO**: 2 minutes

### Scenario 4: Complete Region Failure
- **Detection**: Manual (monitoring alerts)
- **Action**: Manual failover to DR region
- **Steps**:
  1. Update Route 53 to DR region
  2. Promote read replica to primary
  3. Deploy containers in DR region
- **RTO**: 30 minutes (manual process)
- **RPO**: 5 minutes (replication lag)

## Cost vs. Availability Comparison

| Setup | Availability | Monthly Cost | Use Case |
|-------|--------------|--------------|----------|
| Single EC2 | 95% | $60 | Development/Testing |
| EC2 + Multi-AZ RDS | 99% | $150 | Small Production |
| ECS Multi-AZ + RDS Multi-AZ | 99.9% | $250 | Production |
| ECS Multi-Region | 99.99% | $500+ | Mission-Critical |

## Maintenance Windows

### Recommended Schedule
- **Minor Updates**: Rolling updates (zero downtime)
- **Major Updates**: Sunday 2-4 AM UTC
- **Database Maintenance**: RDS auto-applies during maintenance window

### Update Checklist
1. ✅ Test changes in staging environment
2. ✅ Take manual database snapshot
3. ✅ Create git tag for version
4. ✅ Deploy during low-traffic period
5. ✅ Monitor error rates and latency
6. ✅ Keep previous Docker image for rollback
7. ✅ Update documentation

## Infrastructure as Code (Future)

### Terraform Example
```hcl
resource "aws_ecs_service" "ips_api" {
  name            = "ips-api-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.ips_api.arn
  desired_count   = 3
  
  load_balancer {
    target_group_arn = aws_lb_target_group.ips_api.arn
    container_name   = "ips-api"
    container_port   = 5000
  }
  
  network_configuration {
    subnets         = var.public_subnet_ids
    security_groups = [aws_security_group.ips_api.id]
  }
}
```

## Troubleshooting Production Issues

### High CPU Usage
```bash
# Check current CPU usage
aws ecs describe-services \
  --cluster ips-api-cluster \
  --services ips-api-service

# View CloudWatch metrics
aws cloudwatch get-metric-statistics \
  --namespace AWS/ECS \
  --metric-name CPUUtilization \
  --start-time 2024-10-20T00:00:00Z \
  --end-time 2024-10-20T23:59:59Z \
  --period 3600 \
  --statistics Average
```

### Database Connection Pool Exhausted
- Increase max pool size
- Check for connection leaks
- Review long-running queries

### Deployment Failures
```bash
# Check service events
aws ecs describe-services \
  --cluster ips-api-cluster \
  --services ips-api-service \
  --query 'services[0].events[:5]'

# View container logs
aws logs tail /ecs/ips-api --follow
```

## Security Compliance

### Data Protection
- Encryption at rest (RDS, EBS)
- Encryption in transit (TLS 1.2+)
- No sensitive data in logs
- Password hashing (PBKDF2)

### Access Control
- IAM roles with least privilege
- MFA for AWS Console access
- Audit logs via CloudTrail
- Regular security reviews

### Compliance Standards
- GDPR: User data deletion on request
- HIPAA: Encryption and audit trails (if needed)
- SOC 2: Logging and monitoring

---

**Next Steps:**
1. Set up VPC and networking
2. Create RDS instance with Multi-AZ
3. Deploy ECS cluster with auto-scaling
4. Configure monitoring and alerts
5. Test failover scenarios
6. Document runbooks for operations team

