terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 3.0"
    }
  }
}

provider "aws" {
  region = "eu-west-2"
}

terraform {
  backend "s3" {
    bucket         = "housing-pre-production-terraform-state"
    encrypt        = true
    region         = "eu-west-2"
    key            = "services/configuration-api/state"
    dynamodb_table = "housing-pre-production-terraform-state-lock"
  }
}

resource "aws_s3_bucket" "configuration" {
  bucket = "configuration-api-configurations-pre-production"
  tags = {
    Name        = "Configuration Api Bucket"
    Environment = var.environment_name
    Application = "MTFH Housing Pre-Production"
    TeamEmail   = "developementteam@hackney.gov.uk"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "enable_encryption" {
  bucket = aws_s3_bucket.configuration.id
  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_versioning" "versioning" {
  bucket = aws_s3_bucket.configuration.id
  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_public_access_block" "block_public_access" {
  bucket                  = aws_s3_bucket.configuration.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_ssm_parameter" "configurations" {
  name  = "/configuration-api/pre-production/bucket-name"
  type  = "String"
  value = aws_s3_bucket.configuration.id
}
