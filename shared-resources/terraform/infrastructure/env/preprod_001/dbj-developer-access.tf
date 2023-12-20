module "dbj" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/ccp-temp-pim-access?ref=v13"

  scope                    = data.azurerm_subscription.this.id
  developer_object_id      = "4b6a4911-0c21-4c94-a285-596cf66a4db2"
  writer_access            = false
  pim_reader_ad_group_name = "sec-a-datahub-preprod-001-database-reader"
}
