# This file contains values that are the same across all environments
# For environment-specific values, refer to /env/<env_name>/environment.auto.tfvars
ip_restrictions = [{
  ip_address = "20.253.5.176/28"
  name       = "github_largerunner"
  }, {
  ip_address = "20.120.143.248/29"
  name       = "github_largerunner"
}]
github_username                           = "PerTHenriksen"
developers_security_group_object_id       = "ffad55e0-f314-4852-9796-1d094a236e7b"
omada_developers_security_group_object_id = "afb8f383-9e8d-40b9-8190-b9010e54a68b"
databricks_group_id                       = "427884581735458" # TODO: remove when we only have the OMADA group
databricks_group_id_migrations            = "141994886557178" # TODO: remove when we only have the OMADA group
databricks_developers_group_id            = "729028915538231"
databricks_migrations_group_id            = "371082943190175"
