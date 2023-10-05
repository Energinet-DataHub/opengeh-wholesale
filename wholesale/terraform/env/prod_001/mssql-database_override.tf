module "mssqldb_wholesale" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=v13"

  developer_ad_group_name = var.developer_ad_group_name
}
