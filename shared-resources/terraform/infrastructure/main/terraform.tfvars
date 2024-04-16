# This file contains values that are the same across all environments
# For environment-specific values, refer to /env/<env_name>/environment.auto.tfvars
apim_publisher_email                         = "datahub-platformteam@energinet.dk"
arm_tenant_id                                = "f7619355-6c67-4100-9a78-1847f30742e2"
omada_developers_security_group_object_id    = "afb8f383-9e8d-40b9-8190-b9010e54a68b"
developers_security_group_object_id          = "ffad55e0-f314-4852-9796-1d094a236e7b"
omada_platform_team_security_group_object_id = "8266fb6f-569a-4871-aa19-57be5ad05e4f"
platform_team_security_group_object_id       = "0524342a-e770-4b60-8d8d-d639d688b5b9"
ad_group_directory_reader                    = "SEC-A-DataHub-AAD-DirectoryReader"
ip_restrictions = [{
  ip_address = "20.253.5.176/28"
  name       = "github_largerunner"
  }, {
  ip_address = "20.120.143.248/29"
  name       = "github_largerunner"
}]
