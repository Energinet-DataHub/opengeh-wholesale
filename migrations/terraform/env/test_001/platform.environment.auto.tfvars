# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/platform.auto.tfvars
databricks_vnet_address_space                      = "10.142.100.0/22"
databricks_private_subnet_address_prefix           = "10.142.100.0/24"
databricks_public_subnet_address_prefix            = "10.142.101.0/24"
databricks_private_endpoints_subnet_address_prefix = "10.142.102.0/24"
pim_contributor_group_name                         = "SEC-A-Datahub-Test-001-Contributor"
developer_security_group_reader_access             = true
