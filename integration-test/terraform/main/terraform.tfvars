# This file contains values that are the same across all environments
# For environment-specific values, refer to /env/<env_name>/environment.auto.tfvars
github_username                           = "PerTHenriksen"
omada_developers_security_group_object_id = "afb8f383-9e8d-40b9-8190-b9010e54a68b"
b2c_tenant_id                             = "72996b41-f6a7-44db-b070-65acc2fb7818"
b2c_client_id                             = "cc56e8e1-ebb7-4f19-b06e-506af77e5eae"
kv_variables = [{
  "name" : "AZURE-B2C-TENANT",
  "value" : "b2cusersintegrtest002.onmicrosoft.com"
  },
  {
    "name" : "AZURE-B2C-SPN-ID",
    "value" : "cc56e8e1-ebb7-4f19-b06e-506af77e5eae"
  },
  {
    "name" : "AZURE-B2C-BFF-APP-ID",
    "value" : "3648e1ec-a357-4684-ab21-65433642e5dd"  # TODO: Remove once TestCommon 6.0.0 has been adopted by all product teams
  },
  {
    "name" : "AZURE-B2C-BACKEND-APP-ID",
    "value" : "b215178c-7118-479b-bfdc-45cbe63cad9e"  # TODO: Remove once TestCommon 6.0.0 has been adopted by all product teams
  },
  {
    "name" : "AZURE-B2C-BACKEND-SPN-OBJECTID",
    "value" : "95c85cd8-89e4-4adf-9039-c93e6f629c40"  # TODO: Remove once TestCommon 6.0.0 has been adopted by all product teams
  },
  {
    "name" : "AZURE-B2C-BACKEND-APP-OBJECTID",
    "value" : "f6fd2a75-b6d8-462c-a8b3-23f5c6657dd2"  # TODO: Remove once TestCommon 6.0.0 has been adopted by all product teams
}]
