# This file contains values that are the same across all environments
# For environment-specific values, refer to /env/<env_name>/platform.environment.auto.tfvars
ip_restrictions = [{
  ip_address = "20.253.5.176/28"
  name       = "github_largerunner"
  }, {
  ip_address = "20.120.143.248/29"
  name       = "github_largerunner"
}]
tenant_id                    = "f7619355-6c67-4100-9a78-1847f30742e2"
platform_security_group_name = "SEC-G-Datahub-PlatformDevelopersAzure"
