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
}]
