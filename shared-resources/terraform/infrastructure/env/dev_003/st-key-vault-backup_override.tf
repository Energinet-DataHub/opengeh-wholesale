# Temporary fix as the backup vault policy is broken and prevents deployment
module "st_key_vault_backup" {
  blob_storage_backup_policy = null
}
