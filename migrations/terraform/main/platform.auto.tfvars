# This file contains values that are the same across all environments
# For environment-specific values, refer to /env/<env_name>/environment.auto.tfvars
ip_restrictions = [{
  ip_address = "20.253.5.176/28"
  name       = "github_largerunner"
  }, {
  ip_address = "20.120.143.248/29"
  name       = "github_largerunner"
}]
github_username                = "PerTHenriksen"
platform_security_group_name   = "SEC-G-Datahub-PlatformDevelopersAzure"
databricks_developers_group_id = "729028915538231"
databricks_migrations_group_id = "371082943190175"
migration_group_name           = "SEC-G-DataHub-Migration"
