# This file contains values that are the same across all environments
# For environment-specific values, refer to /env/<env_name>/environment.auto.tfvars
ip_restrictions=[{
  ip_address = "20.253.5.176/28"
  name = "github_largerunner"
}, {
  ip_address = "20.120.143.248/29"
  name = "github_largerunner"
}]
developer_ad_group_name="SEC-A-GreenForce-DevelopmentTeamAzure"
developer_ad_group_object_id="ffad55e0-f314-4852-9796-1d094a236e7b"
sendgrid_to_email="irs@energinet.dk"
sendgrid_from_email="info@datahub.dk"
