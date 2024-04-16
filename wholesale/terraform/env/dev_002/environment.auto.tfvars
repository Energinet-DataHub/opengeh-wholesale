# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
databricks_vnet_address_space                      = "10.140.104.0/22"
databricks_private_subnet_address_prefix           = "10.140.104.0/24"
databricks_public_subnet_address_prefix            = "10.140.105.0/24"
databricks_private_endpoints_subnet_address_prefix = "10.140.106.0/24"
calculation_input_folder                           = "calculation_input_from_dev_001"
developer_ad_group_name                            = "SEC-A-GreenForce-DevelopmentTeamAzure"
omada_developers_security_group_name               = "SEC-G-Datahub-DevelopersAzure"
