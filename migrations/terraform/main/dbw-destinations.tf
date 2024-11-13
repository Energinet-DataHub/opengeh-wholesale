# An email destination for migration
resource "databricks_notification_destination" "migration_destination" {
  provider = databricks.dbw
  display_name = "Migration Destination"
  config {
    email {
      addresses = [var.alert_email_address]
    }
  }
}

module "kvs_databricks_notification_destination_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-notification-destination-id"
  value        = resource.databricks_notification_destination.migration_destination.id
  key_vault_id = module.kv_internal.id
}
