#!/bin/bash
# Setup CloudWatch Logs Agent on EC2 for Docker Container Logs
# Run this on your EC2 instance

set -e

echo "Installing CloudWatch Logs Agent..."

# Install CloudWatch agent
sudo yum install -y amazon-cloudwatch-agent || sudo apt-get install -y amazon-cloudwatch-agent

# Create CloudWatch agent config
sudo tee /opt/aws/amazon-cloudwatch-agent/etc/cloudwatch-config.json > /dev/null <<EOF
{
  "logs": {
    "logs_collected": {
      "files": {
        "collect_list": [
          {
            "file_path": "/var/lib/docker/containers/*/*.log",
            "log_group_name": "/ecs/ips-api",
            "log_stream_name": "{instance_id}-docker",
            "timezone": "UTC"
          }
        ]
      }
    },
    "log_stream_name": "ips-api-{instance_id}"
  }
}
EOF

# Start CloudWatch agent
sudo /opt/aws/amazon-cloudwatch-agent/bin/amazon-cloudwatch-agent-ctl \
  -a fetch-config \
  -m ec2 \
  -s \
  -c file:/opt/aws/amazon-cloudwatch-agent/etc/cloudwatch-config.json

echo "CloudWatch Logs Agent configured!"
echo "Logs will appear in CloudWatch at: /ecs/ips-api"
echo ""
echo "Wait 2-3 minutes for logs to start appearing in CloudWatch"

