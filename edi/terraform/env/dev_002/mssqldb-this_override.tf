module "mssqldb_edi" {
  source                  = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=v13"

  developer_ad_group_name = var.developer_ad_group_name
  datawriter_access       = true
}
