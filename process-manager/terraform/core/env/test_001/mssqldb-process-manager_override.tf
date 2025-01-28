module "mssqldb_process_manager" {
  security_groups = concat(local.pim_security_group_rules_001, local.developer_security_group_rules_001_dev_test)
  max_size_gb                 = 1020
  sku_name                    = "GP_S_Gen5_40" # General Purpose (GP) - serverless compute (S) - standard series (Gen5) - max vCores (<number>) : https://learn.microsoft.com/en-us/azure/azure-sql/database/resource-limits-vcore-single-databases?view=azuresql#gen5-hardware-part-1-1
}
