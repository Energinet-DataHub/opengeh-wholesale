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
sendgrid_from_email="no-reply@datahub.dk"
sendgrid_bcc_email="webformlog@energinet.dk"
cvr_username="energinet.dk_CVR_I_SKYEN"
