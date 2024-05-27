# This file contains values that are the same across all environments
# For environment-specific values, refer to /env/<env_name>/platform.auto.tfvars
ip_restrictions=[{
  ip_address = "20.253.5.176/28"
  name = "github_largerunner"
}, {
  ip_address = "20.120.143.248/29"
  name = "github_largerunner"
}]
sendgrid_from_email="no-reply@datahub.dk"
