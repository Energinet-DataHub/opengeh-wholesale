# This file contains values that are the same across all environments
# For environment-specific values, refer to /env/<env_name>/environment.auto.tfvars
apim_publisher_email          = "datahub-platformteam@energinet.dk"
arm_tenant_id                 = "f7619355-6c67-4100-9a78-1847f30742e2"
developer_security_group_name = "SEC-G-Datahub-DevelopersAzure"
platform_security_group_name  = "SEC-G-Datahub-PlatformDevelopersAzure"
ad_group_directory_reader     = "SEC-A-DataHub-AAD-DirectoryReader"
ip_restrictions = [{
  ip_address = "20.253.5.176/28"
  name       = "github_largerunner"
  }, {
  ip_address = "20.120.143.248/29"
  name       = "github_largerunner"
}]
front_door_id                  = "fd2f4c53-e2e3-4bcf-85cf-11bcafa9220b"
