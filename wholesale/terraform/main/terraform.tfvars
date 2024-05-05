# This file contains values that are the same across all environments
# For environment-specific values, refer to /env/<env_name>/environment.auto.tfvars
ip_restrictions = [{
  ip_address = "20.253.5.176/28"
  name       = "github_largerunner"
  }, {
  ip_address = "20.120.143.248/29"
  name       = "github_largerunner"
}]
tenant_id                      = "f7619355-6c67-4100-9a78-1847f30742e2"
databricks_group_id            = "427884581735458" # TODO: remove when we only have the OMADA group
databricks_developers_group_id = "729028915538231"
