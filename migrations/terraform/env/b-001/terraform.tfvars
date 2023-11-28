databricks_vnet_address_space            = "10.140.98.0/23"
databricks_private_subnet_address_prefix = "10.140.98.0/24"
databricks_public_subnet_address_prefix  = "10.140.99.0/24"
vm_user_name                             = "dh3dpla"                            # Temporarily added to support VM in B-001
virtual_network_resource_group_name      = "rg-platformnetwork-preprod-we-001"  # Temporarily added to support VM in B-001
virtual_network_name                     = "vnet-datahub-preprod-we-001"        # Temporarily added to support VM in B-001: Value from dh3-automation "AZURE_VIRTUAL_NETWORK_NAME"
agents_address_space                     = "10.141.2.128/28"                    # Temporarily added to support VM in B-001: Value from dh3-automation "AZURE_COPY_AGENTS_ADDRESS_SPACE"