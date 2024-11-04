# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
databricks_vnet_address_space                      = "10.142.100.0/22"
databricks_private_subnet_address_prefix           = "10.142.100.0/24"
databricks_public_subnet_address_prefix            = "10.142.101.0/24"
databricks_private_endpoints_subnet_address_prefix = "10.142.102.0/24"
platform_security_group_contributor_access         = true
platform_security_group_reader_access              = true
enable_audit_logs                                  = false
developer_security_group_name                      = "SEC-G-Datahub-DevelopersAzure"
